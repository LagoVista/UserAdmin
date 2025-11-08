// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 16ac8e253c6f6364da7922c7d38994dfe88741fe7d328ba6856d0a8234fa658d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class UserRoleRepo : TableStorageBase<UserRoleDTO>, IUserRoleRepo
    {
        private readonly ICacheProvider _cacheProvider;
        private readonly IAdminLogger _adminLogger;

        public UserRoleRepo(IUserAdminSettings settings, ICacheProvider cacheProvider, IAdminLogger logger) : 
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
         }

        public async Task AddUserRole(UserRole role)
        {
            await _cacheProvider.RemoveAsync(RolesCacheKey(role.User.Id, role.Organization.Id));
            await InsertAsync(role.ToDTO());
        }

        public async Task<UserRole> GetRoleAssignmentAsync(string userRoleId, string organizationId)
        {
            return (await GetAsync(organizationId, userRoleId)).ToUserRole();
        }

        private string RolesCacheKey(string userid, string orgId)
        {
            return $"user_role_cache_{userid}_{orgId}";
        }

        public async Task<List<UserRole>> GetRolesForUserAsync(string userId, string organizationId)
        {
            var sw = Stopwatch.StartNew(); 
            var json = await _cacheProvider.GetAsync(RolesCacheKey(userId, organizationId));
            if(!String.IsNullOrEmpty(json))
            {
                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[UserRoleRepo__GetRolesForuser]", $"[UserRoleRepo__GetRolesForuser] - cache hit userid {userId} orgid {organizationId} - {sw.Elapsed.TotalMilliseconds}ms");
                return JsonConvert.DeserializeObject<List<UserRole>>(json);
            }
            sw.Restart();

            var results = await this.GetByFilterAsync(FilterOptions.Create(nameof(UserRoleDTO.UserId), FilterOptions.Operators.Equals, userId), 
                                                                                  FilterOptions.Create(nameof(UserRoleDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId));

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[UserRoleRepo__GetRolesForuser]", $"[UserRoleRepo__GetRolesForuser] - cache miss userid {userId} orgid {organizationId} loaded from storage in - {sw.Elapsed.TotalMilliseconds}ms");
            var roles = results.Select(usr => usr.ToUserRole()).OrderBy(usr => usr.Role.Text).ToList();
            sw.Restart();

            await _cacheProvider.AddAsync(RolesCacheKey(userId, organizationId), JsonConvert.SerializeObject(roles));
            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[UserRoleRepo__GetRolesForuser]", $"[UserRoleRepo__GetRolesForuser] - userid {userId} orgid {organizationId} added to cache in - {sw.Elapsed.TotalMilliseconds}ms");

            return roles;
        }

        public async Task RemoveUserRole(string userRoleId, string organizationId)
        {
            await _cacheProvider.RemoveAsync(RolesCacheKey(userRoleId, organizationId));
            await RemoveAsync(organizationId, userRoleId);
        }
    }
}
