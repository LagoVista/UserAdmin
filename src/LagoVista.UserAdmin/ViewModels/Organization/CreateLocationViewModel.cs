using LagoVista.Core.Attributes;
using System;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.CreateLocationVM_Title, UserAdminResources.Names.CreateOrganizationVM_Help, UserAdminResources.Names.CreateOrganizationVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class CreateLocationViewModel : LocationViewModel
    {
        [FormField(FieldType: FieldTypes.Hidden)]
        public String OrganizationId { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Namespace, HelpResource: Resources.UserAdminResources.Names.LocationNamespace_Help, NamespaceType: NamespaceTypes.Location, NamespaceUniqueMessageResource: Resources.UserAdminResources.Names.OrganizationLocation_NamespaceInUse, FieldType: FieldTypes.NameSpace, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public String LocationNamespace { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Admin_Contact, FieldType: FieldTypes.Picker, PickerType: Constants.PeoplePicker, PickerFor: nameof(AdminContactId), ResourceType: typeof(Resources.UserAdminResources))]
        public String AdminContact { get; set; }
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String AdminContactId { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Technical_Contact, FieldType: FieldTypes.Picker, PickerType: Constants.PeoplePicker, PickerFor: nameof(TechnicalContactId), ResourceType: typeof(Resources.UserAdminResources))]
        public String TechnicalContact { get; set; }
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String TechnicalContactId { get; set; }


        public override void MapToOrganizationLocation(OrgLocation location)
        {
            base.MapToOrganizationLocation(location);
            location.Namespace = LocationNamespace;
            location.AdminContact = EntityHeader.Create(AdminContactId, AdminContact);
            location.TechnicalContact = EntityHeader.Create(AdminContactId, TechnicalContact);
        }

        public static CreateLocationViewModel CreateNew(EntityHeader org, EntityHeader user)
        {
            return new CreateLocationViewModel()
            {
                OrganizationId = org.Id,
                AdminContact = user.Text,
                AdminContactId = user.Id,
                TechnicalContact = user.Text,
                TechnicalContactId = user.Id,
            };
        }
    }
}

