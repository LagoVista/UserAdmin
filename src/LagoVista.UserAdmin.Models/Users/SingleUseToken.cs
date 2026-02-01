// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 09db975f5ae6d96f51da9202fdf52351454a0fce2fd01e04663c88e9b52e47b0
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Users
{
    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.SingleUseToken_Name, UserAdminResources.Names.SingleUseToken_Help,
        UserAdminResources.Names.SingleUseToken_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "authassist", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "organizationdomain,authassist,runtimeartifact,token")]
    public class SingleUseToken
    {
        public string UserId { get; set; }
        public string Token { get; set; }
        public string Expires { get; set; }
    }
}
