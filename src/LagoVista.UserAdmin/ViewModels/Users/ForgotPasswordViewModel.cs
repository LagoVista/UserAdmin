using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(Domains.SecurityViewModels, UserAdminResources.Names.ForgotPasswordVM_Title, UserAdminResources.Names.ForgotPasswordVM_Help, UserAdminResources.Names.ForgotPasswordVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]

    public class ForgotPasswordViewModel : IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; }
    }
}
