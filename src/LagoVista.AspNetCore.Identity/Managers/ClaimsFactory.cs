using LagoVista.AspNetCore.Identity.Interfaces;
using System.Collections.Generic;
using LagoVista.UserAdmin.Models.Users;
using System.Security.Claims;
using LagoVista.Core.Models;
using System.Linq;
using RingCentral;
using System;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class ClaimsFactory : IClaimsFactory
    {
        public const string None = "-";

        public const string Logintype = "com.lagovista.iot.logintype";
        public const string InstanceId = "com.lagovista.iot.instanceid";
        public const string InstanceName = "com.lagovista.iot.instancename";
        public const string HostId = "com.lagovista.iot.hostid";
        public const string HostName = "com.lagovista.iot.hostname";
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
        public const string Anonymous = "com.lagovista.iot.anonymous";
        public const string EmailVerified = "com.lagovista.iot.emailverified";
        public const string PhoneVerfiied = "com.lagovista.iot.phoneverified";
        public const string IsSystemAdmin = "com.lagovista.iot.issystemadmin";
        public const string IsPreviewUser = "com.lagovista.iot.ispreviewuser";
        public const string IsOrgAdmin = "com.lagovista.iot.isorgadmin";
        public const string IsAppBuilder = "com.lagovista.iot.isappbuilder";
        public const string IsUserDevice = "com.lagovista.iot.isuserdevice";
        public const string IsFinancceAdmin = "com.lagovista.iot.isfinanceadmin";
        public const string CurrentUserProfilePictureurl = "com.lagovista.iot.currentprofilepictureurl";

        public const string DeviceUniqueId = "com.lagovista.iot.deviceuniqueid";
        public const string DeviceId = "com.lagovista.iot.deviceid";
        public const string DeviceName = "com.lagovista.iot.devicename";
        public const string DeviceRepoId = "com.lagovista.iot.devicerepoid";
        public const string DeviceRepoName = "com.lagovista.iot.devicereponame";
        public const string DeviceConfigId = "com.lagovista.iot.deviceconfigid";
        public const string DeviceConfigName = "com.lagovista.iot.deviceconfigname";

        public const string CustomerId = "com.lagovista.iot.customerid";
        public const string CustomerName = "com.lagovista.iot.customername";
        public const string CustomerContactId = "com.lagovista.iot.customercontactid";
        public const string CustomerContactName = "com.lagovista.iot.customercontactname";

        // support for unattended Kiosk authentication
        public const string KioskId = "com.lagovista.iot.kioskid";

        public List<Claim> GetClaims(AppUser user)
        {
            if (user.LoginType == LoginTypes.DeviceOwner)
                return GetClaimsForDeviceOwner(user);

            var claims = new List<Claim> {
                new Claim(Logintype, nameof(AppUser)),
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


            if (user.Customer != null)
            {
                claims.Add(new Claim(CustomerId, user.Customer.Id));
                claims.Add(new Claim(CustomerName, user.Customer.Text));
            }

            if (user.CustomerContact != null)
            {
                claims.Add(new Claim(CustomerContactId, user.CustomerContact.Id));
                claims.Add(new Claim(CustomerContactName, user.CustomerContact.Text));
            }

            // OK - this little dance will bite us in the but for sure at some point....
            //      need to align customer type devices and user type devices.
            if (user.DeviceRepo != null)
            {
                claims.Add(new Claim(DeviceRepoId, user.DeviceRepo.Id));
                claims.Add(new Claim(DeviceRepoName, user.DeviceRepo.Text));
            }
            else if(user.CurrentRepo != null)
            {
                claims.Add(new Claim(DeviceRepoId, user.CurrentRepo.Id));
                claims.Add(new Claim(DeviceRepoName, user.CurrentRepo.Text));
            }

            if (user.CurrentInstance != null)
            {
                claims.Add(new Claim(InstanceId, user.CurrentInstance.Id));
                claims.Add(new Claim(InstanceName, user.CurrentInstance.Text));
            }

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
            if (user.LoginType == LoginTypes.DeviceOwner)
                return GetClaimsForDeviceOwner(user);

            var claims = new List<Claim> {
                new Claim(Logintype, nameof(AppUser)),
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
                new Claim(CurrentOrgName, org == null ? None : org.Text),
                new Claim(CurrentOrgId, org == null ? None : org.Id),
                new Claim(CurrentUserProfilePictureurl, user.ProfileImageUrl.ImageUrl),
            };

            if(user.Customer != null)
            {
                claims.Add(new Claim(CustomerId, user.Customer.Id));
                claims.Add(new Claim(CustomerName, user.Customer.Text));
            }

            if(user.CustomerContact != null)
            {
                claims.Add(new Claim(CustomerContactId, user.CustomerContact.Id));
                claims.Add(new Claim(CustomerContactName, user.CustomerContact.Text));
            }

            if(user.DeviceRepo != null)
            {
                claims.Add(new Claim(DeviceRepoId, user.DeviceRepo.Id));
                claims.Add(new Claim(DeviceRepoName, user.DeviceRepo.Text));
            }

            if(user.CurrentInstance != null)
            {
                claims.Add(new Claim(InstanceId, user.CurrentInstance.Id));
                claims.Add(new Claim(InstanceName, user.CurrentInstance.Text));
            }

            if (user.CurrentOrganizationRoles != null)
            {
                foreach (var role in user.CurrentOrganizationRoles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Key));
                }
            }

            return claims;
        }

        public List<Claim> GetClaimsForCustomer(AppUser owner)
        {
            if (string.IsNullOrEmpty(owner.Id))
                throw new ArgumentNullException("Owner.Id");

            if (EntityHeader.IsNullOrEmpty(owner.OwnerOrganization))
                throw new ArgumentNullException(nameof(AppUser.OwnerOrganization));

            if (EntityHeader.IsNullOrEmpty(owner.Customer))
                throw new ArgumentNullException(nameof(AppUser.Customer));

            if (EntityHeader.IsNullOrEmpty(owner.CustomerContact))
                throw new ArgumentNullException(nameof(AppUser.CustomerContact));

            if (EntityHeader.IsNullOrEmpty(owner.CurrentRepo))
                throw new ArgumentNullException(nameof(AppUser.CurrentRepo));

            if (EntityHeader.IsNullOrEmpty(owner.CurrentInstance))
                throw new ArgumentNullException(nameof(AppUser.CurrentInstance));

            var claims = new List<Claim>
            {
                new Claim(CurrentUserId, owner.Id),

                new Claim(ClaimTypes.GivenName, string.IsNullOrEmpty( owner.FirstName) ? "anonymous" : owner.FirstName),
                new Claim(ClaimTypes.Surname, string.IsNullOrEmpty( owner.LastName) ? "anonymous" : owner.LastName),

                new Claim(Logintype, "Customer"),
                new Claim(Anonymous, owner.IsAnonymous.ToString()),

                new Claim(EmailVerified, true.ToString()),
                new Claim(PhoneVerfiied, true.ToString()),

                new Claim(CurrentOrgName, owner.OwnerOrganization.Text),
                new Claim(CurrentOrgId, owner.OwnerOrganization.Id),

                new Claim(CurrentOrgName, owner.OwnerOrganization.Text),
                new Claim(CurrentOrgId, owner.OwnerOrganization.Id),

                new Claim(InstanceId, owner.CurrentInstance.Id),
                new Claim(InstanceName, owner.CurrentInstance.Text),

                new Claim(DeviceRepoId, owner.CurrentRepo.Id),
                new Claim(DeviceRepoName, owner.CurrentRepo.Text),

                new Claim(CustomerId, owner.Customer.Id),
                new Claim(CustomerName, owner.Customer.Text),

                new Claim(CustomerContactId, owner.CustomerContact.Id),
                new Claim(CustomerContactName, owner.CustomerContact.Text),
            };

            return claims;
        }

        public List<Claim> GetClaimsForDeviceOwner(AppUser owner)
        {
            if (string.IsNullOrEmpty(owner.Id))
                throw new ArgumentNullException("Owner.Id");

            if (EntityHeader.IsNullOrEmpty(owner.OwnerOrganization))
                throw new ArgumentNullException(nameof(DeviceOwnerUser.OwnerOrganization));

            if (EntityHeader.IsNullOrEmpty(owner.CurrentDevice))
                throw new ArgumentNullException(nameof(DeviceOwnerUser.CurrentDevice));

            if (EntityHeader.IsNullOrEmpty(owner.CurrentRepo))
                throw new ArgumentNullException(nameof(DeviceOwnerUser.CurrentRepo));

            if (EntityHeader.IsNullOrEmpty(owner.CurrentDeviceConfig))
                throw new ArgumentNullException(nameof(DeviceOwnerUser.CurrentDeviceConfig));

            if (String.IsNullOrEmpty(owner.CurrentDeviceId))
                throw new ArgumentNullException(nameof(DeviceOwnerUser.CurrentDeviceId));

            var claims = new List<Claim>
            {
                new Claim(CurrentUserId, owner.Id),

                new Claim(ClaimTypes.GivenName, string.IsNullOrEmpty( owner.FirstName) ? "anonymous" : owner.FirstName),
                new Claim(ClaimTypes.Surname, string.IsNullOrEmpty( owner.LastName) ? "anonymous" : owner.LastName),

                new Claim(Logintype, nameof(DeviceOwnerUser)),
                new Claim(Anonymous, owner.IsAnonymous.ToString()),

                new Claim(EmailVerified, true.ToString()),
                new Claim(PhoneVerfiied, true.ToString()),

                new Claim(CurrentOrgName, owner.OwnerOrganization.Text),
                new Claim(CurrentOrgId, owner.OwnerOrganization.Id),

                new Claim(DeviceId, owner.CurrentDeviceId),

                new Claim(DeviceRepoId, owner.CurrentRepo.Id),
                new Claim(DeviceRepoName, owner.CurrentRepo.Text),

                new Claim(DeviceUniqueId, owner.CurrentDevice.Id),
                new Claim(DeviceName, owner.CurrentDevice.Text),

                new Claim(DeviceConfigId, owner.CurrentDeviceConfig.Id),
                new Claim(DeviceConfigName, owner.CurrentDeviceConfig.Text),
            };

            return claims;
        }
    }
}

