// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1933585895d41155443e6dcda7d4596bc5d00554580dc19712b51b56bcbeabad
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.User
{
    public class AdminConnectivitySettings : IUserAdminSettings
    {
        public IConnectionSettings UserStorage => LagoVista.CloudStorage.Utils.TestConnections.ProductionDocDB;

        public IConnectionSettings UserTableStorage => LagoVista.CloudStorage.Utils.TestConnections.ProductionTableStorageDB;

        public IConnectionSettings AccessLogTableStorage => LagoVista.CloudStorage.Utils.TestConnections.ProductionTableStorageDB;

        public TimeSpan AccessTokenExpiresTimeSpan => TimeSpan.FromMinutes(15);

        public TimeSpan RefreshTokenExpiresTimeSpan => TimeSpan.FromDays(2);

        public bool ShouldConsolidateCollections => true;
    }
}
