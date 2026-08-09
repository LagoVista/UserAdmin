using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class ContinuitySessionManager : IContinuitySessionManager
    {
        private readonly IAnonymousVisitorBootstrapManager _visitorManager;
        private readonly IProvisionalEnvironmentManager _provisionalEnvironmentManager;
        private readonly IAppUserLoaderRepo _appUserRepo;
        private readonly IOrganizationLoaderRepo _organizationRepo;
        private readonly ITokenAuthOptions _tokenOptions;
        private readonly ITokenHelper _tokenHelper;

        public ContinuitySessionManager(IAnonymousVisitorBootstrapManager visitorManager, IProvisionalEnvironmentManager provisionalEnvironmentManager, IAppUserLoaderRepo appUserRepo, IOrganizationLoaderRepo organizationRepo, ITokenAuthOptions tokenOptions, ITokenHelper tokenHelper)
        {
            _visitorManager = visitorManager ?? throw new ArgumentNullException(nameof(visitorManager));
            _provisionalEnvironmentManager = provisionalEnvironmentManager ?? throw new ArgumentNullException(nameof(provisionalEnvironmentManager));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _organizationRepo = organizationRepo ?? throw new ArgumentNullException(nameof(organizationRepo));
            _tokenOptions = tokenOptions ?? throw new ArgumentNullException(nameof(tokenOptions));
            _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        }

        public async Task<InvokeResult<ContinuitySessionResponse>> ResolveAsync(string continuityToken)
        {
            if (!String.IsNullOrWhiteSpace(continuityToken))
            {
                var visitorResult = await _visitorManager.RestoreAsync(new AnonymousVisitorRestoreRequest { ContinuityToken = continuityToken });
                if (visitorResult.Successful) return InvokeResult<ContinuitySessionResponse>.Create(ToSession(visitorResult.Result));

                var provisionalResult = await _provisionalEnvironmentManager.RestoreAsync(new RestoreProvisionalEnvironmentRequest { RecoveryToken = continuityToken });
                if (provisionalResult.Successful) return await CreateProvisionalSessionAsync(provisionalResult.Result);
            }

            var bootstrapResult = await _visitorManager.BootstrapAsync(new AnonymousVisitorBootstrapRequest());
            return bootstrapResult.Successful
                ? InvokeResult<ContinuitySessionResponse>.Create(ToSession(bootstrapResult.Result))
                : InvokeResult<ContinuitySessionResponse>.FromInvokeResult(bootstrapResult.ToInvokeResult());
        }

        private async Task<InvokeResult<ContinuitySessionResponse>> CreateProvisionalSessionAsync(RestoreProvisionalEnvironmentResponse environment)
        {
            var appUser = await _appUserRepo.FindByIdAsync(environment.AppUserId);
            if (appUser == null) return InvokeResult<ContinuitySessionResponse>.FromError("The provisional AppUser was not found.");

            var organization = await _organizationRepo.GetOrganizationAsync(environment.OrganizationId);
            if (organization == null) return InvokeResult<ContinuitySessionResponse>.FromError("The provisional organization was not found.");

            appUser.CurrentOrganization = organization.CreateSummary();
            var accessTokenExpiresUtc = DateTime.UtcNow.Add(_tokenOptions.AccessExpiration);
            if (accessTokenExpiresUtc > environment.ExpiresUtc) accessTokenExpiresUtc = environment.ExpiresUtc;

            return InvokeResult<ContinuitySessionResponse>.Create(new ContinuitySessionResponse
            {
                ActorId = environment.ActorId,
                IdentityStage = ClaimsFactory.ProvisionalIdentityStage,
                AccessToken = _tokenHelper.GetProvisionalJWToken(appUser, environment.ActorId, accessTokenExpiresUtc),
                AccessTokenExpiresUtc = accessTokenExpiresUtc,
                ContinuityToken = environment.RecoveryToken,
                IdentityExpiresUtc = environment.ExpiresUtc,
                WasRestored = true,
                ProvisionalEnvironmentId = environment.ProvisionalEnvironmentId,
                AppUserId = environment.AppUserId,
                OrganizationId = environment.OrganizationId,
                SubscriptionId = environment.SubscriptionId,
                BootstrapContext = environment.BootstrapContext
            });
        }

        private static ContinuitySessionResponse ToSession(AnonymousVisitorBootstrapResponse visitor)
        {
            return new ContinuitySessionResponse
            {
                ActorId = visitor.ActorId,
                IdentityStage = visitor.IdentityStage,
                AccessToken = visitor.AccessToken,
                AccessTokenExpiresUtc = visitor.AccessTokenExpiresUtc,
                ContinuityToken = visitor.ContinuityToken,
                IdentityExpiresUtc = visitor.VisitorExpiresUtc,
                WasRestored = visitor.WasRestored,
                BootstrapContext = visitor.BootstrapContext
            };
        }
    }
}
