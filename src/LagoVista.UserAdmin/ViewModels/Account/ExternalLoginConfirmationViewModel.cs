using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.Account
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.ExternalLoginConfirmVM_Title, UserAdminResources.Names.ExternalLoginConfirmVM_Help, UserAdminResources.Names.ExternalLoginConfirmVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class ExternalLoginConfirmationViewModel : IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; }
    }
}
