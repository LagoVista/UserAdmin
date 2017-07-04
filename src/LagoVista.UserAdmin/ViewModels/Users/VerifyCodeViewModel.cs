using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.VerifyCodeVM_Title, UserAdminResources.Names.VerifyCodeVM_Help, UserAdminResources.Names.VerifyCodeVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class VerifyCodeViewModel : IValidateable
    {
        [FormField(IsRequired: true)]
        public string Provider { get; set; }

        [FormField(IsRequired:true, FieldType:FieldTypes.Integer)]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_RememberMe, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RememberBrowser { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_RememberMe, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RememberMe { get; set; }
    }
}
