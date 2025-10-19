// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: bac2de982ca546e3cb002e6f2e76b887c319d9efc73ce480c44814a71782712f
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.ManageUsers
{
    [EntityDescription(Domains.SecurityViewModels, UserAdminResources.Names.SetPasswordVM_Title, UserAdminResources.Names.SetPasswordVM_Help, UserAdminResources.Names.SetPasswordVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class SetPasswordViewModel : IValidateable
    {       
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Password, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string NewPassword { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_ConfirmPassword, CompareTo: nameof(NewPassword), CompareToMsgResource: UserAdminResources.Names.AppUser_PasswordConfirmPasswordMatch, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string ConfirmPassword { get; set; }
    }
}
