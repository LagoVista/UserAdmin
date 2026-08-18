using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class ContinuitySessionManager : IContinuitySessionManager
    {
        private const int ProvisionalExpiredRetentionDays = 30;

        private readonly IAnonymousVisitorBootstrapManager _visitorManager;
        private readonly IProvisionalEnvironmentManager _provisionalEnvironmentManager;
        private readonly IAnonymousVisitorRepo _visitorRepo;
        private readonly IProvisionalEnvironmentRepo _provisionalEnvironmentRepo;
        private readonly IAppUserLoaderRepo _appUserRepo;
        private readonly IOrganizationLoaderRepo _organizationRepo;
        private readonly ITokenAuthOptions _tokenOptions;
        private readonly ITokenHelper _tokenHelper;

        public ContinuitySessionManager(IAnonymousVisitorBootstrapManager visitorManager, IProvisionalEnvironmentManager provisionalEnvironmentManager, IAnonymousVisitorRepo visitorRepo, IProvisionalEnvironmentRepo provisionalEnvironmentRepo, IAppUserLoaderRepo appUserRepo, IOrganizationLoaderRepo organizationRepo, ITokenAuthOptions tokenOptions, ITokenHelper tokenHelper)
        {
            _visitorManager = visitorManager ?? throw new ArgumentNullException(nameof(visitorManager));
            _provisionalEnvironmentManager = provisionalEnvironmentManager ?? throw new ArgumentNullException(nameof(provisionalEnvironmentManager));
            _visitorRepo = visitorRepo ?? throw new ArgumentNullException(nameof(visitorRepo));
            _provisionalEnvironmentRepo = provisionalEnvironmentRepo ?? throw new ArgumentNullException(nameof(provisionalEnvironmentRepo));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _organizationRepo = organizationRepo ?? throw new ArgumentNullException(nameof(organizationRepo));
            _tokenOptions = tokenOptions ?? throw new ArgumentNullException(nameof(tokenOptions));
            _tokenHelper = tokenHelper ?? throw new ArgumentNullException(nameof(tokenHelper));
        }

        public async Task<InvokeResult<ContinuitySessionResponse>> ResolveAsync(string continuityToken, string appUserId = null)
        {
            if (!String.IsNullOrWhiteSpace(continuityToken))
            {
                if (!String.IsNullOrWhiteSpace(appUserId))
                {
                    var claimedResult = await GetClaimedSessionAsync(continuityToken, appUserId);
                    if (claimedResult.Successful) return claimedResult;
                }

                var visitorResult = await _visitorManager.RestoreAsync(new AnonymousVisitorRestoreRequest { ContinuityToken = continuityToken });
                if (visitorResult.Successful) return InvokeResult<ContinuitySessionResponse>.Create(ToSession(visitorResult.Result));

                var provisionalResult = await _provisionalEnvironmentManager.RestoreAsync(new RestoreProvisionalEnvironmentRequest { RecoveryToken = continuityToken });
                if (provisionalResult.Successful) return await CreateProvisionalSessionAsync(provisionalResult.Result);
            }

            return await CreateFreshVisitorAsync();
        }

        public async Task<InvokeResult<ContinuitySessionResponse>> GetClaimedSessionAsync(string provisionalEnvironmentId, string appUserId, bool wasRestored = true)
        {
            if (String.IsNullOrWhiteSpace(provisionalEnvironmentId)) return InvokeResult<ContinuitySessionResponse>.FromError("ProvisionalEnvironmentId is required.");
            if (String.IsNullOrWhiteSpace(appUserId)) return InvokeResult<ContinuitySessionResponse>.FromError("AppUserId is required.");

            var environment = await _provisionalEnvironmentRepo.GetByIdAsync(provisionalEnvironmentId);
            if (environment == null) return InvokeResult<ContinuitySessionResponse>.FromError("The claimed environment was not found.");
            if (environment.State != ProvisionalEnvironmentState.Claimed) return InvokeResult<ContinuitySessionResponse>.FromError("The environment has not been claimed.");
            if (!String.Equals(environment.AppUserId, appUserId, StringComparison.Ordinal)) return InvokeResult<ContinuitySessionResponse>.FromError("The claimed environment does not belong to the current user.");

            return InvokeResult<ContinuitySessionResponse>.Create(new ContinuitySessionResponse
            {
                ActorId = environment.OriginActorId ?? environment.AppUserId,
                IdentityStage = ClaimsFactory.RegisteredIdentityStage,
                ContinuityToken = environment.Id,
                IdentityExpiresUtc = environment.ExpiresUtc,
                WasRestored = wasRestored,
                ProvisionalEnvironmentId = environment.Id,
                AppUserId = environment.AppUserId,
                OrganizationId = environment.OrganizationId,
                SubscriptionId = environment.SubscriptionId,
                BootstrapContext = environment.BootstrapContext
            });
        }

        public async Task<InvokeResult<ContinuitySessionResponse>> ResetAsync(string actorId, string identityStage, string continuityToken)
        {
            if (String.IsNullOrWhiteSpace(actorId)) return InvokeResult<ContinuitySessionResponse>.FromError("ActorId is required.");
            if (String.IsNullOrWhiteSpace(identityStage)) return InvokeResult<ContinuitySessionResponse>.FromError("IdentityStage is required.");

            InvokeResult retireResult;
            if (String.Equals(identityStage, ClaimsFactory.VisitorIdentityStage, StringComparison.Ordinal))
                retireResult = await RetireVisitorAsync(actorId);
            else if (String.Equals(identityStage, ClaimsFactory.ProvisionalIdentityStage, StringComparison.Ordinal))
                retireResult = await RetireProvisionalAsync(actorId, continuityToken);
            else
                return InvokeResult<ContinuitySessionResponse>.FromError("The current identity stage cannot be reset through continuity.");

            if (!retireResult.Successful) return InvokeResult<ContinuitySessionResponse>.FromInvokeResult(retireResult);
            return await CreateFreshVisitorAsync();
        }

        private async Task<InvokeResult> RetireVisitorAsync(string actorId)
        {
            var visitor = await _visitorRepo.GetByActorIdAsync(actorId);
            if (visitor == null) return InvokeResult.FromError("The current continuity identity could not be retired.");

            var now = DateTime.UtcNow;
            visitor.State = AnonymousVisitorState.Expired;
            visitor.ExpiredUtc = now;
            visitor.StateChangedUtc = now;
            visitor.ContinuityTokenHash = null;
            visitor.InstallationIdHash = null;
            await _visitorRepo.UpdateAsync(visitor);
            return InvokeResult.Success;
        }

        private async Task<InvokeResult> RetireProvisionalAsync(string actorId, string continuityToken)
        {
            if (String.IsNullOrWhiteSpace(continuityToken)) return InvokeResult.FromError("The continuity credential is required to reset a provisional identity.");

            var environment = await _provisionalEnvironmentRepo.FindByRecoveryTokenHashAsync(Hash(continuityToken));
            var environmentActorId = environment?.OriginActorId ?? environment?.AppUserId;
            if (environment == null || !String.Equals(environmentActorId, actorId, StringComparison.Ordinal)) return InvokeResult.FromError("The current continuity identity could not be retired.");

            var now = DateTime.UtcNow;
            environment.State = ProvisionalEnvironmentState.Expired;
            environment.ExpiredUtc = now;
            environment.PurgeAfterUtc = now.AddDays(ProvisionalExpiredRetentionDays);
            environment.StateChangedUtc = now;
            environment.RecoveryTokenHash = null;
            environment.InstallationIdHash = null;
            await _provisionalEnvironmentRepo.UpdateAsync(environment);
            return InvokeResult.Success;
        }

        private async Task<InvokeResult<ContinuitySessionResponse>> CreateFreshVisitorAsync()
        {
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
