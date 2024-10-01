using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class DeviceOwnerRepo : DocumentDBRepoBase<DeviceOwnerUser>, IDeviceOwnerRepo
    {
        private IRDBMSManager _rdbmsUserManager;

        public DeviceOwnerRepo(IRDBMSManager rdbmsUserManager, IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependnecyManager)
            : base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider, dependencyManager: dependnecyManager)
        {
            _rdbmsUserManager = rdbmsUserManager ?? throw new ArgumentNullException(nameof(rdbmsUserManager));
        }

        protected override bool ShouldConsolidateCollections => true;

        public async Task<DeviceOwnerUser> AddOwnedDeviceAsync(string orgId,string ownerId, DeviceOwnerDevices device)
        {
            var owner = await FindByIdAsync(ownerId);
            owner.Devices.Add(device);
            await UpdateUserAsync(owner);

            await _rdbmsUserManager.AddOwnedDeviceAsync(orgId, ownerId, device);

            return owner;
        }

        public async Task AddUserAsync(DeviceOwnerUser user)
        {
            user.IsAnonymous = false;
            await CreateDocumentAsync(user);
            await _rdbmsUserManager.AddDeviceOwnerAsync(user);
        }

        public async Task DeleteUserAsync(string orgId,string id)
        {
            await DeleteDocumentAsync(id);
            await _rdbmsUserManager.DeleteDeviceOwnerAsync(orgId, id);
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

        public async Task<DeviceOwnerUser> RemoveOwnedDeviceAsync(string orgId, string ownerId, string id)
        {
            var owner = await FindByIdAsync(ownerId);
            var existing = owner.Devices.SingleOrDefault(dev => dev.Id == id);
            owner.Devices.Remove(existing);
            await UpdateUserAsync(owner);

            await _rdbmsUserManager.RemoveOwnedDeviceAsync(orgId, id);

            return owner;      
        }

        public async Task<DeviceOwnerUser> UpdateOwnedDeviceAsync(string orgId, string ownerId, DeviceOwnerDevices device)
        {
            var owner = await FindByIdAsync(ownerId);
            var existing = owner.Devices.SingleOrDefault(dev => dev.Id == device.Id);

            await _rdbmsUserManager.UpdateOwnedDeviceAsync(orgId, device);
            // not much to update now, will likely do when we figure out the billing component.

            return owner;
        }

        public async Task UpdateUserAsync(DeviceOwnerUser user)
        {
            await UpsertDocumentAsync(user);
            await _rdbmsUserManager.AddDeviceOwnerAsync(user);
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
