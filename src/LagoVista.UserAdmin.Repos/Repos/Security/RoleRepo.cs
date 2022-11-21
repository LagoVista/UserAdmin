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

namespace LagoVista.UserAdmin.Repos.Security
{
    public class RoleRepo : DocumentDBRepoBase<Role>, IRoleRepo
    {

        private readonly IDefaultRoleList _defaultRoleList;
        private readonly IUserSecurityServices _securityService;

        private bool _consolidateCollectoins;

        public RoleRepo(IUserAdminSettings settings, IUserSecurityServices securityService, IDefaultRoleList defaultRoleList, IAdminLogger logger) : 
            base(settings.UserStorage.Uri, settings.UserStorage.AccessKey, settings.UserStorage.ResourceName, logger)
        {
            _consolidateCollectoins = settings.ShouldConsolidateCollections;
            _defaultRoleList = defaultRoleList;
            _securityService = securityService;
        }

        protected override bool ShouldConsolidateCollections => _consolidateCollectoins;

        public Task AddRoleAsync(Role role)
        {
            return CreateDocumentAsync(role);
        }

        public Task<Role> GetRoleAsync(string id)
        {
            return GetDocumentAsync(id);
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

        public Task UpdateAsync(Role role)
        {
            return base.UpsertDocumentAsync(role);
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
    }
}
