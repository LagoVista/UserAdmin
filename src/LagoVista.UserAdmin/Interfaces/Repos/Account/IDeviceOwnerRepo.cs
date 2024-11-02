using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Account
{
    public interface IDeviceOwnerRepo
    {
        Task<InvokeResult> AddUserAsync(DeviceOwnerUser user);
        Task<InvokeResult> UpdateUserAsync(DeviceOwnerUser user);
        Task<InvokeResult> DeleteUserAsync(string orgId, string id);
        Task<DeviceOwnerUser> FindByIdAsync(string userId);
        Task<DeviceOwnerUser> FindByPhoneNumberAsync(string userId);
        Task<DeviceOwnerUser> FindByNameAsync(string userName);
        Task<DeviceOwnerUser> FindByEmailAsync(string email);

        Task<DeviceOwnerUser> AddOwnedDeviceAsync(string orgId, string ownerId, DeviceOwnerDevices device);
        Task<DeviceOwnerUser> UpdateOwnedDeviceAsync(string orgId, string ownerId, DeviceOwnerDevices device);
        Task<DeviceOwnerUser> RemoveOwnedDeviceAsync(string orgId, string ownerId, string id);
        Task<ListResponse<DeviceOwnerUser>> GetDeviceOwnersForDeviceAsync(string ownedDeviceId, ListRequest listRequest);
        Task<ListResponse<DeviceOwnerUserSummary>> GetAllAsync(ListRequest listRequest);
    }
}
