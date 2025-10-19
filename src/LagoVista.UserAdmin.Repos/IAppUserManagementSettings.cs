// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 932760f9b2f214ba7ccb8819bde8f72fbb853c3845726040e995a0c3965fb87e
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Repos
{
    public interface IUserAdminSettings
    {
        IConnectionSettings UserStorage { get; }
        IConnectionSettings UserTableStorage { get; }
        IConnectionSettings AccessLogTableStorage { get; }

        
        TimeSpan AccessTokenExpiresTimeSpan { get; }
        TimeSpan RefreshTokenExpiresTimeSpan { get; }

        bool ShouldConsolidateCollections { get; }
    }
}
                                                                                                                                                                                                                               