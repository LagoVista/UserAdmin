using LagoVista.Core.PlatformSupport;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LagoVista.Core.Models;
using System;
using LagoVista.CloudStorage.Storage;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class OrgUserRepo : TableStorageBase<OrgUser>, IOrgUserRepo
    {
        public OrgUserRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }
       
        public async Task<bool> QueryOrgHasUserAsync(string orgId, string userId)
        {
            return (await base.GetAsync(orgId, OrgUser.CreateRowKey(orgId, userId),false)) != null;
        }

        public async Task AddOrgUserAsync(OrgUser user)
        {
            await InsertAsync(user);
        }

        public Task<IEnumerable<OrgUser>> GetOrgsForUserAsync(string userId)
        {
            return GetByFilterAsync(FilterOptions.Create(nameof(OrgUser.UserId), FilterOptions.Operators.Equals, userId));
        }

        public Task<IEnumerable<OrgUser>> GetUsersForOrgAsync(string orgId)
        {
            return GetByParitionIdAsync(orgId);
        }

        public Task RemoveUserFromOrgAsync(string orgid, string userid, EntityHeader removedBy)
        {
            var rowKey = OrgUser.CreateRowKey(orgid, userid);
            return RemoveAsync(orgid, rowKey);
        }

        public async Task<bool> QueryOrgHasUserByEmailAsync(string orgId, string email)
        {
            return (await GetByFilterAsync(
                FilterOptions.Create(nameof(OrgUser.Email), FilterOptions.Operators.Equals, email.ToUpper()),
                FilterOptions.Create(nameof(OrgUser.PartitionKey), FilterOptions.Operators.Equals, orgId)
                )).ToList().Any();
        }

        public async  Task<bool> IsUserOrgAdminAsync(string orgId, string userId)
        {
            var orgUser = await GetOrgUserAsync(orgId, userId);
            return orgUser.IsOrgAdmin;
        }
       
        public async Task<OrgUser> GetOrgUserAsync(string orgId, string userId)
        {
            return (await GetByFilterAsync(
                FilterOptions.Create(nameof(OrgUser.UserId), FilterOptions.Operators.Equals, userId.ToUpper()),
                FilterOptions.Create(nameof(OrgUser.PartitionKey), FilterOptions.Operators.Equals, orgId)
                )).FirstOrDefault();
        }

        public Task UpdateOrgUserAsync(OrgUser orgUser)
        {
            return this.UpdateAsync(orgUser);
        }

        public async Task<bool> IsAppBuilderAsync(string orgId, string userId)
        {
            var orgUser = await GetOrgUserAsync(orgId, userId);
            return orgUser.IsAppBuilder;
        }
    }
}
