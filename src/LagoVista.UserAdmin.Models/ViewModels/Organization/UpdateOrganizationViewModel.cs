// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cd61a86f6c03a824b50b4e22468afa254292399ea95d33670934a8ff8919e1c4
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;
using System;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationViewModels, UserAdminResources.Names.UpdateOrganizationVM_Title, UserAdminResources.Names.UpdateOrganizationVM_Help, UserAdminResources.Names.UpdateOrganizationVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class UpdateOrganizationViewModel : OrganizationViewModel, IValidateable
    {
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String OrganziationId { get; set; }

        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String LastUpdatedDate { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_Primary_Location, FieldType: FieldTypes.Picker, PickerType: Constants.LocationPicker, PickerFor: (nameof(PrimaryLocationId)), ResourceType: typeof(UserAdminResources))]
        public String PrimaryLocation { get; set; }
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String PrimaryLocationId { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Admin_Contact, FieldType: FieldTypes.Picker, PickerType: Constants.PeoplePicker, PickerFor: nameof(AdminContactId), ResourceType: typeof(UserAdminResources))]
        public String AdminContact { get; set; }
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String AdminContactId { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Billing_Contact, FieldType: FieldTypes.Picker, PickerType: Constants.PeoplePicker, PickerFor: nameof(BillingContactId), ResourceType: typeof(UserAdminResources))]
        public String BillingContact { get; set; }
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String BillingContactId { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Technical_Contact, FieldType: FieldTypes.Picker, PickerType: Constants.PeoplePicker, PickerFor: nameof(TechnicalContactId), ResourceType: typeof(UserAdminResources))]
        public String TechnicalContact { get; set; }
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String TechnicalContactId { get; set; }

        public void MapToOrganziation(Models.Orgs.Organization organization)
        {
            organization.Name = Name;
            organization.WebSite = WebSite;
            organization.PrimaryLocation = EntityHeader.Create(PrimaryLocationId, PrimaryLocation);
            organization.AdminContact = EntityHeader.Create(AdminContactId, AdminContact);
            organization.BillingContact = EntityHeader.Create(BillingContactId, BillingContact);
            organization.TechnicalContact = EntityHeader.Create(TechnicalContactId, TechnicalContact);
        }

        public static UpdateOrganizationViewModel CreateFromOrg(Models.Orgs.Organization org)
        {
            return new UpdateOrganizationViewModel()
            {
                OrganziationId = org.Id,
                LastUpdatedDate = org.LastUpdatedDate,
                Name = org.Name,
                Namespace = org.Namespace,
                WebSite = org.WebSite,
                PrimaryLocation = org.PrimaryLocation.Text,
                PrimaryLocationId = org.PrimaryLocation.Text,
                AdminContact = org.AdminContact.Text,
                AdminContactId = org.AdminContact.Id,
                BillingContact = org.BillingContact.Text,
                BillingContactId = org.BillingContact.Id,
                TechnicalContact = org.TechnicalContact.Id,
                TechnicalContactId = org.TechnicalContact.Id,
            };
        }
    }
}
