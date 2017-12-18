﻿using LagoVista.Core.Attributes;
using System;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationViewModels, UserAdminResources.Names.UpdateLocationVM_Title, UserAdminResources.Names.UpdateLocationVM_Help, UserAdminResources.Names.UpdateLocatoinVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class UpdateLocationViewModel : LocationViewModel
    {
        [FormField(FieldType: FieldTypes.Hidden)]
        public String LocationId { get; set; }
  
        public String LastUpdatedDate { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, HelpResource: UserAdminResources.Names.LocationNamespace_Help, IsUserEditable:false, NamespaceType: NamespaceTypes.Location, NamespaceUniqueMessageResource: UserAdminResources.Names.OrganizationLocation_NamespaceInUse, FieldType: FieldTypes.NameSpace, IsRequired: true, ResourceType: typeof(UserAdminResources))]
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
            location.AdminContact = EntityHeader.Create(AdminContactId, AdminContact);
            location.TechnicalContact = EntityHeader.Create(AdminContactId, TechnicalContact);
        }

        public static UpdateLocationViewModel CreateForOrganizationLocation(OrgLocation loc)
        {
            return new Organization.UpdateLocationViewModel()
            {
                LocationId = loc.Id,
                LastUpdatedDate = loc.LastUpdatedDate,
                Addr1 = loc.Addr1,
                Addr2 = loc.Addr2,
                City = loc.City,
                StateProvince = loc.StateProvince,
                Country = loc.Country,
                AdminContact = loc.AdminContact.Text,
                AdminContactId = loc.AdminContact.Id,
                TechnicalContact = loc.TechnicalContact.Text,
                TechnicalContactId = loc.TechnicalContact.Id
            };
        }
    }
}
