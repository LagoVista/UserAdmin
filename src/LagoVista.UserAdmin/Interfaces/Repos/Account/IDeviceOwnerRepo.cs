// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: dd25fde56a8ddf2bf782be954b8819380899bde2dd69f94ea3f352dbb5e1e8d4
// IndexVersion: 2
// --- END CODE INDEX META ---
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
