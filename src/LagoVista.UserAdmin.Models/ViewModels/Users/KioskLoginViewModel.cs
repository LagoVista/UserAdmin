// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ee6fd5675666fc6f9129d60c5ed3824fea094dec1996bd9c945e7ac8f4c0724d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models;

namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.KioskLoginViewModel_Name, UserAdminResources.Names.KioskLoginViewModel_Help,
        UserAdminResources.Names.KioskLoginViewModel_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "login", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "authdomain,login,runtimeartifact")]
    public class KioskLoginViewModel
    {
        public string ClientId { get; set; }
        public string ApiKey { get; set; }
    }
}
