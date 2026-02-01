// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 60079e707839e12936e91b814db10a63f53fa50c0bb73b885707654ffd601021
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Users
{
    [EntityDescription(
        Domains.UserDomain, UserAdminResources.Names.CoreUserInfo_Name, UserAdminResources.Names.CoreUserInfo_Help,
        UserAdminResources.Names.CoreUserInfo_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "users", ModelType: EntityDescriptionAttribute.ModelTypes.DomainEntity, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Secondary,
        IndexPriority: 70, IndexTagsCsv: "userdomain,users,domainentity,profile")]
    public class CoreUserInfo : IValidateable
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_FirstName, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string FirstName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_LastName, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string LastName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_UserTitle, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string Title { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Bio, FieldType: FieldTypes.MultiLineText, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string Bio { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_PhoneNumber, FieldType: FieldTypes.Phone, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string PhoneNumber { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_SSN, HelpResource: UserAdminResources.Names.AppUser_SSN_Help,
           SecureIdFieldName: nameof(SsnSecretId), FieldType: FieldTypes.Secret, ResourceType: typeof(UserAdminResources))]
        public string Ssn { get; set; }

        public string SsnSecretId { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_TeamsAccountName, FieldType: FieldTypes.Text, IsUserEditable: true, ResourceType: typeof(UserAdminResources))]
        public string TeamsAccountName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Address1, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Address1 { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Address2, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Address2 { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_City, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string City { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_State, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string State { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Country, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Country { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_PostalCode, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string PostalCode { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_TimeZome, IsRequired: false, WaterMark: UserAdminResources.Names.Common_TimeZome_Picker, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader TimeZone { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_EmailSignature, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string EmailSignature { get; set; }
    }
}
