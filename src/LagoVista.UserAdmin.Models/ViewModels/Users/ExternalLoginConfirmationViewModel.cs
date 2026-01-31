// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: bc2bf1284e0766c02aaa762e96b670074929945c0bfa37899501706f6639cdf2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(Domains.AuthDomain, UserAdminResources.Names.ExternalLoginConfirmVM_Title, UserAdminResources.Names.ExternalLoginConfirmVM_Help, UserAdminResources.Names.ExternalLoginConfirmVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class ExternalLoginConfirmationViewModel : IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; }
    }
}
