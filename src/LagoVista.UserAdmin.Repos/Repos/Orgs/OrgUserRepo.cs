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
using LagoVista.Core.Interfaces;
using Newtonsoft.Json;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class OrgUserRepo : TableStorageBase<OrgUser>, IOrgUserRepo
    {
        private readonly ICacheProvider _cacheProvider;

        public OrgUserRepo(IUserAdminSettings settings, IAdminLogger logger, ICacheProvider cacheProvider) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        }

        private string GetCacheKey(string orgid)
        {
            return $"ORG_USERS_{orgid}";
        }

        public async Task<bool> QueryOrgHasUserAsync(string orgId, string userId)
        {
            return (await base.GetAsync(orgId, OrgUser.CreateRowKey(orgId, userId), false)) != null;
        }

        public async Task AddOrgUserAsync(OrgUser user)
        {
            await InsertAsync(user);
            await _cacheProvider.RemoveAsync(GetCacheKey(user.OrgId));
        }

        public Task<IEnumerable<OrgUser>> GetOrgsForUserAsync(string userId)
        {
            return GetByFilterAsync(FilterOptions.Create(nameof(OrgUser.UserId), FilterOptions.Operators.Equals, userId));
        }



        public async Task<IEnumerable<OrgUser>> GetUsersForOrgAsync(string orgId)
        {
            var json = await _cacheProvider.GetAsync(GetCacheKey(orgId));
            if(String.IsNullOrEmpty(json))
            {
                var users = await GetByParitionIdAsync(orgId);
                await _cacheProvider.AddAsync(GetCacheKey(orgId), JsonConvert.SerializeObject(users));
                return users;
            }
            else
            {
                return JsonConvert.DeserializeObject<IEnumerable<OrgUser>>(json);
            }
        }

        public async Task RemoveUserFromOrgAsync(string orgid, string userid, EntityHeader removedBy)
        {
            var rowKey = OrgUser.CreateRowKey(orgid, userid);
            await RemoveAsync(orgid, rowKey);
            await _cacheProvider.RemoveAsync(GetCacheKey(orgid));
    }

        public async Task<bool> QueryOrgHasUserByEmailAsync(string orgId, string email)
        {
            return (await GetByFilterAsync(
                FilterOptions.Create(nameof(OrgUser.Email), FilterOptions.Operators.Equals, email.ToUpper()),
                FilterOptions.Create(nameof(OrgUser.PartitionKey), FilterOptions.Operators.Equals, orgId)
                )).ToList().Any();
        }

        public async Task<bool> IsUserOrgAdminAsync(string orgId, string userId)
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

        public async Task UpdateOrgUserAsync(OrgUser orgUser)
        {
            await this.UpdateAsync(orgUser);
            await _cacheProvider.RemoveAsync(GetCacheKey(orgUser.OrgId));
        }

        public async Task<bool> IsAppBuilderAsync(string orgId, string userId)
        {
            var orgUser = await GetOrgUserAsync(orgId, userId);
            return orgUser.IsAppBuilder;
        }

        public async Task ClearOrgCacheAsync(string orgId)
        {
            await _cacheProvider.RemoveAsync(GetCacheKey(orgId));
        }
    }
}
