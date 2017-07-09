using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Utils
{
    public class OrgHelper : IOrgHelper
    {
        IAdminLogger _adminLogger;
        IOrganizationManager _orgManager;
        IUserManager _userManager;

        public OrgHelper(IAdminLogger adminLogger, IOrganizationManager orgManager, IUserManager userManager)
        {
            _adminLogger = adminLogger;
            _orgManager = orgManager;
            _userManager = userManager;
        }

        public async Task<InvokeResult> SetUserOrgAsync(AuthRequest authRequest, AppUser appUser)
        {
            // Synthesize the org and user from request and app user
            var org = new EntityHeader() { Id = authRequest.OrgId, Text = authRequest.OrgName };
            var user = new EntityHeader() { Id = appUser.Id, Text = $"{appUser.FirstName} {appUser.LastName}" };


            // 1) Ensure user has access to the requested org.
            var orgs = await _orgManager.GetOrganizationsForUserAsync(appUser.Id, org, user);
            var switchToOrg = orgs.Where(o => o.OrgId == authRequest.OrgId).FirstOrDefault();
            if (switchToOrg == null)
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_SetOrg", UserAdminErrorCodes.AuthMissingRefreshToken.Message,
                    new KeyValuePair<string, string>("userid", appUser.Id),
                    new KeyValuePair<string, string>("requestedOrgId", authRequest.OrgId));
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthOrgNotAuthorized.ToErrorMessage());
            }

            var oldOrgId = EntityHeader.IsNullOrEmpty(appUser.CurrentOrganization) ? "none" : appUser.CurrentOrganization.Id;
            var oldOrgName = EntityHeader.IsNullOrEmpty(appUser.CurrentOrganization) ? "none" : appUser.CurrentOrganization.Text;

            // 2) Change the org on the user object
            appUser.CurrentOrganization = new EntityHeader()
            {
                Id = authRequest.OrgId,
                Text = switchToOrg.OrganizationName,
            };

            // 3) Add the roles to the user for the org.
            var orgRoles = await _orgManager.GetUsersRolesInOrgAsync(authRequest.OrgId, appUser.Id, appUser.CurrentOrganization, appUser.ToEntityHeader());
            appUser.CurrentOrganizationRoles = new List<EntityHeader>();
            foreach (var orgRole in orgRoles)
            {
                appUser.CurrentOrganizationRoles.Add(orgRole.ToEntityHeader());
            }

            // 4) Write the updated user back to storage.
            var updateResult = await _userManager.UpdateAsync(appUser);
            if (!updateResult.Succeeded)
            {
                var args = new List<KeyValuePair<string, string>>();
                var idx = 1;

                foreach (var err in updateResult.Errors)
                {
                    args.Add(new KeyValuePair<string, string>($"{idx++}", err.Description));
                }

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Error, "AuthTokenManager_SetOrg", UserAdminErrorCodes.AuthErrorUpdatingUser.Message, args.ToArray());
                return InvokeResult.FromErrors(UserAdminErrorCodes.AuthMissingRefreshToken.ToErrorMessage());
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
