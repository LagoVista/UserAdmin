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
    public class Subscription : UserAdminModelBase, IOwnedEntity, IValidateable, IKeyedEntity, INoSQLEntity, INamedEntity
    {
        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: Resources.UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key, RegExValidationMessageResource: Resources.UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, FieldType:FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Description { get; set; }

        public SubscriptionSummary CreateSummary()
        {
            return new SubscriptionSummary()
            {
                Description = Description,
                Id = Id,
                IsPublic = IsPublic,
                Key = Key,
                Name = Name
            };
        }
    }

    public class SubscriptionSummary : SummaryData
    {

    }
}
