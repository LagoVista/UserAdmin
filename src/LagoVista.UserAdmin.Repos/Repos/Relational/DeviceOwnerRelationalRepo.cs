using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Interfaces.AutoMapper;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Relational;
using LagoVista.Relational.DataContexts;
using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZstdSharp.Unsafe;

namespace LagoVista.UserAdmin.Repos.Repos.Relational
{
    [CriticalCoverage]
    internal class DeviceOwnerRelationalRepo : IDeviceOwnerRelationalRepo
    {
        ILagoVistaAutoMapper _autoMapper;
        BillingDataContext _context;
        IAdminLogger _logger;

        public DeviceOwnerRelationalRepo(BillingDataContext context, IAdminLogger adminLogger, ILagoVistaAutoMapper autoMapper, ISecureStorage secureStorage)
        {
            _autoMapper = autoMapper ?? throw new ArgumentNullException(nameof(autoMapper));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
        }

        public async Task AddOwnedDeviceAsync(NormalizedId32 orgId, NormalizedId32 userId, DeviceOwnerDevices device)
        {
            var dto = new OwnedDeviceDTO()
            {
                Id = GuidString36.Factory(),
                DeviceUniqueId = device.Id,
                DeviceId = device.Id,
                DeviceName = device.Device.Text,
                Discount = 0,
                DeviceOwnerUserId = userId,
                ProductId = Guid.Parse(device.Product.Id),
            };

            _context.DeviceOwnerUserDevices.Add(dto);
            await _context.SaveChangesAsync();
        }

        public async Task AddUserAsync(DeviceOwnerUser downerUser)
        {
            var timestamp = DateTime.UtcNow;

            var dto = new DeviceOwnerDTO()
            {              
                DeviceOwnerUserId = downerUser.Id,
                CreationDate = timestamp,
                Email = downerUser.EmailAddress,
                FullName = downerUser.Name,
                Phone = downerUser.PhoneNumber,
                LastUpdatedDate = timestamp,
            };

            _context.DeviceOwnerUser.Add(dto);
            await _context.SaveChangesAsync();
        }


        public Task DeleteUserAsync(NormalizedId32 id)
        {
            return _context.DeviceOwnerUserDevices
                    .Where(x => x.Id == id)
                    .ExecuteDeleteAsync();
        }

        public async Task<DeviceOwnerUser> FindByIdAsync(NormalizedId32 id)
        {
            var dto = await _context.DeviceOwnerUser
                      .SingleOrDefaultAsync(x => x.DeviceOwnerUserId == id);   

            return new DeviceOwnerUser()
            {
                Id = dto.DeviceOwnerUserId,
                EmailAddress = dto.Email,
                Name = dto.FullName,
                PhoneNumber = dto.Phone
            };
        }

        public async Task<DeviceOwnerUser> FindByNameAsync(string name)
        {
            var dto = await _context.DeviceOwnerUser
             .SingleOrDefaultAsync(x => x.Email == name);

            return new DeviceOwnerUser()
            {
                Id = dto.DeviceOwnerUserId,
                EmailAddress = dto.Email,
                Name = dto.FullName,
                PhoneNumber = dto.Phone
            };
        }
        public Task RemoveOwnedDeviceAsync(NormalizedId32 orgId, NormalizedId32 deviceId)
        {
            return _context.DeviceOwnerUserDevices
                    .Where(x => x.Id == deviceId)
                    .ExecuteDeleteAsync();
        }

        public Task UpdateOwnedDeviceAsync(NormalizedId32 orgId, DeviceOwnerDevices device)
        {
            return _context.DeviceOwnerUserDevices
                   .Where(x => x.DeviceOwnerUserId == device.Id)
                   .ExecuteUpdateAsync(setters => setters
                   .SetProperty(x => x.DeviceName, device.Device.Text));
        }

        public Task UpdateUserAsync(DeviceOwnerUser deviceOwner)
        {
            return _context.DeviceOwnerUser
                    .Where(x => x.DeviceOwnerUserId == deviceOwner.Id)
                    .ExecuteUpdateAsync(setters => setters
                    .SetProperty(x => x.Email, deviceOwner.EmailAddress)
                    .SetProperty(x => x.FullName, deviceOwner.Name)
                    .SetProperty(x => x.Phone, deviceOwner.PhoneNumber));
        }
    }

    public static class PhoneExtensions
    {
        public static string CleanPhoneNumber(this string phoneNumber)
        {
            if (String.IsNullOrEmpty(phoneNumber))
                return String.Empty;

            return phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        }
    }
}
/*''
 * 
 // --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 53e77235cb8476ca38202d64b7552866103fc22a7e6f5c3bfcfa7c9a55d3361a
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class DeviceOwnerRepo : DocumentDBRepoBase<DeviceOwnerUser>, IDeviceOwnerRepo
    {
        private IDeviceOwnerRepo _;

        public DeviceOwnerRepo(IRDBMSManager rdbmsUserManager, IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services)
            : base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
            _rdbmsUserManager = rdbmsUserManager ?? throw new ArgumentNullException(nameof(rdbmsUserManager));
        }

        protected override bool ShouldConsolidateCollections => true;

        public async Task<DeviceOwnerUser> AddOwnedDeviceAsync(string orgId,string ownerId, DeviceOwnerDevices device)
        {
            var owner = await FindByIdAsync(ownerId);
            owner.Devices.Add(device);
            await UpsertDocumentAsync(owner);
            await _rdbmsUserManager.AddOwnedDeviceAsync(ownerId, orgId, device);

            return owner;
        }

        public async Task<InvokeResult> AddUserAsync(DeviceOwnerUser user)
        {
            user.IsAnonymous = false;
            await CreateDocumentAsync(user);
            return await _rdbmsUserManager.AddDeviceOwnerAsync(user);
        }

        public async Task<InvokeResult> DeleteUserAsync(string orgId,string id)
        {
            await DeleteDocumentAsync(id);
            return await _rdbmsUserManager.DeleteDeviceOwnerAsync(orgId, id);
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

        public async Task<InvokeResult> UpdateUserAsync(DeviceOwnerUser user)
        {
            await UpsertDocumentAsync(user);
            return await _rdbmsUserManager.UpdateDeviceOwnerAsync(user);
        }
    }


}
*/