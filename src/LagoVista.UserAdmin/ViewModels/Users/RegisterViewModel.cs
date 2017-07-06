using LagoVista.Core.Attributes;
using System;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Models;

namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(Domains.UserViewModels, UserAdminResources.Names.RegisterVM_Title, UserAdminResources.Names.RegisterVM_Help, UserAdminResources.Names.RegisterVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class RegisterViewModel : IValidateable
    {
        public String AppId { get; set; }
        public String InstallationId { get; set; }
        public String ClientType { get; set; }
        public String DeviceId { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_FirstName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string FirstName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_LastName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string LastName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Password, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Password { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_ConfirmPassword, CompareTo: nameof(Password), CompareToMsgResource: UserAdminResources.Names.AppUser_PasswordConfirmPasswordMatch, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string ConfirmPassword { get; set; }
    }
}
