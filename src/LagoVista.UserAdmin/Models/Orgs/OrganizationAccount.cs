using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.OrganizationAccountRole_Title, UserAdminResources.Names.Organization_Help, UserAdminResources.Names.Organization_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class OrganizationAccount : TableStorageEntity, IValidateable, ITableStorageAuditableEntity
    {
        /* Id will becomposed o */

        public OrganizationAccount(string organizationId, string accountId)
        {
            RowKey = CreateRowKey(organizationId, accountId);
            PartitionKey = $"{organizationId}";
            AccountId = accountId;
            OrganizationId = organizationId;
        }

        [FormField(IsRequired: true)]
        public String AccountId { get; set; }
        [FormField(IsRequired: true)]
        public String AccountName { get; set; }
        [FormField(IsRequired: true)]
        public String OrganizationId { get; set; }
        [FormField(IsRequired: true)]
        public String OrganizationName { get; set; }
        [FormField(IsRequired: true)]
        public String ProfileImageUrl { get; set; }
        [FormField(IsRequired: true)]
        public String Email { get; set; }
        [FormField(IsRequired: true)]
        public String CreatedBy { get; set; }
        [FormField(IsRequired: true)]
        public String CreatedById { get; set; }
        [FormField(IsRequired: true)]
        public String CreationDate { get; set; }
        [FormField(IsRequired: true)]
        public String LastUpdatedBy { get; set; }
        [FormField(IsRequired: true)]
        public String LastUpdatedById { get; set; }
        [FormField(IsRequired: true)]
        public String LastUpdatedDate { get; set; }

        public static String CreateRowKey(String organizationId, String accountId)
        {
            return $"{organizationId}.{accountId}";
        }
    }
}
