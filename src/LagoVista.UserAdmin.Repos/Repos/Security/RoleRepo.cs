using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.PlatformSupport;
using System.Threading.Tasks;
using System;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.IoT.Logging.Loggers;
using System.Collections.Generic;
using System.Linq;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.CloudStorage;
using Newtonsoft.Json;
using LagoVista.Core.Interfaces;

namespace LagoVista.UserAdmin.Repos.Security
{
    public class RoleRepo : DocumentDBRepoBase<Role>, IRoleRepo
    {

        private readonly IDefaultRoleList _defaultRoleList;
       
        private readonly ICacheProvider _cacheProvider;

        private bool _consolidateCollectoins;

        public RoleRepo(IUserAdminSettings settings, IDefaultRoleList defaultRoleList, IAdminLogger logger, ICacheProvider cacheProvider) : 
            base(settings.UserStorage.Uri, settings.UserStorage.AccessKey, settings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _consolidateCollectoins = settings.ShouldConsolidateCollections;
            _defaultRoleList = defaultRoleList;
            _cacheProvider = cacheProvider;
        }

        protected override bool ShouldConsolidateCollections => _consolidateCollectoins;

        public async Task<Role> GetRoleAsync(string id)
        {
            var role = _defaultRoleList.GetStandardRoles().FirstOrDefault(rol => rol.Id == id);
            if (role != null)
                return role;

            return await GetDocumentAsync(id);
        }

        public Task AddRoleAsync(Role role)
        {
            return CreateDocumentAsync(role);
        }

        public async Task<List<Role>> GetAllPublicRoles()
        {
            var roles = await QueryAsync(role => role.IsPublic);
            var allRRoles = roles.ToList();
            allRRoles.AddRange(_defaultRoleList.GetStandardRoles());

            return allRRoles.OrderBy(rol => rol.Name).ToList();
        }

       
        public Task InsertAsync(Role role)
        {
            return CreateDocumentAsync(role);
        }


        public Task RemoveAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public async Task UpdateAsync(Role role)
        {
            await _cacheProvider.RemoveAsync($"ROLEB_KEY_{role.Key}_{role.OwnerOrganization.Id}");
            await base.UpsertDocumentAsync(role);
        }

        public async Task<List<RoleSummary>> GetAssignableRolesAsync(string orgId)
        {
            var roles = await QueryAsync(role => role.OwnerOrganization.Id == orgId || role.IsPublic);
            var allRoles = roles.ToList();
            allRoles.AddRange(_defaultRoleList.GetStandardRoles());

            return allRoles.Select(rol => rol.CreateSummary()).OrderBy(rol => rol.Name).ToList();
        }

        public async Task<List<RoleSummary>> GetRolesAsync(string orgId)
        {
            var roles = await QueryAsync(role => role.OwnerOrganization.Id == orgId);
            return roles.Select(rol => rol.CreateSummary()).OrderBy(rol => rol.Name).ToList();
        }

        public async Task<Role> GetRoleByKeyAsync(string key, string orgId)
        {
            var role = _defaultRoleList.GetStandardRoles().SingleOrDefault(rol => rol.Key == key);
            if (role == null)
            {
                var json = await _cacheProvider.GetAsync($"ROLEB_KEY_{key}_{orgId}");
                if (!String.IsNullOrEmpty(json))
                {
                    role = JsonConvert.DeserializeObject<Role>(json);
                }
                else
                {
                    var roles = await QueryAsync(role => role.OwnerOrganization.Id == orgId && role.Key == key);
                    if(!roles.Any())
                    {
                        throw new ArgumentOutOfRangeException($"Could not find role {key} in organization {orgId}");
                    }
                    
                    role = roles.First();

                    await _cacheProvider.AddAsync($"ROLEB_KEY_{key}_{orgId}", JsonConvert.SerializeObject(role));
                }
            }

            return role;
        }
    }
}
