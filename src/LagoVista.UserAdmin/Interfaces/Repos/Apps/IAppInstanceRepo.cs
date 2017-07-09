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
