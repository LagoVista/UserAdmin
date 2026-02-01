// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a941aa7a8987450659cdc97058f62418dbb03cf57bf38a6e65642441f417f333
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Auth
{
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.AcceptInviteResponse_Name, UserAdminResources.Names.AcceptInviteResponse_Help,
        UserAdminResources.Names.AcceptInviteResponse_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "invites", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 10, IndexTagsCsv: "authdomain,invites,runtimeartifact")]
    public class AcceptInviteResponse
    {
        public string OriginalEmail { get; set; }
        public string RedirectPage { get; set; }
        public string ResponseMessage { get; set; }
        public EntityHeader Customer { get; set; }
        public EntityHeader CustomerContact { get; set; }
        public EntityHeader EndUserAppOrg { get; set; }
    }
}
