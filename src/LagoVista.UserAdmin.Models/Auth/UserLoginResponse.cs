// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 09eedcc05af30f4d708bf3910f30fd491571dc12cf69cfde84e07d067007f79f
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace LagoVista.UserAdmin.Models.Auth
{
    /// <summary>
    /// Legacy login response retained for downstream compatibility.
    /// New authentication flows should use AuthenticationResponse.
    /// </summary>
    [Obsolete("UserLoginResponse is retained for compatibility only. New authentication flows should use AuthenticationResponse.", false)]
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.UserLoginResponse_Name, UserAdminResources.Names.UserLoginResponse_Help,
        UserAdminResources.Names.UserLoginResponse_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "login", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 10, IndexTagsCsv: "authdomain,login,runtimeartifact")]
    public class UserLoginResponse : EntityHeader
    {
        private Stopwatch _sw;

        public UserLoginResponse()
        {
            _sw = Stopwatch.StartNew();
        }

        public AppUser User { get; set; }

        public UserFavorites Favorites { get; set; }
        public MostRecentlyUsed MostRecentlyUsed { get; set; }

        public List<Metric> AuthMetrics { get; } = new List<Metric>();

        public void AddAuthMetric(string name)
        {
            AuthMetrics.Add(new Metric(name, _sw.ElapsedMilliseconds));
            _sw = Stopwatch.StartNew();
        }

        public AuthenticationResponseState AuthenticationState { get; set; }

        public string AuthenticationReasonCode { get; set; }

        public string PendingIdentityId { get; set; }

        public string MaskedEmail { get; set; }

        public string Provider { get; set; }

        public string InviteId { get; set; }

        public bool CanEnterApplication => AuthenticationState == AuthenticationResponseState.Authenticated;

        public string ResponseMessage { get; set; }

        public string RedirectPage { get; set; }

        public class Metric
        {
            public Metric(string name, long ms)
            {
                Name = name;
                Ms = ms;
            }

            public string Name { get; set; }
            public long Ms { get; set; }
        }
    }
}