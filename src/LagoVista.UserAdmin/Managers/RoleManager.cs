// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 98117072c8e213b4520f6510a16772181d14c6d268210d75ff8bfffe251b37b6
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    internal class RoleManager : ManagerBase, IRoleManager
    {
        private readonly IRoleRepo _roleRepo;
        private readonly IRoleAccessRepo _roleAccessRepo;

        public RoleManager(IRoleRepo roleRepo, IRoleAccessRepo roleAccessRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) : base(logger, appConfig, dependencyManager, security)
        {
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _roleAccessRepo = roleAccessRepo ?? throw new ArgumentNullException(nameof(roleAccessRepo));
        }

        public async Task<InvokeResult> AddRoleAccessAsync(RoleAccess access, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(RoleAccess), Actions.Create);
            await _roleAccessRepo.AddRoleAccess(access);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddRoleAsync(Role role, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(role, Actions.Create);
            await AuthorizeAsync(role, AuthorizeResult.AuthorizeActions.Create, user, org);

            await _roleRepo.InsertAsync(role);

            return InvokeResult.Success;
        }

        public Task<List<RoleAccess>> GetRoleAccessAsync(string roleId, EntityHeader org, EntityHeader user)
        {
            return _roleAccessRepo.GetRoleAccessForRoleAsync(roleId, org.Id);
        }

        public async Task<Role> GetRoleAsync(string id, EntityHeader org, EntityHeader user)
        {
            var role = await _roleRepo.GetRoleAsync(id);
            
            if(!role.IsSystemRole)
                await AuthorizeAsync(role, AuthorizeResult.AuthorizeActions.Create, user, org);

            return role;
        }

        public async Task<List<RoleSummary>> GetRolesAsync(EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Role), Actions.Read); 
            return await _roleRepo.GetRolesAsync(org.Id);
        }

        public async Task<List<RoleSummary>> GetAssignableRolesAsync(EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Role), Actions.Read);
            return await _roleRepo.GetAssignableRolesAsync(org.Id);
        }

        public async Task<InvokeResult> RevokeRoleAccessAsync(string accessId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(RoleAccess), Actions.Delete);
            await _roleAccessRepo.RemoveRoleAccess(accessId, org.Id);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateRoleAsync(Role role, EntityHeader org, EntityHeader user)
        {
            if (role.IsSystemRole)
                return InvokeResult.FromError("Can not update system role.");

            ValidationCheck(role, Actions.Create);
            await AuthorizeAsync(role, AuthorizeResult.AuthorizeActions.Update, user, org);
            
            await _roleRepo.UpdateAsync(role);

            return InvokeResult.Success;
        }

        public async Task<List<RoleAccess>> GetRoleAccessForModuleAsync(string moduleId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(RoleAccess), Actions.Read);
            return await _roleAccessRepo.GetRoleAccessForModuleAsync(moduleId, org.Id);
        }

        public async Task<List<RoleAccess>> GetRoleAccessForAreaAsync(string moduleId, string areaId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Role), Actions.Read);
            return await _roleAccessRepo.GetRoleAccessForAreaAsync(moduleId, areaId, org.Id);
        }

        public async Task<List<RoleAccess>> GetRoleAccessForPageAsync(string moduleId,  string areaId, string pageId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Role), Actions.Read);
            return await _roleAccessRepo.GetRoleAccessForPageAsync(moduleId, areaId, pageId, org.Id);
        }

        public async Task<List<RoleAccess>> GetRoleAccessForModuleFeatureAsync(string moduleId, string featureId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Role), Actions.Read);
            return await _roleAccessRepo.GetRoleAccessForModuleFeatureAsync(moduleId, featureId, org.Id);
        }

        public async Task<List<RoleAccess>> GetRoleAccessForAreaFeatureAsync(string moduleId, string areaId, string featureId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Role), Actions.Read);
            return await _roleAccessRepo.GetRoleAccessForAreaFeatureAsync(moduleId, areaId, featureId, org.Id);
        }

        public async Task<List<RoleAccess>> GetRoleAccessForPageFeatureAsync(string moduleId, string areaId, string pageId, string featureId, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Role), Actions.Read);
            return await _roleAccessRepo.GetRoleAccessForPageFeatureAsync(moduleId, areaId, pageId, featureId, org.Id);
        }
    }
}
