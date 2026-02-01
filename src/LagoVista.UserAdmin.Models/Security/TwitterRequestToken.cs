// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8b3966b78053e2e295804e027f8159884de245e66ab9278f7ec6ef9436419e6b
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.TwitterRequestToken_Name, UserAdminResources.Names.TwitterRequestToken_Help,
        UserAdminResources.Names.TwitterRequestToken_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "integrations", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "organizationdomain,integrations,runtimeartifact,token")]
    public class TwitterRequestToken
    {
        public string Token { get; set; }
        public string TokenSecret { get; set; }
        public bool CallbackConfirmed { get; set; }
    }

    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.TwitterAccessToken_Name,
        UserAdminResources.Names.TwitterAccessToken_Help,
        UserAdminResources.Names.TwitterAccessToken_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class TwitterAccessToken : TwitterRequestToken
    {
        public string UserId { get; set; }
        public string ScreenName { get; set; }
    }
}
