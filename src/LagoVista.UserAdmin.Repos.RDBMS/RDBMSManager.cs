using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;
using LagoVista.Core;
using System.Linq;
using LagoVista.UserAdmin.Models.Orgs;
using System.Text;
using System;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using LagoVista.Core.Models;
using System.Diagnostics;
using LagoVista.IoT.Logging.Loggers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RingCentral;
using LagoVista.UserAdmin.Repos.RDBMS.Models;
using Npgsql;
using System.Security.Cryptography;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public class RDBMSManager : IRDBMSManager
    {
        private readonly UserAdminDataContext _dataCtx;
        private readonly IRDBMSConnectionProvider _connectionProvider;
        private readonly IAdminLogger _adminLogger;
        public RDBMSManager(UserAdminDataContext dataContext, IRDBMSConnectionProvider connectionProvider, IAdminLogger adminLogger)
        {
            _dataCtx = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _adminLogger = adminLogger;
            _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        }

        public async Task<UserAdminDataContext> GetBillingDataContextAsync(string orgId)
        {
            var connection = await _connectionProvider.GetConnectionAsync(orgId);
            switch (connection.ConnectionType)
            {
                case RDBMSConnectionType.PosgreSQLDedicated:
                    
                    var options = new DbContextOptionsBuilder<UserAdminDataContext>().UseNpgsql(connection.ConnectionString);
                    if (connection.VerboseLogging)
                    {
                        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                        var logger = loggerFactory.CreateLogger<RDBMSManager>();

                        options = options.UseLoggerFactory(loggerFactory);
                    }

                    return new UserAdminDataContext(options.Options);
                default:
                    return _dataCtx;
            }
        }

        public async Task<InvokeResult> AddAppUserToOrgAsyncAsync(string orgId, AppUser user)
        {
            var dbUser = new Models.RDBMSAppUser()
            {
                AppUserId = user.Id,
                CreationDate = user.CreationDate.ToDateTime(),
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                LastUpdatedDate = user.LastUpdatedDate.ToDateTime()
            };            

            var ctx = await GetBillingDataContextAsync(orgId);

            if (await ctx.AppUser.Where(usr => usr.AppUserId == user.Id).AnyAsync())
                return InvokeResult.Success;

            ctx.AppUser.Add(dbUser);
            await ctx.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddOrgAsync(Organization org)
        {
            var dbOrg = new Models.RDBMSOrg()
            {
                CreationDate = org.CreationDate.ToDateTime(),
                LastUpdatedDate = org.LastUpdatedDate.ToDateTime(),
                OrgBillingContactId = org.BillingContact.Id,
                OrgId = org.Id,
                OrgName = org.Name,
                Status = org.Status
            };

            var ctx = await GetBillingDataContextAsync(org.Id);
            ctx.Org.Add(dbOrg);
            await ctx.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> RemoveAppUserFromOrgAsync(string orgId, string userId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            var appUser = await ctx.AppUser.FindAsync(userId);
            ctx.AppUser.Remove(appUser);
            await ctx.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteOrgAsync(string orgId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            var org = await ctx.Org.FindAsync(orgId);
            if (org != null)
            {
                ctx.Org.Remove(org);
                await ctx.SaveChangesAsync();
            }

            return InvokeResult.Success;
        }

        public async Task<List<EntityHeader>> GetBillingContactOrgsForUserAsync(string orgId, string userId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            return  await ctx.Org.Where(org => org.OrgBillingContactId == userId).Select(org => new EntityHeader() { Id = org.OrgId, Text = org.OrgName }).ToListAsync();
        }

        public async Task<bool> HasBillingRecords(string orgId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            var query = new StringBuilder(@"
select o.*
 from BillingEvents be
 join Subscription subs on be.SubscriptionId = subs.Id
 join Org o on subs.OrgId = o.OrgId
 where subs.OrgId = @orgId
");
            
                var results = await ctx.Org.FromSqlRaw(query.ToString(), ctx.Database.IsNpgsql() ? new NpgsqlParameter("@orgid", orgId) :
                    new SqlParameter("@orgid", orgId)).ToListAsync();
                return results.Count > 0;
        }

        public async Task<InvokeResult> UpdateAppUserAsync(string orgId, AppUser user)
        {
            var sw = Stopwatch.StartNew();
            var ctx = await GetBillingDataContextAsync(orgId);
            var loadedUser = ctx.AppUser.Where(usr => usr.AppUserId == user.Id).FirstOrDefault();            
            _adminLogger.Trace($"[RDBMSManager__UpdateAppuserAsync] Found User: {sw.ElapsedMilliseconds}ms");
            loadedUser.FullName = user.Name;
            loadedUser.Email = user.Email;
            loadedUser.CreationDate = user.CreationDate.ToDateTime();
            loadedUser.LastUpdatedDate = user.LastUpdatedDate.ToDateTime();

            ctx.AppUser.Update(loadedUser);            
            await ctx.SaveChangesAsync();
            _adminLogger.Trace($"[RDBMSManager__UpdateAppuserAsync] Updated User: {sw.ElapsedMilliseconds}ms");
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateOrgAsync(Organization org)
        {
            var ctx = await GetBillingDataContextAsync(org.Id);
            var loadedOrg = ctx.Org.Where(usr => usr.OrgId == org.Id).FirstOrDefault();
            loadedOrg.CreationDate = org.CreationDate.ToDateTime();
            loadedOrg.LastUpdatedDate = org.LastUpdatedDate.ToDateTime();
            loadedOrg.OrgName = org.Name;
            loadedOrg.Status = org.Status;
            loadedOrg.OrgBillingContactId = org.BillingContact.Id;
            ctx.Org.Update(loadedOrg);
            await ctx.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<bool> UserExistsAsync(string orgId, string userId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            return await ctx.AppUser.AnyAsync(org => org.AppUserId == userId);
        }

        public async Task<bool> OrgExistsAsync(string orgId)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            return await ctx.Org.AnyAsync(org => org.OrgId == orgId);
        }

        public async Task<InvokeResult> AddDeviceOwnerAsync(DeviceOwnerUser deviceOwner)
        {
            var dbUser = new RDBMSDeviceOwnerUser()
            {
                FullName = $"{deviceOwner.FirstName} {deviceOwner.LastName}",
                CreationDate = deviceOwner.CreationDate.ToDateTime(),
                LastUpdatedDate = deviceOwner.LastUpdatedDate.ToDateTime(),
                Email = deviceOwner.EmailAddress,
                Phone = deviceOwner.PhoneNumber,
                DeviceOwnerUserId = deviceOwner.Id                
            };

            var ctx = await GetBillingDataContextAsync(deviceOwner.OwnerOrganization.Id);
            ctx.DeviceOwnerUser.Add(dbUser);
            await ctx.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateDeviceOwnerAsync(DeviceOwnerUser updateDeviceOwner)
        {
            var ctx = await GetBillingDataContextAsync(updateDeviceOwner.OwnerOrganization.Id);
            var owner = await ctx.DeviceOwnerUser.SingleOrDefaultAsync(dev => dev.DeviceOwnerUserId == updateDeviceOwner.Id);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteDeviceOwnerAsync(string orgId, string id)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            var owner = await ctx.DeviceOwnerUser.SingleOrDefaultAsync(dev => dev.DeviceOwnerUserId == id);
            if (owner != null)
            {
                ctx.DeviceOwnerUser.Remove(owner);
                await ctx.SaveChangesAsync();
            }

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddOwnedDeviceAsync(string deviceOwneruserId, string orgId, DeviceOwnerDevices device)
        {
            if (device.Product == null)
                throw new Exception("To add a device as a device user, it must be associate with a product from the product catalog.");

            var ownedDevice = new OwnedDevice()
            {
                Id = device.Id,
                DeviceUniqueId = device.Device.Id,
                DeviceId = device.DeviceId,
                DeviceName = device.Device.Text,
                DeviceOwnerUserId = deviceOwneruserId,
                ProductId = Guid.Parse(device.Product.Id),
                Discount = 0,
            };           

            var ctx = await GetBillingDataContextAsync(orgId);
            ctx.DeviceOwnerUserDevices.Add(ownedDevice);
            await ctx.SaveChangesAsync();
        
            return InvokeResult.Success;
        }

        public Task<InvokeResult> UpdateOwnedDeviceAsync(string orgId, DeviceOwnerDevices device)
        {
            return Task.FromResult(InvokeResult.Success);
        }

        public async Task<InvokeResult> RemoveOwnedDeviceAsync(string orgId, string id)
        {
            var ctx = await GetBillingDataContextAsync(orgId);
            var ownedDevice = await ctx.DeviceOwnerUserDevices.SingleOrDefaultAsync(dev => dev.Id == id);
            if (ownedDevice != null)
            {
                ctx.DeviceOwnerUserDevices.Remove(ownedDevice);
                await ctx.SaveChangesAsync();
            }

            return InvokeResult.Success;
        }
    }
}
