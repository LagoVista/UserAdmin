// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0801dff43253a58b717ac5e188be4edf4fcf975574fcd9b3555cd06794f4e57a
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Auth
{
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.AuthLoginRequest_Name, UserAdminResources.Names.AuthLoginRequest_Help,
        UserAdminResources.Names.AuthLoginRequest_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "login", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "authdomain,login,runtimeartifact")]
    public class AuthLoginRequest
    {
        public string EndUserAppOrgId { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string InviteId { get; set; }
        public bool LockoutOnFailure { get; set; }
    }
}
