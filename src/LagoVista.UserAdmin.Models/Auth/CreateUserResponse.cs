// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5e840bdd4de0c8ac51b13ed2244ba71239553a3d75b1cde7d715f083d2e35dae
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Auth
{
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.CreateUserResponse_Name, UserAdminResources.Names.CreateUserResponse_Help,
        UserAdminResources.Names.CreateUserResponse_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "login", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "authdomain,login,runtimeartifact")]
    public class CreateUserResponse : AuthResponse
    {
        public static CreateUserResponse FromAuthResponse(AuthResponse response)
        {
            return new CreateUserResponse()
            {
                User = response.User,
                AppUser = response.AppUser,
                AccessToken = response.AccessToken,
                AppInstanceId = response.AppInstanceId,
                AccessTokenExpiresUTC = response.AccessTokenExpiresUTC,
                Org = response.Org,
                IsLockedOut = response.IsLockedOut,
                RefreshToken = response.RefreshToken,
                RefreshTokenExpiresUTC = response.RefreshTokenExpiresUTC,
            };
        }

        public UserSetupStates UserSetupState { get; set; } = UserSetupStates.Unknown;

        public string ResponseMessage { get; set; }

        public string RedirectPage { get; set; }
    }
}
