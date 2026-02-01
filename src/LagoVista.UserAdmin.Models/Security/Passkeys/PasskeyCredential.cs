using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using System;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.PasskeyCredential_Name, UserAdminResources.Names.PasskeyCredential_Help,
        UserAdminResources.Names.PasskeyCredential_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "passkeys", ModelType: EntityDescriptionAttribute.ModelTypes.DomainEntity, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "authdomain,passkeys,domainentity")]
    public class PasskeyCredential
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        public string UserId { get; set; }
        public string RpId { get; set; }

        // base64url
        public string CredentialId { get; set; }

        // base64url (COSE key bytes)
        public string PublicKey { get; set; }

        public uint SignCount { get; set; }

        public string Name { get; set; }

        public string CreatedUtc { get; set; }
        public string LastUsedUtc { get; set; }

        [JsonIgnore]
        public EntityHeader User { get; set; }
    }
}
