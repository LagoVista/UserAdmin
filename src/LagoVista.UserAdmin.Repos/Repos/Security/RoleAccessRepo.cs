using LagoVista.CloudStorage;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
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


        public const string IS_PUBLIC = "public_role_access";

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
            await _cacheProvider.RemoveAsync(CacheKey(roleAccess.Role.Id, roleAccess.IsPublic ? IS_PUBLIC : roleAccess.Organization.Id));
            await InsertAsync(roleAccess.ToDTO());
        }

        public async Task<List<RoleAccess>> GetRoleAccessForModuleAsync(string moduleId, string organizationId)
        {
            var results = (await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId), 
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, false))).ToList();

            results.AddRange(await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, true)));

            return results.Select(usr => usr.ToRoleAccess()).OrderBy(usr => usr.Role.Text).ToList();
        }

        public async Task<List<RoleAccess>> GetRoleAccessForAreaAsync(string moduleId, string areaId, string organizationId)
        {
            var results = (await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.AreaId), FilterOptions.Operators.Equals, areaId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, false))).ToList();

            results.AddRange(await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.AreaId), FilterOptions.Operators.Equals, areaId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, true)));

            return results.Select(usr => usr.ToRoleAccess()).OrderBy(usr => usr.Role.Text).ToList();
        }

        public async Task<List<RoleAccess>> GetRoleAccessForPageAsync(string moduleId, string areaId, string pageId, string organizationId)
        {
            var results = (await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.AreaId), FilterOptions.Operators.Equals, areaId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PageId), FilterOptions.Operators.Equals, pageId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, false))).ToList();

            results.AddRange(await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.AreaId), FilterOptions.Operators.Equals, areaId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PageId), FilterOptions.Operators.Equals, pageId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, true)));

            return results.Select(usr => usr.ToRoleAccess()).OrderBy(usr => usr.Role.Text).ToList();
        }

        public async Task<List<RoleAccess>> GetRoleAccessForModuleFeatureAsync(string moduleId, string featureId, string organizationId)
        {
            var results = (await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.FeatureId), FilterOptions.Operators.Equals, featureId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, false))).ToList();

            results.AddRange(await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.FeatureId), FilterOptions.Operators.Equals, featureId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, true)));

            return results.Select(usr => usr.ToRoleAccess()).OrderBy(usr => usr.Role.Text).ToList();
        }

        public async Task<List<RoleAccess>> GetRoleAccessForAreaFeatureAsync(string moduleId, string areaId, string featureId, string organizationId)
        {
            var results = (await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.AreaId), FilterOptions.Operators.Equals, areaId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.FeatureId), FilterOptions.Operators.Equals, featureId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, false))).ToList();

            results.AddRange(await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.AreaId), FilterOptions.Operators.Equals, areaId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.FeatureId), FilterOptions.Operators.Equals, featureId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, true)));
            return results.Select(usr => usr.ToRoleAccess()).OrderBy(usr => usr.Role.Text).ToList();
        }

        public async Task<List<RoleAccess>> GetRoleAccessForPageFeatureAsync(string moduleId, string areaId, string pageId, string featureId, string organizationId)
        {
            var results = (await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.AreaId), FilterOptions.Operators.Equals, areaId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PageId), FilterOptions.Operators.Equals, pageId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.FeatureId), FilterOptions.Operators.Equals, featureId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, false))).ToList();

            results.AddRange(await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.ModuleId), FilterOptions.Operators.Equals, moduleId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.AreaId), FilterOptions.Operators.Equals, areaId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.PageId), FilterOptions.Operators.Equals, pageId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.FeatureId), FilterOptions.Operators.Equals, featureId),
                                                      FilterOptions.Create(nameof(RoleAccessDTO.IsPublic), FilterOptions.Operators.Equals, true)));

            return results.Select(usr => usr.ToRoleAccess()).OrderBy(usr => usr.Role.Text).ToList();
        }


        public async Task<List<RoleAccess>> GetRoleAccessForRoleAsync(string roleId, string organizationId)
        {
            var roleAccess = new List<RoleAccess>();

            var json = await _cacheProvider.GetAsync(CacheKey(roleId, organizationId));
            if (String.IsNullOrEmpty(json))
            {
                var results = await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.RoleId), FilterOptions.Operators.Equals, roleId),
                                                          FilterOptions.Create(nameof(UserRoleDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId));
                var orgRoleAcess = results.Select(usr => usr.ToRoleAccess()).OrderBy(usr => usr.Role.Text).ToList();
                json = JsonConvert.SerializeObject(orgRoleAcess);
                await _cacheProvider.AddAsync(CacheKey(roleId, organizationId), json);
                roleAccess.AddRange(orgRoleAcess);
            }
            else
            {
                roleAccess.AddRange(JsonConvert.DeserializeObject<List<RoleAccess>>(json));
            }

            json = await _cacheProvider.GetAsync(CacheKey(roleId, IS_PUBLIC));
            if (String.IsNullOrEmpty(json))
            {
                var results = await this.GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.RoleId), FilterOptions.Operators.Equals, roleId),
                                                          FilterOptions.Create(nameof(UserRoleDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId));
                var orgRoleAcess = results.Select(usr => usr.ToRoleAccess()).OrderBy(usr => usr.Role.Text).ToList();
                json = JsonConvert.SerializeObject(orgRoleAcess);
                await _cacheProvider.AddAsync(CacheKey(roleId, IS_PUBLIC), json);
                roleAccess.AddRange(orgRoleAcess);
            }
            else
            {
                roleAccess.AddRange(JsonConvert.DeserializeObject<List<RoleAccess>>(json));
            }


            return roleAccess;
        }

        public async Task RemoveRoleAccess(string roleAccessId, string organizationId)
        {
            var roleAccess = await GetAsync(organizationId, roleAccessId);
            await _cacheProvider.RemoveAsync(CacheKey(roleAccess.RoleId, roleAccess.IsPublic ? IS_PUBLIC : roleAccess.OrganizationId));
            await RemoveAsync(organizationId, roleAccessId);
        }
    }
}
