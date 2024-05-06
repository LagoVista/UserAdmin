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
