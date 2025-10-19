// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 695ed0d20ada2e1618ef5790342390cae87003b864ed1a019fff05e47edd5fca
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.ManageUsers
{
    [EntityDescription(Domains.UserViewModels, UserAdminResources.Names.IndexVM_Title, UserAdminResources.Names.IndexVM_Help, UserAdminResources.Names.IndexVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class IndexViewModel
    {
        [FormField(LabelResource: UserAdminResources.Names.VerifyUser_PhoneConfirmed, FieldType: FieldTypes.Phone, ResourceType: typeof(UserAdminResources))]
        public string PhoneNumber { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.VerifyUser_BrowserRemembered, FieldType: FieldTypes.Bool, ResourceType: typeof(UserAdminResources))]
        public bool BrowserRemembered { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.VerifyUser_EmailConfirmed, FieldType: FieldTypes.Bool, ResourceType: typeof(UserAdminResources))]
        public bool EmailConfirmed { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.VerifyUser_PhoneConfirmed, FieldType: FieldTypes.Bool, ResourceType: typeof(UserAdminResources))]
        public bool PhoneConfirmd { get; set; }
    }
}
