using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.ManageAccount
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
