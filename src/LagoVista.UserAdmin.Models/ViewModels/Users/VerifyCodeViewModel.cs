// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 66bbc4f8aeaf9a2bdd3ef88372031ae414bb244b38bdb574b39689f34252b572
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.VerifyCodeVM_Title, UserAdminResources.Names.VerifyCodeVM_Help, UserAdminResources.Names.VerifyCodeVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class VerifyCodeViewModel : IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.VerifyCodeViewModel_Provider, IsRequired: true)]
        public string Provider { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.VerifyCodeViewModel_RememberMe,  IsRequired:true, FieldType:FieldTypes.Integer)]
        public string Code { get; set; }

        public string ReturnUrl { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.VerifyCodeViewModel_RememberBrowser, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RememberBrowser { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.VerifyCodeViewModel_RememberMe, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RememberMe { get; set; }
    }
}
