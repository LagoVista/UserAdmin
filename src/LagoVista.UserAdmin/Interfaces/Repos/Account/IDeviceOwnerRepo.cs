using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Account
{
    public interface IDeviceOwnerRepo
    {
        Task AddUserAsync(DeviceOwnerUser user);
        Task UpdateUserAsync(DeviceOwnerUser user);
        Task DeleteUserAsync(string orgId, string id);
        Task<DeviceOwnerUser> FindByIdAsync(string userId);
        Task<DeviceOwnerUser> FindByPhoneNumberAsync(string userId);
        Task<DeviceOwnerUser> FindByNameAsync(string userName);
        Task<DeviceOwnerUser> FindByEmailAsync(string email);

        Task<DeviceOwnerUser> AddOwnedDeviceAsync(string orgId, string ownerId, DeviceOwnerDevices device);
        Task<DeviceOwnerUser> UpdateOwnedDeviceAsync(string orgId, string ownerId, DeviceOwnerDevices device);
        Task<DeviceOwnerUser> RemoveOwnedDeviceAsync(string orgId, string ownerId, string id);
    }
}
