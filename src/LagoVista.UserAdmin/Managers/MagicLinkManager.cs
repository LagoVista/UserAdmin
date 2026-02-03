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
     
        public MagicLinkManager(
            IUserManager userManager,
            IEmailSender emailSender,
            IAuthenticationLogManager authLogMgr,
            IMagicLinkAttemptStore store,
            IAppConfig appConfig)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        public async Task<InvokeResult> RequestSignInLinkAsync(MagicLinkRequest request, MagicLinkRequestContext context)
        {
            if (request == null) return InvokeResult.FromError("missing_request");

            var email = NormalizeEmail(request.Email);
            if (string.IsNullOrWhiteSpace(email)) return InvokeResult.FromError("missing_email");

            var channel = NormalizeChannel(request.Channel);
            if (channel == null) return InvokeResult.FromError("invalid_channel");

            // Always log the request (non-enumeration compatible).
            await _authLogMgr.AddAsync(
                AuthLogTypes.MagicLinkRequested,
                userName: email,
                orgId: _appConfig.SystemOwnerOrg.Id,
                errors: "",
                extras: $"channel={channel}",
                redirectUri: request.ReturnUrl ?? "");

            // Sign-in only: resolve existing user.
            var appUser = await _userManager.FindByEmailAsync(email);

            // Non-enumerating: return success even if not found.
            if (appUser == null)
            {
                return InvokeResult.Success;
            }

            var nowUtc = DateTime.UtcNow;
            var rawCode = GenerateCode();
            var codeHash = Hash(rawCode);

            var attempt = new MagicLinkAttempt
            {
                Id = Guid.NewGuid().ToString("N"),
                Email = email,
                UserId = appUser.Id,
                Channel = channel,
                Purpose = MagicLinkAttempt.Purpose_SignIn,
                CodeHash = codeHash,
                ExpiresAtUtc = nowUtc.Add(MagicLinkTtl),
                ConsumedAtUtc = null,
                ReturnUrl = request.ReturnUrl,
                IpAddress = context?.IpAddress,
                UserAgent = context?.UserAgent,
                CorrelationId = context?.CorrelationId
            };

            var create = await _store.CreateAsync(attempt);
            if (!create.Successful)
            {
                return create;
            }

            var subject = "Your sign-in link";
            var body = BuildEmailBody(attempt, rawCode);

            var orgEh = _appConfig.SystemOwnerOrg;
            var userEh = ToEntityHeader(appUser);

            var send = await _emailSender.SendInBackgroundAsync(email, subject, body, orgEh, userEh);
            if (!send.Successful)
            {
                await _authLogMgr.AddAsync(
                    AuthLogTypes.MagicLinkConsumeFailed,
                    user: userEh,
                    org: orgEh,
                    errors: "email_send_failed",
                    extras: $"channel={channel}",
                    redirectUri: request.ReturnUrl ?? "",
                    challengeId: attempt.Id);

                // Still return success to remain non-enumerating and avoid leaking mail delivery.
                return InvokeResult.Success;
            }

            await _authLogMgr.AddAsync(
                AuthLogTypes.MagicLinkSent,
                user: userEh,
                org: orgEh,
                errors: "",
                extras: $"channel={channel}",
                redirectUri: request.ReturnUrl ?? "",
                challengeId: attempt.Id);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<MagicLinkConsumeResponse>> ConsumeAsync(string code, MagicLinkConsumeContext context)
        {
            if (string.IsNullOrWhiteSpace(code))
                return InvokeResult<MagicLinkConsumeResponse>.FromError("missing_code");

            var channel = NormalizeChannel(context?.Channel);
            if (channel == null)
                return InvokeResult<MagicLinkConsumeResponse>.FromError("invalid_channel");

            var nowUtc = DateTime.UtcNow;
            var codeHash = Hash(code);

            var consume = await _store.TryConsumeAsync(codeHash, nowUtc);
            if (!consume.Successful)
            {
                await _authLogMgr.AddAsync(
                    AuthLogTypes.MagicLinkConsumeFailed,
                    org: _appConfig.SystemOwnerOrg,
                    errors: FirstErrorOrDefault(consume),
                    extras: $"channel={channel}",
                    redirectUri: context?.ReturnUrl ?? "");

                return InvokeResult<MagicLinkConsumeResponse>.FromErrors(consume.Errors.ToArray());
            }

            var attempt = consume.Result?.Attempt;
            if (attempt == null)
            {
                await _authLogMgr.AddAsync(
                    AuthLogTypes.MagicLinkConsumeFailed,
                    org: _appConfig.SystemOwnerOrg,
                    errors: "not_found",
                    extras: $"channel={channel}",
                    redirectUri: context?.ReturnUrl ?? "");

                return InvokeResult<MagicLinkConsumeResponse>.FromError("not_found");
            }

            if (!string.Equals(attempt.Channel, channel, StringComparison.Ordinal))
            {
                await _authLogMgr.AddAsync(
                    AuthLogTypes.MagicLinkConsumeFailed,
                    org: _appConfig.SystemOwnerOrg,
                    errors: "channel_mismatch",
                    extras: $"attemptChannel={attempt.Channel};channel={channel}",
                    redirectUri: context?.ReturnUrl ?? "",
                    challengeId: attempt.Id);

                return InvokeResult<MagicLinkConsumeResponse>.FromError("channel_mismatch");
            }

            var appUser = await _userManager.FindByIdAsync(attempt.UserId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(
                    AuthLogTypes.MagicLinkConsumeFailed,
                    org: _appConfig.SystemOwnerOrg,
                    errors: "user_not_found",
                    extras: $"channel={channel}",
                    challengeId: attempt.Id);

                return InvokeResult<MagicLinkConsumeResponse>.FromError("user_not_found");
            }

            var userEh = ToEntityHeader(appUser);
            var orgEh = _appConfig.SystemOwnerOrg;

            await _authLogMgr.AddAsync(
                AuthLogTypes.MagicLinkConsumed,
                user: userEh,
                org: orgEh,
                errors: "",
                extras: $"channel={channel}",
                redirectUri: context?.ReturnUrl ?? "",
                challengeId: attempt.Id);

            var response = new MagicLinkConsumeResponse
            {
                Attempt = attempt,
                ExchangeCode = null
            };

            if (string.Equals(channel, MagicLinkAttempt.Channel_Mobile, StringComparison.Ordinal))
            {
                var exchangeCode = GenerateCode();
                var exchangeHash = Hash(exchangeCode);
                var exchangeExpiresAtUtc = nowUtc.Add(ExchangeTtl);

                var set = await _store.SetExchangeAsync(attempt.Id, exchangeHash, exchangeExpiresAtUtc, nowUtc);
                if (!set.Successful)
                {
                    await _authLogMgr.AddAsync(
                        AuthLogTypes.MagicLinkExchangeFailed,
                        user: userEh,
                        org: orgEh,
                        errors: FirstErrorOrDefault(set),
                        extras: $"channel={channel}",
                        challengeId: attempt.Id);

                    return InvokeResult<MagicLinkConsumeResponse>.FromErrors(set.Errors.ToArray());
                }

                await _authLogMgr.AddAsync(
                    AuthLogTypes.MagicLinkExchangeIssued,
                    user: userEh,
                    org: orgEh,
                    errors: "",
                    extras: $"ttlMinutes={(int)ExchangeTtl.TotalMinutes}",
                    challengeId: attempt.Id);

                response.ExchangeCode = exchangeCode;
            }

            return InvokeResult<MagicLinkConsumeResponse>.Create(response);
        }

        public async Task<InvokeResult<AppUser>> ExchangeAsync(string exchangeCode, MagicLinkExchangeContext context)
        {
            if (string.IsNullOrWhiteSpace(exchangeCode))
                return InvokeResult<AppUser>.FromError("missing_exchange_code");

            var nowUtc = DateTime.UtcNow;
            var exchangeHash = Hash(exchangeCode);

            var consume = await _store.TryConsumeExchangeAsync(exchangeHash, nowUtc);
            if (!consume.Successful)
            {
                await _authLogMgr.AddAsync(
                    AuthLogTypes.MagicLinkExchangeFailed,
                    org: _appConfig.SystemOwnerOrg,
                    errors: FirstErrorOrDefault(consume),
                    extras: "",
                    challengeId: "");

                return InvokeResult<AppUser>.FromErrors(consume.Errors.ToArray());
            }

            var attempt = consume.Result?.Attempt;
            if (attempt == null)
            {
                await _authLogMgr.AddAsync(
                    AuthLogTypes.MagicLinkExchangeFailed,
                    org: _appConfig.SystemOwnerOrg,
                    errors: "not_found",
                    extras: "",
                    challengeId: "");

                return InvokeResult<AppUser>.FromError("not_found");
            }

            var appUser = await _userManager.FindByIdAsync(attempt.UserId);
            if (appUser == null)
            {
                await _authLogMgr.AddAsync(
                    AuthLogTypes.MagicLinkExchangeFailed,
                    org: _appConfig.SystemOwnerOrg,
                    errors: "user_not_found",
                    extras: "",
                    challengeId: attempt.Id);

                return InvokeResult<AppUser>.FromError("user_not_found");
            }

            var userEh = ToEntityHeader(appUser);
            var orgEh = _appConfig.SystemOwnerOrg;

            await _authLogMgr.AddAsync(
                AuthLogTypes.MagicLinkExchangeSucceeded,
                user: userEh,
                org: orgEh,
                errors: "",
                extras: "",
                challengeId: attempt.Id);

            return InvokeResult<AppUser>.Create(appUser);
        }

        private string BuildEmailBody(MagicLinkAttempt attempt, string rawCode)
        {
            var webBase = GetWebURI().TrimEnd('/');
            var webLink = $"{webBase}/auth/link/consume?code={Uri.EscapeDataString(rawCode)}";
            var mobileLink = $"nuviot:/auth/link/consume?code={Uri.EscapeDataString(rawCode)}";

            var ttlMinutes = (int)MagicLinkTtl.TotalMinutes;

            var sb = new StringBuilder();
            sb.AppendLine("Use this link to sign in:");
            sb.AppendLine(webLink);
            sb.AppendLine();
            sb.AppendLine("If you're signing in on a mobile device, you can also use this app link:");
            sb.AppendLine(mobileLink);
            sb.AppendLine();
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
            // URL-safe base64 without paddingGetInt32
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            var b64 = Convert.ToBase64String(bytes);
            return b64.Replace("+", "-").Replace("/", "_").Replace("=", string.Empty);
        }

        private static string FirstErrorOrDefault(InvokeResult result)
        {
            if (result?.Errors == null || result.Errors.Count == 0) return "error";
            return result.Errors[0].Message;
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
                var bytes = Encoding.UTF8.GetBytes(secret);
                var hash = sha.ComputeHash(bytes);

                // Lowercase hex for stable storage and comparison
                var sb = new StringBuilder(hash.Length * 2);
                foreach (var b in hash)
                {
                    sb.Append(b.ToString("x2"));
                }

                return sb.ToString();
            }
        }
    }
}
