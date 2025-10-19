// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 48d348fc333f22bf23c4ad4e89acf521647a1cd98b6d2e30ff3afbc7cf1e001d
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.ManageUsers
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
