using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.PasskeyCredentialIndex_Name, UserAdminResources.Names.PasskeyCredentialIndex_Help,
        UserAdminResources.Names.PasskeyCredentialIndex_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "passkeys", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "authdomain,passkeys,configuration")]
    public class PasskeyCredentialIndex
    {
        public string UserId { get; set; }
        public string RpId { get; set; }
        public string CredentialId { get; set; }
        public string CreatedUtc { get; set; }
    }
}
