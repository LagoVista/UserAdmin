// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 009438824a39e01fdd79ae01bb36d1825582d573f922755f1578d30c9ca32ad5
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    [EntityDescription(
        Domains.UserDomain, UserAdminResources.Names.InviteUser_Name, UserAdminResources.Names.InviteUser_Help, UserAdminResources.Names.InviteUser_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "invites", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 10, IndexTagsCsv: "userdomain,invites,runtimeartifact")]
    public class InviteUser
    {
        [JsonProperty("inviteFromuserId")]
        public string InviteFromUserId { get; set; }

        [JsonProperty("inviteToOrgId")]
        public string InviteToOrgId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("endUserOrgApp")]
        public EntityHeader EndUserAppOrg { get; set; }

        [JsonProperty("customer")]
        public EntityHeader Customer { get; set; }

        [JsonProperty("customerContact")]
        public EntityHeader CustomerContact { get; set; }
    }
}
