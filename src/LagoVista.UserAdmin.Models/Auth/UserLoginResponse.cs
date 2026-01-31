// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 09eedcc05af30f4d708bf3910f30fd491571dc12cf69cfde84e07d067007f79f
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Diagnostics;

namespace LagoVista.UserAdmin.Models.Auth
{
    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.UserLoginResponse_Name,
        UserAdminResources.Names.UserLoginResponse_Help,
        UserAdminResources.Names.UserLoginResponse_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
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
