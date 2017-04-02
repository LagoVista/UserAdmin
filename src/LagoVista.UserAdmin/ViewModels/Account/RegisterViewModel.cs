using LagoVista.Core.Attributes;
using System;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Models;

namespace LagoVista.UserAdmin.ViewModels.Account
{
    [EntityDescription(Domains.UserViewModels, UserAdminResources.Names.RegisterVM_Title, UserAdminResources.Names.RegisterVM_Help, UserAdminResources.Names.RegisterVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class RegisterViewModel : IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.AppUser_FirstName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String FirstName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_LastName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String LastName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String Email { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Password, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Password { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_ConfirmPassword, CompareTo: nameof(Password), CompareToMsgResource: UserAdminResources.Names.AppUser_PasswordConfirmPasswordMatch, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string ConfirmPassword { get; set; }
    }
}
