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

namespace LagoVista.UserAdmin.Models.Users
{

    [EntityDescription(Domains.UserDomain, UserAdminResources.Names.DeviceOwner_Title, UserAdminResources.Names.DeviceOwner_Description,
        UserAdminResources.Names.DeviceOwner_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources), Icon: "icon-ae-error-1")]
    public class DeviceOwnerUser : UserAdminModelBase, IValidateable
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

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_Devices, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public List<DeviceOwnerDevices> Devices { get; set; } = new List<DeviceOwnerDevices>();
        public EntityHeader CurrentRepo { get; set; }
        public EntityHeader CurrentDevice { get; set; }
        public string CurrentDeviceId { get; set; }        

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
                CurrentDeviceId = CurrentDeviceId,
                OwnerOrganization = OwnerOrganization,
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
                OwnerOrganization = appUser.OwnerOrganization,
            };
        }
    }

    [EntityDescription(Domains.UserDomain, UserAdminResources.Names.DeviceOwnersDevices_Title, UserAdminResources.Names.DeviceOwnersDevices_Description,
        UserAdminResources.Names.DeviceOwner_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources), Icon: "icon-ae-error-1")]
    public class DeviceOwnerDevices
    {
        public string Id { get; set; }
     
        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_Password, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false, IsRequired: true)]
        public EntityHeader DeviceRepository { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_Password, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false, IsRequired: true)]
        public EntityHeader Device { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.DeviceOwner_Password, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: false, IsRequired: true)]
        public string DeviceId { get; set; }

        public EntityHeader Product { get; set; }
    }
}
