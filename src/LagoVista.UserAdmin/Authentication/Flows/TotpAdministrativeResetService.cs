using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public interface ITotpAdministrativeResetService
    {
        Task<InvokeResult> ResetAsync(string targetUserId, EntityHeader organization, EntityHeader actor);
    }

    [CriticalCoverage]
    public class TotpAdministrativeResetService : ITotpAdministrativeResetService
    {
        private static readonly TimeSpan MfaFreshnessWindow = TimeSpan.FromMinutes(15);

        private readonly IAppUserRepo _appUserRepo;
        private readonly IAppUserMfaManager _mfaManager;
        private readonly IOrganizationManager _organizationManager;
        private readonly IAuthenticationLogManager _authLogManager;

        public TotpAdministrativeResetService(
            IAppUserRepo appUserRepo,
            IAppUserMfaManager mfaManager,
            IOrganizationManager organizationManager,
            IAuthenticationLogManager authLogManager)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
            _organizationManager = organizationManager ?? throw new ArgumentNullException(nameof(organizationManager));
            _authLogManager = authLogManager ?? throw new ArgumentNullException(nameof(authLogManager));
        }

        public async Task<InvokeResult> ResetAsync(string targetUserId, EntityHeader organization, EntityHeader actor)
        {
            if (String.IsNullOrWhiteSpace(targetUserId))
                return InvokeResult.FromError("target_user_required");
            if (organization == null || String.IsNullOrWhiteSpace(organization.Id))
                return InvokeResult.FromError("organization_required");
            if (actor == null || String.IsNullOrWhiteSpace(actor.Id))
                return InvokeResult.FromError("actor_required");

            var extras = $"targetUserId={targetUserId}";
            await _authLogManager.AddAsync(AuthLogTypes.TotpAdministrativeResetStart, actor, organization, extras: extras);

            var actorUser = await _appUserRepo.FindByIdAsync(actor.Id);
            if (actorUser == null)
                return await FailedAsync(actor, organization, extras, "actor_not_found");

            var isAuthorized = actorUser.IsSystemAdmin || await _organizationManager.IsUserOrgAdminAsync(organization.Id, actor.Id);
            if (!isAuthorized)
                return await FailedAsync(actor, organization, extras, "not_authorized");

            if (!TryGetFreshMfaUtc(actorUser.LastMfaDateTimeUtc, out var lastMfaUtc) || DateTime.UtcNow - lastMfaUtc > MfaFreshnessWindow)
                return await FailedAsync(actor, organization, extras, "step_up_required");

            var targetUser = await _appUserRepo.FindByIdAsync(targetUserId);
            if (targetUser == null)
                return await FailedAsync(actor, organization, extras, "target_user_not_found");

            var belongsToOrganization = targetUser.Organizations?.Any(org => String.Equals(org.Id, organization.Id, StringComparison.OrdinalIgnoreCase)) == true;
            if (!belongsToOrganization)
                return await FailedAsync(actor, organization, extras, "target_not_in_organization");

            var resetResult = await _mfaManager.ResetMfaAsync(targetUser.Id, organization, actor);
            if (!resetResult.Successful)
                return await FailedAsync(actor, organization, extras, "reset_failed");

            await _authLogManager.AddAsync(AuthLogTypes.TotpAdministrativeResetSuccess, actor, organization, extras: extras);
            return InvokeResult.Success;
        }

        private async Task<InvokeResult> FailedAsync(EntityHeader actor, EntityHeader organization, string extras, string error)
        {
            await _authLogManager.AddAsync(AuthLogTypes.TotpAdministrativeResetFailed, actor, organization, errors: error, extras: extras);
            return InvokeResult.FromError(error);
        }

        private static bool TryGetFreshMfaUtc(string value, out DateTime utc)
        {
            utc = default;
            if (!DateTime.TryParse(value, out var parsed))
                return false;

            utc = parsed.Kind == DateTimeKind.Utc ? parsed : parsed.ToUniversalTime();
            return true;
        }
    }
}
