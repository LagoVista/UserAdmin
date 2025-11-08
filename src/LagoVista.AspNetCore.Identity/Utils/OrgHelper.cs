// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f3fdff392f8756c9c62f428572d91181df54087b5779f53d4aef081c401ced86
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core;
using LagoVista.UserAdmin;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using System;

namespace LagoVista.AspNetCore.Identity.Utils
{
    public class OrgHelper : IOrgHelper
    {
        private readonly IAdminLogger _adminLogger;
        private readonly IOrganizationManager _orgManager;
        private readonly IUserManager _userManager;
        private readonly IUserRoleRepo _userRoleRepo;

        public OrgHelper(IAdminLogger adminLogger, IOrganizationManager orgManager, IUserManager userManager, IUserRoleRepo userRoleRepo)
        {
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _orgManager = orgManager ?? throw new ArgumentNullException(nameof(orgManager));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
        }

        public async Task<InvokeResult> SetUserOrgAsync(AuthRequest authRequest, AppUser appUser)
        {
            // Synthesize the org and user from request and app user
            var org = new EntityHeader() { Id = authRequest.OrgId, Text = authRequest.OrgName };
            var user = new EntityHeader() { Id = appUser.Id, Text = $"{appUser.FirstName} {appUser.LastName}" };

            authRequest.OrgId = org.Id;
            authRequest.OrgName = org.Text;

            // 1) Ensure user has access to the requested org.
            var orgs = await _orgManager.GetOrganizationsForUserAsync(appUser.Id, org, user);
            var switchToOrg = orgs.Where(o => o.OrgId == authRequest.OrgId).FirstOrDefault();
            if (switchToOrg == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_SetOrg", UserAdminErrorCodes.AuthOrgNotAuthorized.Message,
                    new KeyValuePair<string, string>("userid", appUser.Id),
                    new KeyValuePair<string, string>("requestedOrgId", authRequest.OrgId));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthOrgNotAuthorized.ToErrorMessage());
            }

            var oldOrgId = null == appUser.CurrentOrganization ? "none" : appUser.CurrentOrganization.Id;
            var oldOrgName = null == appUser.CurrentOrganization ? "none" : appUser.CurrentOrganization.Text;

            var fullOrg = await _orgManager.GetOrganizationAsync(authRequest.OrgId, org, user);

            // 2) Change the org on the user object
            appUser.CurrentOrganization = fullOrg.CreateSummary();
            appUser.IsOrgAdmin = switchToOrg.IsOrgAdmin;    

            // 3) Add the roles to the user for the org.
            var orgRoles = await _userRoleRepo.GetRolesForUserAsync( appUser.Id, authRequest.OrgId);
            appUser.CurrentOrganizationRoles = new List<EntityHeader>();
            foreach (var orgRole in orgRoles)
            {
                appUser.CurrentOrganizationRoles.Add(orgRole.ToEntityHeader());
            }

            // 4) Write the updated user back to storage.
            var updateResult = await _userManager.UpdateAsync(appUser);
            if (!updateResult.Successful)
            {
                var invokeResult = updateResult.ToInvokeResult();

                _adminLogger.LogInvokeResult("OrgHelper_SetUserOrgAsync", invokeResult,
                    new KeyValuePair<string, string>("userId", appUser.Id),
                    new KeyValuePair<string, string>("userName", appUser.UserName));

                return invokeResult;
            }

            // 5) Write this change to logger.
            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "AuthTokenManager_SetOrg", "UserSwitchedOrg",
                new KeyValuePair<string, string>("userId", appUser.Id),
                new KeyValuePair<string, string>("userName", appUser.UserName),
                new KeyValuePair<string, string>("oldOrgId", oldOrgId),
                new KeyValuePair<string, string>("oldOrgName", oldOrgName),
                new KeyValuePair<string, string>("newOrgId", appUser.CurrentOrganization.Id),
                new KeyValuePair<string, string>("newOrgName", appUser.CurrentOrganization.Text));

            // 6) Return success, no response data necessary, app user is by reference so it should already be updated.
            return InvokeResult.Success;
        }
    }
}
