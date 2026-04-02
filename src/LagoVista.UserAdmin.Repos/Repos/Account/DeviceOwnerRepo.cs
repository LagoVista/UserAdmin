using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class DeviceOwnerRepo : DocumentDBRepoBase<DeviceOwnerUser>, IDeviceOwnerRepo
    {
        private IDeviceOwnerRelationalRepo _relationalRepo;

        public DeviceOwnerRepo(IDeviceOwnerRelationalRepo relationalRepo, IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services)
            : base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
            _relationalRepo = relationalRepo ?? throw new ArgumentNullException(nameof(relationalRepo));    
        }

        public async Task<DeviceOwnerUser> AddOwnedDeviceAsync(string orgId,string ownerId, DeviceOwnerDevices device)
        {
            var owner = await FindByIdAsync(ownerId);
            owner.Devices.Add(device);
            await UpsertDocumentAsync(owner);
            await _relationalRepo.AddOwnedDeviceAsync(orgId, ownerId, device);

            return owner;
        }

        public async Task<InvokeResult> AddUserAsync(DeviceOwnerUser user)
        {
            user.IsAnonymous = false;
            await CreateDocumentAsync(user);
            await _relationalRepo.AddUserAsync(user);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteUserAsync(string orgId,string id)
        {
            await DeleteDocumentAsync(id);
            await _relationalRepo.DeleteUserAsync(id);
            return InvokeResult.Success;
        }

        public async Task<DeviceOwnerUser> FindByEmailAsync(string email)
        {
            return (await QueryAsync(own => own.EmailAddress.ToUpper() == email.ToUpper())).SingleOrDefault();
        }

        public Task<DeviceOwnerUser> FindByIdAsync(string userId)
        {
            return GetDocumentAsync(userId, false);
        }

        public Task<DeviceOwnerUser> FindByNameAsync(string userName)
        {
            return FindByEmailAsync(userName);
        }

        public async Task<DeviceOwnerUser> FindByPhoneNumberAsync(string phone)
        {
            return (await QueryAsync(own => own.PhoneNumber == phone.CleanPhoneNumber())).SingleOrDefault();
        }

        public Task<ListResponse<DeviceOwnerUserSummary>> GetAllAsync(ListRequest listRequest)
        {
            return QuerySummaryAsync<DeviceOwnerUserSummary, DeviceOwnerUser>(rec => true, rec => rec.Name, listRequest);
        }

        public async Task<ListResponse<DeviceOwnerUser>> GetDeviceOwnersForDeviceAsync(string ownedDeviceId, ListRequest listRequest)
        {
            var query = @"SELECT *
                            FROM c
                            WHERE ARRAY_CONTAINS(c.Devices, {Device:{Id:@id}}, true)
                              AND c.EntityType = 'DeviceOwnerUser'";

            return await QueryAsync(query, listRequest, new CloudStorage.QueryParameter("@id", ownedDeviceId));
        }

        public async Task<DeviceOwnerUser> RemoveOwnedDeviceAsync(string orgId, string ownerId, string id)
        {
            var owner = await FindByIdAsync(ownerId);
            var existing = owner.Devices.SingleOrDefault(dev => dev.Id == id);
            owner.Devices.Remove(existing);
            await UpdateUserAsync(owner);
            await _relationalRepo.RemoveOwnedDeviceAsync(orgId, id);

            return owner;      
        }

        public async Task<DeviceOwnerUser> UpdateOwnedDeviceAsync(string orgId, string ownerId, DeviceOwnerDevices device)
        {
            var owner = await FindByIdAsync(ownerId);
            var existing = owner.Devices.SingleOrDefault(dev => dev.Id == device.Id);

            await _relationalRepo.UpdateOwnedDeviceAsync(orgId,  device);
            return owner;
        }

        public async Task<InvokeResult> UpdateUserAsync(DeviceOwnerUser user)
        {
            await UpsertDocumentAsync(user);
            await _relationalRepo.UpdateUserAsync(user);

            return InvokeResult.Success;
        }
    }

    public static class PhoneExtensions
    {
        public static string CleanPhoneNumber(this string phoneNumber) {
            if (String.IsNullOrEmpty(phoneNumber))
                return String.Empty;

            return phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        }
    }
}
