// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 77cf2a3bc3853fee091b72da86dcd338e81e94f0f86c304eef84e760ba2707b0
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using System;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.CreateLocationVM_Title, UserAdminResources.Names.CreateOrganizationVM_Help, UserAdminResources.Names.CreateOrganizationVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class CreateLocationViewModel : LocationViewModel
    {
        [FormField(FieldType: FieldTypes.Hidden)]
        public String OrganizationId { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, HelpResource: UserAdminResources.Names.LocationNamespace_Help, NamespaceType: NamespaceTypes.Location, NamespaceUniqueMessageResource: UserAdminResources.Names.OrganizationLocation_NamespaceInUse, FieldType: FieldTypes.NameSpace, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String LocationNamespace { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Admin_Contact, FieldType: FieldTypes.Picker, PickerType: Constants.PeoplePicker, PickerFor: nameof(AdminContactId), ResourceType: typeof(UserAdminResources))]
        public String AdminContact { get; set; }
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String AdminContactId { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Technical_Contact, FieldType: FieldTypes.Picker, PickerType: Constants.PeoplePicker, PickerFor: nameof(TechnicalContactId), ResourceType: typeof(UserAdminResources))]
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

