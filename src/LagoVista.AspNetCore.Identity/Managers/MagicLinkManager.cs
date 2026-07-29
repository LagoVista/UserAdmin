using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.REpos.Account;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class MagicLinkManager : IMagicLinkManager
    {
        private static readonly TimeSpan MagicLinkTtl = TimeSpan.FromMinutes(15);
        private static readonly TimeSpan ExchangeTtl = TimeSpan.FromMinutes(5);

        private readonly IUserManager _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IAuthenticationLogManager _authLogMgr;
        private readonly IMagicLinkAttemptStore _store;
        private readonly IAppConfig _appConfig;
        private readonly ISignInManager _signinManager;

        public MagicLinkManager(IUserManager userManager, IEmailSender emailSender, IAuthenticationLogManager authLogMgr, IMagicLinkAttemptStore store, ISignInManager signinManager, IAppConfig appConfig)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _signinManager = signinManager ?? throw new ArgumentNullException(nameof(signinManager));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        public async Task<InvokeResult> RequestSignInLinkAsync(MagicLinkRequest request, MagicLinkRequestContext context)
        {
            var result = await RequestSignInLinkAsyncForTesting(request, context);
            return result.ToInvokeResult();
        }

        public async Task<InvokeResult<string>> RequestSignInLinkAsyncForTesting(MagicLinkRequest request, MagicLinkRequestContext context)
        {
            if (request == null) return InvokeResult<string>.FromError("missing_request");

            var email = NormalizeEmail(request.Email);
            if (string.IsNullOrWhiteSpace(email)) return InvokeResult<string>.FromError("missing_email");

            var channel = NormalizeChannel(request.Channel);
            if (channel == null) return InvokeResult<string>.FromError("invalid_channel");

            await _authLogMgr.AddAsync(AuthLogTypes.MagicLinkRequested, userName: email, orgId: _appConfig.SystemOwnerOrg.Id, errors: "", extras: $"channel={channel}", redirectUri: request.ReturnUrl ?? "");

            var appUser = await _userManager.FindByEmailAsync(email);
            if (appUser == null)
                return InvokeResult<string>.Create(String.Empty);

            var nowUtc = DateTime.UtcNow;
            var rawCode = GenerateCode();
            var attempt = new MagicLinkAttempt
            {
                Id = Guid.NewGuid().ToString("N"),
                Email = email,
                UserId = appUser.Id,
                Channel = channel,
                Purpose = MagicLinkAttempt.Purpose_SignIn,
                CodeHash = Hash(rawCode),
                ExpiresAtUtc = nowUtc.Add(MagicLinkTtl),
                ConsumedAtUtc = null,
                ReturnUrl = request.ReturnUrl,
                IpAddress = context?.IpAddress,
                UserAgent = context?.UserAgent,
                CorrelationId = context?.CorrelationId
            };

            var create = await _store.CreateAsync(attempt);
            if (!create.Successful)
                return create.ToInvokeResult<string>();

            var userEh = ToEntityHeader(appUser);
            var send = await _emailSender.SendInBackgroundAsync(email, "Your sign-in link", BuildEmailBody(attempt, rawCode), _appConfig.SystemOwnerOrg, userEh);
            if (!send.Successful)
            {
                await _authLogMgr.AddAsync(AuthLogTypes.MagicLinkConsumeFailed, user: userEh, org: _appConfig.SystemOwnerOrg, errors: "email_send_failed", extras: $"channel={channel}", redirectUri: request.ReturnUrl ?? "", challengeId: attempt.Id);
                return InvokeResult<string>.Create(String.Empty);
            }

            await _authLogMgr.AddAsync(AuthLogTypes.MagicLinkSent, user: userEh, org: _appConfig.SystemOwnerOrg, errors: "", extras: $"channel={channel}", redirectUri: request.ReturnUrl ?? "", challengeId: attempt.Id);
            return InvokeResult<string>.Create(rawCode);
        }

        public async Task<InvokeResult<AuthenticationResponse>> ConsumeAsync(string code, MagicLinkConsumeContext context)
        {
            if (string.IsNullOrWhiteSpace(code))
                return InvokeResult<AuthenticationResponse>.FromError("missing_code");

            var channel = NormalizeChannel(context?.Channel);
            if (channel == null)
                return InvokeResult<AuthenticationResponse>.FromError("invalid_channel");

            var nowUtc = DateTime.UtcNow;
            var consume = await _store.TryConsumeAsync(Hash(code), nowUtc);
            if (!consume.Successful)
            {
                await _authLogMgr.AddAsync(AuthLogTypes.MagicLinkConsumeFailed, org: _appConfig.SystemOwnerOrg, errors: FirstErrorOrDefault(consume), extras: $"channel={channel}", redirectUri: context?.ReturnUrl ?? "");
                return InvokeResult<AuthenticationResponse>.FromErrors(consume.Errors.ToArray());
            }

            var attempt = consume.Result?.Attempt;
            if (attempt == null)
                return InvokeResult<AuthenticationResponse>.FromError("not_found");

            if (!string.Equals(attempt.Channel, channel, StringComparison.Ordinal))
                return InvokeResult<AuthenticationResponse>.FromError("channel_mismatch");

            var appUser = await _userManager.FindByIdAsync(attempt.UserId);
            if (appUser == null)
                return InvokeResult<AuthenticationResponse>.FromError("user_not_found");

            var userEh = ToEntityHeader(appUser);
            await _authLogMgr.AddAsync(AuthLogTypes.MagicLinkConsumed, user: userEh, org: _appConfig.SystemOwnerOrg, errors: "", extras: $"channel={channel}", redirectUri: context?.ReturnUrl ?? "", challengeId: attempt.Id);

            if (string.Equals(channel, MagicLinkAttempt.Channel_Mobile, StringComparison.Ordinal))
            {
                var exchangeCode = GenerateCode();
                var set = await _store.SetExchangeAsync(attempt.Id, Hash(exchangeCode), nowUtc.Add(ExchangeTtl), nowUtc);
                if (!set.Successful)
                    return InvokeResult<AuthenticationResponse>.FromErrors(set.Errors.ToArray());

                await _authLogMgr.AddAsync(AuthLogTypes.MagicLinkExchangeIssued, user: userEh, org: _appConfig.SystemOwnerOrg, errors: "", extras: $"ttlMinutes={(int)ExchangeTtl.TotalMinutes}", challengeId: attempt.Id);

                return InvokeResult<AuthenticationResponse>.Create(new AuthenticationResponse
                {
                    AuthenticationState = AuthenticationResponseState.Authenticated,
                    AuthenticationReasonCode = "magic_link_exchange_issued",
                    Provider = "magic-link",
                    RedirectPage = exchangeCode
                });
            }

            await _signinManager.SignInAsync(appUser, true);
            return await _signinManager.CompleteSignInToAppAsync(appUser);
        }

        public async Task<InvokeResult<AppUser>> ExchangeAsync(string exchangeCode, MagicLinkExchangeContext context)
        {
            if (string.IsNullOrWhiteSpace(exchangeCode))
                return InvokeResult<AppUser>.FromError("missing_exchange_code");

            var consume = await _store.TryConsumeExchangeAsync(Hash(exchangeCode), DateTime.UtcNow);
            if (!consume.Successful)
                return InvokeResult<AppUser>.FromErrors(consume.Errors.ToArray());

            var attempt = consume.Result?.Attempt;
            if (attempt == null)
                return InvokeResult<AppUser>.FromError("not_found");

            var appUser = await _userManager.FindByIdAsync(attempt.UserId);
            if (appUser == null)
                return InvokeResult<AppUser>.FromError("user_not_found");

            await _authLogMgr.AddAsync(AuthLogTypes.MagicLinkExchangeSucceeded, user: ToEntityHeader(appUser), org: _appConfig.SystemOwnerOrg, errors: "", extras: "", challengeId: attempt.Id);
            return InvokeResult<AppUser>.Create(appUser);
        }

        private string BuildEmailBody(MagicLinkAttempt attempt, string rawCode)
        {
            var webBase = GetWebURI().TrimEnd('/');
            var webLink = $"{webBase}/auth/magiclink/handle?code={Uri.EscapeDataString(rawCode)}";
            var mobileLink = $"nuviot:/auth/securelink/consume?code={Uri.EscapeDataString(rawCode)}";
            var ttlMinutes = (int)MagicLinkTtl.TotalMinutes;

            var sb = new StringBuilder();
            sb.AppendLine("<div><p>Use this link to sign in:</p>");
            sb.AppendLine($"<a href='{webLink}'>Sign In in your browser</a></div>");
            sb.AppendLine("<div><p>If you're signing in on a mobile device, you can also use this app link:</p>");
            sb.AppendLine($"<a href='{mobileLink}'>Sign In in your app</a></div>");
            sb.AppendLine($"This link expires in {ttlMinutes} minutes and can only be used once.");
            return sb.ToString();
        }

        private string GetWebURI()
        {
            var environment = _appConfig.WebAddress;
            if (_appConfig.WebAddress.ToLower().Contains("api"))
            {
                switch (_appConfig.Environment)
                {
                    case Environments.Development: environment = "https://dev.nuviot.com"; break;
                    case Environments.Testing: environment = "https://test.nuviot.com"; break;
                    case Environments.Beta: environment = "https://qa.nuviot.com"; break;
                    case Environments.Staging: environment = "https://stage.nuviot.com"; break;
                    case Environments.Production: environment = "https://www.nuviot.com"; break;
                }
            }

            if (environment.Contains("localhost"))
                environment = "http://localhost:4200";

            return environment;
        }

        private static string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return email.Trim().ToLowerInvariant();
        }

        private static string NormalizeChannel(string channel)
        {
            if (string.IsNullOrWhiteSpace(channel)) return null;
            channel = channel.Trim().ToLowerInvariant();
            if (string.Equals(channel, MagicLinkAttempt.Channel_Portal, StringComparison.Ordinal)) return MagicLinkAttempt.Channel_Portal;
            if (string.Equals(channel, MagicLinkAttempt.Channel_Mobile, StringComparison.Ordinal)) return MagicLinkAttempt.Channel_Mobile;
            return null;
        }

        private static string GenerateCode()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
                rng.GetBytes(bytes);

            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", string.Empty);
        }

        private static string FirstErrorOrDefault<T>(InvokeResult<T> result)
        {
            if (result?.Errors == null || result.Errors.Count == 0) return "error";
            return result.Errors[0].Message;
        }

        private static EntityHeader ToEntityHeader(AppUser user)
        {
            if (user == null) return null;
            return EntityHeader.Create(user.Id, user.UserName ?? user.Email ?? user.Id);
        }

        public string Hash(string secret)
        {
            if (string.IsNullOrWhiteSpace(secret))
                throw new ArgumentNullException(nameof(secret));

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(secret));
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
