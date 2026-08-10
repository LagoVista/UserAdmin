using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class AnonymousVisitorBootstrapManager : IAnonymousVisitorBootstrapManager
    {
        private const int ContinuityTokenBytes = 32;

        private readonly IAnonymousVisitorRepo _visitorRepo;
        private readonly IAnonymousVisitorBootstrapOptions _options;
        private readonly ITokenAuthOptions _tokenOptions;
        private readonly IAnonymousVisitorTokenService _tokenService;

        public AnonymousVisitorBootstrapManager(IAnonymousVisitorRepo visitorRepo, IAnonymousVisitorBootstrapOptions options, ITokenAuthOptions tokenOptions, IAnonymousVisitorTokenService tokenService)
        {
            _visitorRepo = visitorRepo ?? throw new ArgumentNullException(nameof(visitorRepo));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _tokenOptions = tokenOptions ?? throw new ArgumentNullException(nameof(tokenOptions));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        public async Task<InvokeResult<AnonymousVisitorBootstrapResponse>> BootstrapAsync(AnonymousVisitorBootstrapRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var configurationResult = ValidateConfiguration();
            if (!configurationResult.Successful) return InvokeResult<AnonymousVisitorBootstrapResponse>.FromInvokeResult(configurationResult);
            if ((request.BootstrapContext?.Length ?? 0) > AnonymousVisitor.MaximumBootstrapContextLength)
                return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError($"BootstrapContext cannot exceed {AnonymousVisitor.MaximumBootstrapContextLength} characters.");

            var continuityToken = CreateContinuityToken();
            var now = DateTime.UtcNow;
            var visitor = new AnonymousVisitor
            {
                ActorId = Guid.NewGuid().ToId(),
                State = AnonymousVisitorState.Active,
                ContinuityTokenHash = Hash(continuityToken),
                BootstrapContext = request.BootstrapContext,
                CreatedUtc = now,
                LastActivityUtc = now,
                ExpiresUtc = now.Add(_options.ActiveLifetime),
                StateChangedUtc = now,
                ConversionJourneyId = request.ConversionJourneyId,
                AcquisitionSourceKey = request.AcquisitionSourceKey,
                CampaignKey = request.CampaignKey,
                EntryPointType = request.EntryPointType,
                EntryPointKey = request.EntryPointKey,
                ExperimentKey = request.ExperimentKey,
                ExperimentVariantKey = request.ExperimentVariantKey,
                AgentKey = request.AgentKey,
                AgentVersion = request.AgentVersion,
                PromptVersion = request.PromptVersion
            };

            await _visitorRepo.CreateAsync(visitor);

            return CreateResponse(visitor, continuityToken, false);
        }

        public async Task<InvokeResult<AnonymousVisitorBootstrapResponse>> RestoreAsync(AnonymousVisitorRestoreRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var configurationResult = ValidateConfiguration();
            if (!configurationResult.Successful) return InvokeResult<AnonymousVisitorBootstrapResponse>.FromInvokeResult(configurationResult);

            if (String.IsNullOrWhiteSpace(request.ContinuityToken))
                return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("A continuity token is required.");

            var visitor = await _visitorRepo.FindByContinuityTokenHashAsync(Hash(request.ContinuityToken));
            if (visitor == null) return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("The anonymous visitor could not be restored.");
            if (visitor.State == AnonymousVisitorState.Promoted) return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("The anonymous visitor has already been promoted.");
            if (visitor.State != AnonymousVisitorState.Active) return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError($"Anonymous visitor is {visitor.State.ToString().ToLowerInvariant()}.");

            if (visitor.ExpiresUtc.ToUniversalTime() <= DateTime.UtcNow)
            {
                await ExpireAsync(visitor);
                return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("The anonymous visitor has expired.");
            }

            return await ResumeAsync(visitor, true);
        }

        private async Task<InvokeResult<AnonymousVisitorBootstrapResponse>> ResumeAsync(AnonymousVisitor visitor, bool wasRestored)
        {
            var continuityToken = CreateContinuityToken();
            var now = DateTime.UtcNow;
            visitor.ContinuityTokenHash = Hash(continuityToken);
            visitor.LastActivityUtc = now;
            visitor.ExpiresUtc = now.Add(_options.ActiveLifetime);
            await _visitorRepo.UpdateAsync(visitor);
            return CreateResponse(visitor, continuityToken, wasRestored);
        }

        private InvokeResult<AnonymousVisitorBootstrapResponse> CreateResponse(AnonymousVisitor visitor, string continuityToken, bool wasRestored)
        {
            var accessTokenExpiresUtc = DateTime.UtcNow.Add(_tokenOptions.AccessExpiration);
            if (accessTokenExpiresUtc > visitor.ExpiresUtc) accessTokenExpiresUtc = visitor.ExpiresUtc;
            var accessToken = _tokenService.CreateToken(visitor.ActorId, accessTokenExpiresUtc);

            return InvokeResult<AnonymousVisitorBootstrapResponse>.Create(new AnonymousVisitorBootstrapResponse
            {
                ActorId = visitor.ActorId,
                IdentityStage = ClaimsFactory.VisitorIdentityStage,
                AccessToken = accessToken,
                AccessTokenExpiresUtc = accessTokenExpiresUtc,
                ContinuityToken = continuityToken,
                VisitorExpiresUtc = visitor.ExpiresUtc,
                BootstrapContext = visitor.BootstrapContext,
                WasRestored = wasRestored
            });
        }

        private async Task ExpireAsync(AnonymousVisitor visitor)
        {
            if (visitor.State == AnonymousVisitorState.Expired) return;
            var now = DateTime.UtcNow;
            visitor.State = AnonymousVisitorState.Expired;
            visitor.ExpiredUtc = now;
            visitor.StateChangedUtc = now;
            await _visitorRepo.UpdateAsync(visitor);
        }

        private InvokeResult ValidateConfiguration()
        {
            if (_options.ActiveLifetime <= TimeSpan.Zero) return InvokeResult.FromError("AnonymousVisitor:ActiveLifetimeHours must be greater than zero.");
            if (_tokenOptions.AccessExpiration <= TimeSpan.Zero) return InvokeResult.FromError("TokenAuth:AccessTokenExpirationMinutes must be greater than zero.");
            return InvokeResult.Success;
        }

        private static string CreateContinuityToken()
        {
            var bytes = new byte[ContinuityTokenBytes];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", String.Empty);
        }

        private static string Hash(string value)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var item in bytes) builder.Append(item.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
