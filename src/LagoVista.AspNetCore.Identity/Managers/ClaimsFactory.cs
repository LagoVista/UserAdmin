using LagoVista.AspNetCore.Identity.Interfaces;
using System.Collections.Generic;
using LagoVista.UserAdmin.Models.Users;
using System.Security.Claims;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class ClaimsFactory : IClaimsFactory
    {
        public const string None = "-";

        public const string InstanceId = "com.lagovista.iot.instanceid";
        public const string InstanceName = "com.lagovista.iot.instancename";
        
        public const string CurrentUserId = "com.lagovista.iot.userid";
        public const string CurrentOrgName = "com.lagovista.iot.currentorgname";
        public const string CurrentOrgId = "com.lagovista.iot.currentorgid";
        public const string EmailVerified = "com.lagovista.iot.emailverified";
        public const string PhoneVerfiied = "com.lagovista.iot.phoneverified";
        public const string IsSystemAdmin = "com.lagovista.iot.issystemadmin";
        public const string IsPreviewUser = "com.lagovista.iot.ispreviewuser";
        public const string IsOrgAdmin = "com.lagovista.iot.isorgadmin";
        public const string IsAppBuilder = "com.lagovista.iot.isappbuilder";
        public const string IsUserDevice = "com.lagovista.iot.isuserdevice";
        public const string CurrentUserProfilePictureurl = "com.lagovista.iot.currentprofilepictureurl";

        // support for unattended Kiosk authentication
        public const string KioskId = "com.lagovista.iot.kioskid";
      
        public List<Claim> GetClaims(AppUser user)
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(CurrentUserId, user.Id),
                new Claim(IsPreviewUser, user.IsPreviewUser.ToString().ToLower()),
                new Claim(EmailVerified, user.EmailConfirmed.ToString()),
                new Claim(PhoneVerfiied, user.PhoneNumberConfirmed.ToString()),
                new Claim(IsSystemAdmin, user.IsSystemAdmin.ToString()),
                new Claim(IsOrgAdmin, user.IsOrgAdmin.ToString()),
                new Claim(IsAppBuilder, user.IsAppBuilder.ToString()),
                new Claim(IsUserDevice, user.IsUserDevice.ToString()),
                new Claim(CurrentOrgName, user.CurrentOrganization == null ? None : user.CurrentOrganization.Text),
                new Claim(CurrentOrgId, user.CurrentOrganization == null ? None : user.CurrentOrganization.Id),
                new Claim(CurrentUserProfilePictureurl, user.ProfileImageUrl.ImageUrl),
            };

            if (user.CurrentOrganizationRoles != null)
            {
                foreach (var role in user.CurrentOrganizationRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, $"{role.Id}.{role.Text}"));
                }
            }

            return claims;
        }
    }
}
