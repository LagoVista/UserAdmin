using LagoVista.CloudStorage;
using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Security;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class RoleAccessRepo : TableStorageBase<RoleAccessDTO>, IRoleAccessRepo
    {
        private readonly ICacheProvider _cacheProvider;


        public RoleAccessRepo(IUserAdminSettings settings, IAdminLogger logger, ICacheProvider cacheProvider) :
               base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));

        }

        public string CacheKey(string roleId, string organizationId)
        {
            return $"{nameof(RoleAccess)}__{roleId}__{organizationId}";
        }

        public async Task AddRoleAccess(RoleAccess roleAccess)
        {
            await _cacheProvider.RemoveAsync(CacheKey(roleAccess.Role.Id, roleAccess.Organization.Id));
            await InsertAsync(roleAccess.ToDTO());
        }

        public async Task<List<RoleAccess>> GetRoleAccessForRoleAsync(string roleId, string organizationId)
        {
            var json = await _cacheProvider.GetAsync(CacheKey(roleId, organizationId));
            if (String.IsNullOrEmpty(json))
            {
                var results = await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.RoleId), FilterOptions.Operators.Equals, roleId), FilterOptions.Create(nameof(UserRoleDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId));
                var roleAcess = results.Select(usr => usr.ToRoleAccess()).OrderBy(usr => usr.Role.Text).ToList();
                json = JsonConvert.SerializeObject(roleAcess);
                await _cacheProvider.AddAsync(CacheKey(roleId, organizationId), json);
                return roleAcess;
            }
            else
            {
                return JsonConvert.DeserializeObject<List<RoleAccess>>(json);
            }
        }

        public async Task RemoveRoleAccess(string roleAccessId, string organizationId)
        {
            var roleAccess = await GetAsync(roleAccessId, organizationId);
            await _cacheProvider.RemoveAsync(CacheKey(roleAccess.RoleId, roleAccess.OrganizationId));
            await RemoveAsync(organizationId, roleAccessId);
        }
    }
}
