using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;
using LagoVista.Core;
using System.Linq;
using LagoVista.UserAdmin.Models.Orgs;
using System.Text;
using System;
using System.Data.SqlClient;
using System.Collections.Generic;
using LagoVista.Core.Models;

namespace LagoVista.UserAdmin.Repos.RDBMS
{
    public class RDBMSManager : IRDBMSManager
    {
        private readonly UserAdminDataContext _dataContext;
        private readonly IRDBMSConnectionSettings _connectionSettings;
        public RDBMSManager(UserAdminDataContext dataContext, IRDBMSConnectionSettings connectionSettings)
        {
            _dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            _connectionSettings = connectionSettings ?? throw new ArgumentNullException(nameof(connectionSettings));
        }

        public async Task<InvokeResult> AddAppUserAsync(AppUser user)
        {
            var dbUser = new Models.RDBMSAppUser()
            {
                AppUserId = user.Id,
                CreationDate = user.CreationDate.ToDateTime(),
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}",
                LastUpdatedDate = user.LastUpdatedDate.ToDateTime()
            };

            _dataContext.AppUser.Add(dbUser);
            await _dataContext.SaveChangesAsync();

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

            _dataContext.Org.Add(dbOrg);
            await _dataContext.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteAppUserAsync(string userId)
        {
            var appUser = await _dataContext.AppUser.FindAsync(userId);
            _dataContext.AppUser.Remove(appUser);
            await _dataContext.SaveChangesAsync();

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteOrgAsync(string orgId)
        {
            var org = await _dataContext.Org.FindAsync(orgId);
            if (org != null)
            {
                _dataContext.Org.Remove(org);
                await _dataContext.SaveChangesAsync();
            }

            return InvokeResult.Success;
        }

        public Task<List<EntityHeader>> GetBillingContactOrgsForUserAsync(string userId)
        {
            return Task.FromResult(_dataContext.Org.Where(org => org.OrgBillingContactId == userId).Select(org => new EntityHeader() { Id = org.OrgId, Text = org.OrgName }).ToList());
        }

        public async Task<bool> HasBillingRecords(string orgId)
        {
            var query = new StringBuilder(@"
select subs.id 
 from BillingEvents be
 join Subscription subs on be.SubscriptionId = subs.Id
 where subs.OrgId = @orgId
");

            var connectionString = $"Server=tcp:{_connectionSettings.DbConnectionSettings.Uri},1433;Initial Catalog={_connectionSettings.DbConnectionSettings.ResourceName};Persist Security Info=False;User ID={_connectionSettings.DbConnectionSettings.UserName};Password={_connectionSettings.DbConnectionSettings.Password};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            using(var cn = new SqlConnection(connectionString))
            using(var cmd = new SqlCommand())
            {
                cmd.Connection = cn;
                cmd.CommandText = query.ToString();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection.Open();
                cmd.Parameters.AddWithValue(@"orgId", orgId);
                using(var rdr = await cmd.ExecuteReaderAsync())
                {
                    return await rdr.ReadAsync();
                }
            }
        }

        public async Task<InvokeResult> UpdateAppUserAsync(AppUser user)
        {
            var loadedUser = _dataContext.AppUser.Where(usr => usr.AppUserId == user.Id).FirstOrDefault();
            loadedUser.FullName = user.Name;
            loadedUser.LastUpdatedDate = user.LastUpdatedDate.ToDateTime();

            _dataContext.AppUser.Update(loadedUser);
            await _dataContext.SaveChangesAsync();
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateOrgAsync(Organization org)
        {
            var loadedOrg = _dataContext.Org.Where(usr => usr.OrgId == org.Id).FirstOrDefault();
            loadedOrg.LastUpdatedDate = org.LastUpdatedDate.ToDateTime();
            loadedOrg.OrgName = org.Name;
            loadedOrg.Status = org.Status;
            loadedOrg.OrgBillingContactId = org.BillingContact.Id;

            _dataContext.Org.Update(loadedOrg);
            await _dataContext.SaveChangesAsync();

            return InvokeResult.Success;
        }
    }
}
