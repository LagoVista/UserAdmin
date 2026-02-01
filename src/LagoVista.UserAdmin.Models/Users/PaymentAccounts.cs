// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0f5d39d607548e6c683f1f82c9eab53440873fcfc587ef0d4552a6d2fa0d06af
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Users
{
    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.PaymentAccounts_Name, UserAdminResources.Names.PaymentAccounts_Help,
        UserAdminResources.Names.PaymentAccounts_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "subscriptions", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "organizationdomain,subscriptions,configuration,restricted")]
    public class PaymentAccounts
    {
        public string PaymentAccount1 { get; set; }
        public string RoutingAccount1 { get; set; }

        public string PaymentAccount2 { get; set; }
        public string RoutingAccount2 { get; set; }
    }
}
