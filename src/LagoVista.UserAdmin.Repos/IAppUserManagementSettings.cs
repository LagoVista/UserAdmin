using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Repos
{
    public interface IAppUserManagementSettings
    {
        IConnectionSettings UserStorage { get; }
        IConnectionSettings UserTableStorage { get; }

        
        TimeSpan AccessTokenExpiresTimeSpan { get; }
        TimeSpan RefreshTokenExpiresTimeSpan { get; }

        bool ShouldConsolidateCollections { get; }
    }
}
