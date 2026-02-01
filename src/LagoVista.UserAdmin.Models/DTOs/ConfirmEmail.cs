// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2ec908f9460f4419f0857d75c4713e636ead102fae837402f766bc7127427651
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    [EntityDescription(
        Domains.UserDomain, UserAdminResources.Names.ConfirmEmail_Name, UserAdminResources.Names.ConfirmEmail_Help,
        UserAdminResources.Names.ConfirmEmail_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "security", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "userdomain,security,runtimeartifact,restricted")]
    public class ConfirmEmail
    {
        [JsonProperty("receivedCode")]
        public string ReceivedCode { get; set; }
    }
}
