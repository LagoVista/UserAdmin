using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.ManageAccount
{
    [EntityDescription(Domains.SecurityViewModels, UserAdminResources.Names.ChangePasswordVM_Title, UserAdminResources.Names.ChangePasswordVM_Help, UserAdminResources.Names.ChangePasswordVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class ChangePasswordViewModel : IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.AppUser_OldPassword, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string OldPassword { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_NewPassword, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string NewPassword { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_ConfirmPassword, CompareTo: nameof(NewPassword), CompareToMsgResource: UserAdminResources.Names.AppUser_PasswordConfirmPasswordMatch, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string ConfirmPassword { get; set; }
    }
}
