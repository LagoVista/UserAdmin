using LagoVista.AspNetCore.Identity.Interfaces;
using System.Collections.Generic;
using LagoVista.UserAdmin.Models.Users;
using System.Security.Claims;
using LagoVista.Core.Models;
using System.Linq;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class ClaimsFactory : IClaimsFactory
    {
        public const string None = "-";

        public const string Logintype = "com.lagovista.iot.logintype";
        public const string InstanceId = "com.lagovista.iot.instanceid";
        public const string InstanceName = "com.lagovista.iot.instancename";
        public const string Bio = "com.lagovista.iot.bio";
        public const string Organization = "com.lagovista.iot.organization";
        public const string Avatar = "com.lagovista.iot.avatar";
        public const string ExternalAccountVerified = "com.lagovista.iot.externalaccount.verified";
        public const string ExternalAccountName = "com.lagovista.iot.externalaccount.name";
        public const string CurrentUserId = "com.lagovista.iot.userid";
        public const string CurrentOrgName = "com.lagovista.iot.currentorgname";
        public const string CurrentOrgId = "com.lagovista.iot.currentorgid";
        public const string OAuthToken = "com.lagovista.iot.oauth.token";
        public const string OAuthTokenVerifier = "com.lagovista.iot.oauth.tokenverifier";
        public const string EmailVerified = "com.lagovista.iot.emailverified";
        public const string PhoneVerfiied = "com.lagovista.iot.phoneverified";
        public const string IsSystemAdmin = "com.lagovista.iot.issystemadmin";
        public const string IsPreviewUser = "com.lagovista.iot.ispreviewuser";
        public const string IsOrgAdmin = "com.lagovista.iot.isorgadmin";
        public const string IsAppBuilder = "com.lagovista.iot.isappbuilder";
        public const string IsUserDevice = "com.lagovista.iot.isuserdevice";
        public const string IsFinancceAdmin = "com.lagovista.iot.isfinanceadmin";
        public const string CurrentUserProfilePictureurl = "com.lagovista.iot.currentprofilepictureurl";

        // support for unattended Kiosk authentication
        public const string KioskId = "com.lagovista.iot.kioskid";
      
        public List<Claim> GetClaims(AppUser user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.GivenName, !string.IsNullOrWhiteSpace(user.FirstName) ? user.FirstName : None),
                new Claim(ClaimTypes.Surname, !string.IsNullOrWhiteSpace(user.LastName) ? user.LastName : None),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(Avatar, user.ProfileImageUrl.ImageUrl),
                new Claim(CurrentUserId, user.Id),
                new Claim(IsPreviewUser, user.IsPreviewUser.ToString().ToLower()),
                new Claim(ExternalAccountVerified, user.ExternalLogins.Any().ToString()),
                new Claim(EmailVerified, user.EmailConfirmed.ToString()),
                new Claim(PhoneVerfiied, user.PhoneNumberConfirmed.ToString()),
                new Claim(IsSystemAdmin, user.IsSystemAdmin.ToString()),
                new Claim(IsOrgAdmin, user.IsOrgAdmin.ToString()),
                new Claim(IsAppBuilder, user.IsAppBuilder.ToString()),
                new Claim(IsUserDevice, user.IsUserDevice.ToString()),
                new Claim(IsFinancceAdmin, user.IsFinanceAdmin.ToString()),
                new Claim(CurrentOrgName, user.CurrentOrganization == null ? None : user.CurrentOrganization.Text),
                new Claim(CurrentOrgId, user.CurrentOrganization == null ? None : user.CurrentOrganization.Id),
                new Claim(CurrentUserProfilePictureurl, user.ProfileImageUrl.ImageUrl),
            };

            if (user.CurrentOrganizationRoles != null)
            {
                foreach (var role in user.CurrentOrganizationRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Key));
                }
            }

            return claims;
        }

        public List<Claim> GetClaims(AppUser user, EntityHeader org, bool isOrgAdmin, bool isAppBuilder)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.GivenName, !string.IsNullOrWhiteSpace(user.FirstName) ? user.FirstName : None),
                new Claim(ClaimTypes.Surname, !string.IsNullOrWhiteSpace(user.LastName) ? user.LastName : None),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(Avatar, user.ProfileImageUrl.ImageUrl),
                new Claim(CurrentUserId, user.Id),
                new Claim(ExternalAccountVerified, user.ExternalLogins.Any().ToString()),
                new Claim(IsPreviewUser, user.IsPreviewUser.ToString().ToLower()),
                new Claim(EmailVerified, user.EmailConfirmed.ToString()),
                new Claim(PhoneVerfiied, user.PhoneNumberConfirmed.ToString()),
                new Claim(IsSystemAdmin, user.IsSystemAdmin.ToString()),
                new Claim(IsOrgAdmin, isOrgAdmin.ToString()),
                new Claim(IsAppBuilder, isAppBuilder.ToString()),
                new Claim(IsUserDevice, user.IsUserDevice.ToString()),
                new Claim(IsFinancceAdmin, user.IsFinanceAdmin.ToString()),
                new Claim(CurrentOrgName, org.Text),
                new Claim(CurrentOrgId, org.Id),
                new Claim(CurrentUserProfilePictureurl, user.ProfileImageUrl.ImageUrl),
            };

            if (user.CurrentOrganizationRoles != null)
            {
                foreach (var role in user.CurrentOrganizationRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Key));
                }
            }

            return claims;
        }

    }
}
