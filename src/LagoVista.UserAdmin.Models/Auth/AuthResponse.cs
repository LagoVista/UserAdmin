// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b8acb550359808fc87cd719ec04dfbc0351db4568baba3de45e6a9b792cf7894
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Auth
{
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.AuthResponse_Name, UserAdminResources.Names.AuthResponse_Help,
        UserAdminResources.Names.AuthResponse_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "login", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "authdomain,login,runtimeartifact")]
    public class AuthResponse
    {
        public AuthResponse()
        {
            Roles = new List<EntityHeader>();
        }

        public string AccessToken { get; set; }

        public string AccessTokenExpiresUTC { get; set; }

        public string RefreshToken { get; set; }

        public string RefreshTokenExpiresUTC { get; set; }

        public string AppInstanceId { get; set; }

        public bool IsLockedOut { get; set; }

        public AppUser AppUser { get; set; }

        public EntityHeader User { get; set; }
        public EntityHeader Org { get; set; }
        public List<EntityHeader> Roles { get; set; }

        public string RedirectUri { get; set; }
    }
}
