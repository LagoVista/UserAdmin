// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b0c0eb78b50b0eb5f4130fbe948452338405f752218cebf74376bc92dc09916d
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using System.Diagnostics;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class OrgUserRepo : TableStorageBase<OrgUser>, IOrgUserRepo
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly IAdminLogger _adminLogger;

        public OrgUserRepo(IUserAdminSettings settings, IAdminLogger logger, ICacheProvider cacheProvider) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                var users = await GetByPartitionIdAsync(orgId);
                await _cacheProvider.AddAsync(GetCacheKey(orgId), JsonConvert.SerializeObject(users));
                return users;
            }
            else
            {
                return JsonConvert.DeserializeObject<IEnumerable<OrgUser>>(json);
            }
        }

        public async Task RemoveUserFromOrgAsync(string orgId, string userId, EntityHeader removedBy)
        {
            var rowKey = OrgUser.CreateRowKey(orgId, userId);
            await RemoveAsync(orgId, rowKey);
            await _cacheProvider.RemoveAsync(GetCacheKey(orgId));
            await _cacheProvider.RemoveAsync(GetOrgUserKey(orgId, userId));
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
            if(string.IsNullOrEmpty(orgId)) throw new ArgumentNullException(nameof(orgId));
            if(string.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            var orgUser = await GetOrgUserAsync(orgId, userId);
            if(orgUser == null)
                throw new ArgumentNullException(nameof(orgUser), $"OrgUser not found for OrgId: {orgId}, UserId: {userId}");

            return orgUser.IsOrgAdmin;
        }

        private string GetOrgUserKey(string orgId, string userId)
        {
            return $"org_user_{orgId}_{userId}";
        }

        public async Task<OrgUser> GetOrgUserAsync(string orgId, string userId)
        {
            var sw = Stopwatch.StartNew();
            var json = await _cacheProvider.GetAsync(GetOrgUserKey(orgId, userId));
            if(!String.IsNullOrEmpty(json))
            {
                _adminLogger.AddCustomEvent(LogLevel.Message, "[OrgUserRepo__GetOrguserAsync]", $"[OrgUserRepo__GetOrguserAsync] - Cache Hit OrgId: {orgId}, UserId: {userId} - {sw.Elapsed.TotalMilliseconds}.");
                return JsonConvert.DeserializeObject<OrgUser>(json);
            }
            _adminLogger.AddCustomEvent(LogLevel.Message, "[OrgUserRepo__GetOrguserAsync]", $"[OrgUserRepo__GetOrguserAsync] - Cache Miss OrgId: {orgId}, UserId: {userId} - {sw.Elapsed.TotalMilliseconds}.");
            sw.Restart();

            var orgUser = (await GetByFilterAsync(
                FilterOptions.Create(nameof(OrgUser.UserId), FilterOptions.Operators.Equals, userId.ToUpper()),
                FilterOptions.Create(nameof(OrgUser.PartitionKey), FilterOptions.Operators.Equals, orgId)
                )).FirstOrDefault();

            await _cacheProvider.AddAsync(GetOrgUserKey(orgId, userId), JsonConvert.SerializeObject(orgUser));

            _adminLogger.AddCustomEvent(LogLevel.Message, "[OrgUserRepo__GetOrguserAsync]", $"[OrgUserRepo__GetOrguserAsync] - Added To Cache OrgId: {orgId}, UserId: {userId} - {sw.Elapsed.TotalMilliseconds}.");

            return orgUser;
        }

        public async Task UpdateOrgUserAsync(OrgUser orgUser)
        {
            await this.UpdateAsync(orgUser);

            await _cacheProvider.RemoveAsync(GetOrgUserKey(orgUser.OrgId, orgUser.UserId));
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
