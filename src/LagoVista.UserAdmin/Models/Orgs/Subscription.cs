using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Subscription_Title, UserAdminResources.Names.Subscription_Help, UserAdminResources.Names.Subscription_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Subscription : IValidateable, IKeyedEntity,  INamedEntity
    {
        public Guid Id { get; set; }

        public String OrgId { get; set; }

        public string CreatedById { get; set; }

        public string LastUpdatedById { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastUpdatedDate { get; set; }

        public string PaymentToken { get; set; }
        public DateTime? PaymentTokenDate { get; set; }

        public string PaymentTokenStatus { get; set; }

        public String Status { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: Resources.UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key, RegExValidationMessageResource: Resources.UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, FieldType:FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources), IsRequired: false)]
        public string Description { get; set; }

        public SubscriptionSummary CreateSummary()
        {
            return new SubscriptionSummary()
            {
                Id = Id,
                Name = Name,
                PaymentTokenStatus = PaymentTokenStatus,
                Key = Key
            };
        }
    }    

    public class SubscriptionSummary
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string PaymentTokenStatus { get; set; }
        public string Key { get; set; }
    }
}
