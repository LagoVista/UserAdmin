using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos
{
    public interface IDeviceOwnerRelationalRepo
    {
        Task AddUserAsync(DeviceOwnerUser downerUser);
        Task AddOwnedDeviceAsync(NormalizedId32 orgId, NormalizedId32 userId, DeviceOwnerDevices device);

        Task<DeviceOwnerUser> FindByIdAsync(NormalizedId32 id);
        Task DeleteUserAsync(NormalizedId32 id);
        Task<DeviceOwnerUser> FindByNameAsync(string name);
        Task UpdateUserAsync(DeviceOwnerUser downerUser);

        Task UpdateOwnedDeviceAsync(NormalizedId32 orgId, DeviceOwnerDevices device);

        Task RemoveOwnedDeviceAsync(NormalizedId32 orgId, NormalizedId32 deviceId);
    }
}
