using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.PasskeyCredentialSummary_Name, UserAdminResources.Names.PasskeyCredentialSummary_Help,
        UserAdminResources.Names.PasskeyCredentialSummary_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "passkeys", ModelType: EntityDescriptionAttribute.ModelTypes.DomainEntity, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Aux,
        IndexPriority: 30, IndexTagsCsv: "authdomain,passkeys,summary")]
    public class PasskeyCredentialSummary
    {
        public string CredentialId { get; set; }
        public string Name { get; set; }
        public string CreatedUtc { get; set; }
        public string LastUsedUtc { get; set; }
    }
}
