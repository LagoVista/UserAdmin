// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c726e2bdf892302858863640541b67847d29613809e9c9ba007976971b6d9932
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Apps;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Apps
{
    public interface IAppInstanceRepo
    {
        Task AddAppInstanceAsync(AppInstance appInstance);

        Task UpdateAppInstanceAsync(AppInstance appInstance);
        Task<IEnumerable<AppInstance>> GetForUserAsync(string userId);

        Task<AppInstance> GetAppInstanceAsync(string userId, string appInstanceId);
    }
}
