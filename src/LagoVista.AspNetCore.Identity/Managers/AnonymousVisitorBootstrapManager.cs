using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
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
        private readonly IAppUserLoaderRepo _appUserRepo;
        private readonly IOrganizationLoaderRepo _organizationRepo;
        private readonly IAnonymousVisitorBootstrapOptions _options;
        private readonly ITokenAuthOptions _tokenOptions;
        private readonly ITokenHelper _tokenHelper;

        public AnonymousVisitorBootstrapManager(IAnonymousVisitorRepo visitorRepo, IAppUserLoaderRepo appUserRepo, IOrganizationLoaderRepo organizationRepo, IAnonymousVisitorBootstrapOptions options, ITokenAuthOptions tokenOptions, ITokenHelper tokenHelper)
        {
            _visitorRepo = visitorRepo ?? throw new ArgumentNullException(nameof(visitorRepo));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _organizationRepo = organizationRepo ?? throw new ArgumentNullException(nameof(organizationRepo));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _tokenOptions = tokenOptions ?? throw new ArgumentNullException(nameof(tokenOptions));
            _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        }

        public async Task<InvokeResult<AnonymousVisitorBootstrapResponse>> BootstrapAsync(AnonymousVisitorBootstrapRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var configurationResult = ValidateConfiguration();
            if (!configurationResult.Successful) return InvokeResult<AnonymousVisitorBootstrapResponse>.FromInvokeResult(configurationResult);
            if ((request.BootstrapContext?.Length ?? 0) > AnonymousVisitor.MaximumBootstrapContextLength)
                return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError($"BootstrapContext cannot exceed {AnonymousVisitor.MaximumBootstrapContextLength} characters.");

            AnonymousVisitor visitor = null;
            if (!String.IsNullOrWhiteSpace(request.InstallationId))
                visitor = await _visitorRepo.FindByInstallationIdHashAsync(Hash(request.InstallationId));

            if (visitor != null)
            {
                if (visitor.State == AnonymousVisitorState.Promoted)
                    return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("The anonymous visitor has already been promoted.");

                if (visitor.State == AnonymousVisitorState.Active && visitor.ExpiresUtc.ToUniversalTime() > DateTime.UtcNow)
                {
                    if (!String.IsNullOrWhiteSpace(request.BootstrapContext)) visitor.BootstrapContext = request.BootstrapContext;
                    return await ResumeAsync(visitor, true);
                }

                await ReleaseInstallationAsync(visitor);
            }

            var continuityToken = CreateContinuityToken();
            var now = DateTime.UtcNow;
            visitor = new AnonymousVisitor
            {
                ActorId = Guid.NewGuid().ToId(),
                State = AnonymousVisitorState.Active,
                ContinuityTokenHash = Hash(continuityToken),
                InstallationIdHash = HashOptional(request.InstallationId),
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

            try
            {
                await _visitorRepo.CreateAsync(visitor);
            }
            catch when (!String.IsNullOrWhiteSpace(request.InstallationId))
            {
                visitor = await _visitorRepo.FindByInstallationIdHashAsync(Hash(request.InstallationId));
                if (visitor == null || visitor.State != AnonymousVisitorState.Active || visitor.ExpiresUtc.ToUniversalTime() <= DateTime.UtcNow) throw;
                if (!String.IsNullOrWhiteSpace(request.BootstrapContext)) visitor.BootstrapContext = request.BootstrapContext;
                return await ResumeAsync(visitor, true);
            }

            return await CreateResponseAsync(visitor, continuityToken, false);
        }

        public async Task<InvokeResult<AnonymousVisitorBootstrapResponse>> RestoreAsync(AnonymousVisitorRestoreRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var configurationResult = ValidateConfiguration();
            if (!configurationResult.Successful) return InvokeResult<AnonymousVisitorBootstrapResponse>.FromInvokeResult(configurationResult);

            var hasContinuityToken = !String.IsNullOrWhiteSpace(request.ContinuityToken);
            var hasInstallationId = !String.IsNullOrWhiteSpace(request.InstallationId);
            if (!hasContinuityToken && !hasInstallationId)
                return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("A continuity token or installation ID is required.");

            var continuityVisitor = hasContinuityToken ? await _visitorRepo.FindByContinuityTokenHashAsync(Hash(request.ContinuityToken)) : null;
            var installationVisitor = hasInstallationId ? await _visitorRepo.FindByInstallationIdHashAsync(Hash(request.InstallationId)) : null;

            if (hasContinuityToken && hasInstallationId && (continuityVisitor == null || installationVisitor == null || !String.Equals(continuityVisitor.ActorId, installationVisitor.ActorId, StringComparison.Ordinal)))
                return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("The supplied continuity credentials do not identify the same anonymous visitor.");

            var visitor = continuityVisitor ?? installationVisitor;
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
            return await CreateResponseAsync(visitor, continuityToken, wasRestored);
        }

        private async Task<InvokeResult<AnonymousVisitorBootstrapResponse>> CreateResponseAsync(AnonymousVisitor visitor, string continuityToken, bool wasRestored)
        {
            var appUser = await _appUserRepo.FindByIdAsync(_options.AppUserId);
            if (appUser == null) return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("The shared anonymous AppUser was not found.");

            var organization = await _organizationRepo.GetOrganizationAsync(_options.OrganizationId);
            if (organization == null) return InvokeResult<AnonymousVisitorBootstrapResponse>.FromError("The shared anonymous organization was not found.");

            appUser.CurrentOrganization = organization.CreateSummary();
            var accessTokenExpiresUtc = DateTime.UtcNow.Add(_tokenOptions.AccessExpiration);
            if (accessTokenExpiresUtc > visitor.ExpiresUtc) accessTokenExpiresUtc = visitor.ExpiresUtc;
            var accessToken = _tokenHelper.GetAnonymousVisitorJWToken(appUser, visitor.ActorId, accessTokenExpiresUtc);

            return InvokeResult<AnonymousVisitorBootstrapResponse>.Create(new AnonymousVisitorBootstrapResponse
            {
                ActorId = visitor.ActorId,
                IdentityStage = "visitor",
                AccessToken = accessToken,
                AccessTokenExpiresUtc = accessTokenExpiresUtc,
                ContinuityToken = continuityToken,
                VisitorExpiresUtc = visitor.ExpiresUtc,
                BootstrapContext = visitor.BootstrapContext,
                WasRestored = wasRestored
            });
        }

        private async Task ReleaseInstallationAsync(AnonymousVisitor visitor)
        {
            if (visitor.State == AnonymousVisitorState.Active) await ExpireAsync(visitor);
            visitor.InstallationIdHash = null;
            await _visitorRepo.UpdateAsync(visitor);
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
            if (String.IsNullOrWhiteSpace(_options.AppUserId)) return InvokeResult.FromError("AnonymousVisitor:AppUserId is not configured.");
            if (String.IsNullOrWhiteSpace(_options.OrganizationId)) return InvokeResult.FromError("AnonymousVisitor:OrganizationId is not configured.");
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

        private static string HashOptional(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? null : Hash(value);
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
