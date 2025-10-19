// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e302b21cbab74b514310fa0348c694cb1276c57576b51ce27a577b323eea56b2
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.Core.Interfaces;

namespace LagoVista.UserAdmin.Models.Users
{

    [EntityDescription(Domains.UserDomain, UserAdminResources.Names.DeviceOwner_Title, UserAdminResources.Names.DeviceOwner_Description,
        UserAdminResources.Names.DeviceOwner_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources), Icon: "icon-ae-emoji",
        ListUIUrl: "/sysadmin/deviceusers", EditUIUrl: "/sysadmin/deviceuser/{id}",
        GetListUrl: "/api/sysadmin/deviceownerusers", GetUrl: "/api/sysadmin/deviceowneruser/{orgid}/{id}", DeleteUrl: "/api/sysadmin/deviceowneruser/{orgid}/{id}", SaveUrl: "/api/sysadmin/deviceowneruser", FactoryUrl: "/api/sysadmin/deviceowneruser/factory")]
    public class DeviceOwnerUser : UserAdminModelBase, IValidateable, ISummaryFactory
    {
        public DeviceOwnerUser()
        {
            Id = Guid.NewGuid().ToId();
        }

        public bool IsAnonymous { get; set; } = true;

        public string UserName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_FirstName, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false)]
        public string FirstName { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_LastName, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false)]
        public string LastName { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_EmailAddress, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false)]
        public string EmailAddress { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_PhoneNumber, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public string PhoneNumber { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_Password, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public string Password { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_Password, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public string PasswordConfirm { get; set; }

        public string PasswordHash { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_Devices, FieldType: FieldTypes.ChildList, ChildListDisplayMember:nameof(DeviceOwnerDevices.DeviceId), ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public List<DeviceOwnerDevices> Devices { get; set; } = new List<DeviceOwnerDevices>();
        public EntityHeader CurrentRepo { get; set; }
        public EntityHeader CurrentDevice { get; set; }
        public EntityHeader CurrentDeviceConfig { get; set; }
        public string CurrentDeviceId { get; set; }        

        public string HomePage { get; set; }
        public string MobileHomePage { get; set; }

        public DeviceOwnerUserSummary CreateSummary()
        {
            return new DeviceOwnerUserSummary()
            {
                FirstName = FirstName,
                LastName = LastName,
                Name = $"{FirstName} {LastName}",
                Key = Key,
                PhoneNumber = PhoneNumber,
                Id = Id,                 
            };
        }

        public AppUser ToAppUser()
        {
            return new AppUser()
            {
                Id = Id,
                Key = Key,        
                LoginType = LoginTypes.DeviceOwner,
                UserName = UserName,
                FirstName = FirstName,
                LastName = LastName,
                Email = EmailAddress,
                PhoneNumber = PhoneNumber,
                CurrentRepo = CurrentRepo,
                CurrentDevice = CurrentDevice,
                CurrentDeviceConfig = CurrentDeviceConfig,
                CurrentDeviceId = CurrentDeviceId,
                OwnerOrganization = OwnerOrganization,
                HomePage = HomePage,
                MobileHomePage = MobileHomePage,
            };
        }

        public static DeviceOwnerUser FromAppUser(AppUser appUser)
        {
            return new DeviceOwnerUser()
            {
                Id = appUser.Id,
                Key = appUser.Key,                
                UserName = appUser.UserName,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                EmailAddress = appUser.Email,
                PhoneNumber = appUser.PhoneNumber,
                CurrentRepo = appUser.CurrentRepo,
                CurrentDevice = appUser.CurrentDevice,
                CurrentDeviceId = appUser.CurrentDeviceId,
                CurrentDeviceConfig = appUser.CurrentDeviceConfig,
                OwnerOrganization = appUser.OwnerOrganization,
                HomePage = appUser.HomePage,
                MobileHomePage = appUser.MobileHomePage,
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

   
    public class DeviceOwnerDevices
    {
        public DeviceOwnerDevices()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }
     
        [FormField(LabelResource: UserAdminResources.Names.DeviceOwnersDevices_DeviceRepository, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false, IsRequired: true)]
        public EntityHeader DeviceRepository { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwnersDevices_Device, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false, IsRequired: true)]
        public EntityHeader Device { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwnersDevices_DeviceId, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false, IsRequired: true)]
        public string DeviceId { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwnersDevices_Product, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false, IsRequired: true)]
        public EntityHeader Product { get; set; }
    }

    [EntityDescription(Domains.UserDomain, UserAdminResources.Names.DeviceOwner_Title, UserAdminResources.Names.DeviceOwner_Description,
       UserAdminResources.Names.DeviceOwner_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources), Icon: "icon-ae-emoji",
       ListUIUrl: "/sysadmin/deviceusers", EditUIUrl: "/sysadmin/deviceuser/{id}",
       GetListUrl: "/api/sysadmin/deviceownerusers", GetUrl: "/api/sysadmin/deviceowneruser/{orgid}/{id}", DeleteUrl: "/api/sysadmin/deviceowneruser/{orgid}/{id}", SaveUrl: "/api/sysadmin/deviceowneruser", FactoryUrl: "/api/sysadmin/deviceowneruser/factory")]
    public class DeviceOwnerUserSummary : SummaryData
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
