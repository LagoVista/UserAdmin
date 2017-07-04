using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(Domains.SecurityViewModels, UserAdminResources.Names.LoginVM_Title, UserAdminResources.Names.LoginVM_Help, UserAdminResources.Names.LoginVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class LoginViewModel : IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Password, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Password { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_RememberMe, FieldType: FieldTypes.CheckBox,  ResourceType: typeof(UserAdminResources))]
        public bool RememberMe { get; set; }
    }
}
