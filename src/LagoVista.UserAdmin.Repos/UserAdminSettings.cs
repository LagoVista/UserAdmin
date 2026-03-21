using LagoVista.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos
{
    public class UserAdminSettings : IUserAdminSettings
    {
        public IConnectionSettings UserStorage { get; }

        public IConnectionSettings UserTableStorage { get; }

        public IConnectionSettings AccessLogTableStorage { get; }

        public TimeSpan AccessTokenExpiresTimeSpan { get; }

        public TimeSpan RefreshTokenExpiresTimeSpan { get; }

        public UserAdminSettings(IConfiguration configuration)
        {
            UserStorage = configuration.CreateDefaultDBStorageSettings();
            UserTableStorage = configuration.CreateDefaultTableStorageSettings();
            AccessLogTableStorage = configuration.CreateDefaultTableStorageSettings();

            AccessTokenExpiresTimeSpan = TimeSpan.FromMinutes(30);
            RefreshTokenExpiresTimeSpan = TimeSpan.FromDays(7);
        }
    }
}
