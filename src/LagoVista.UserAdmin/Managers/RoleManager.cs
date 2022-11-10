using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    internal class RoleManager : ManagerBase, IUserRoleManager
    {
        private readonly IRoleRepo _roleRepo;

        public RoleManager(IRoleRepo roleRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) : base(logger, appConfig, dependencyManager, security)
        {
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
        }

        public async Task<InvokeResult> AddRoleAsync(Role role, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(role, Actions.Create);
            await AuthorizeAsync(role, AuthorizeResult.AuthorizeActions.Create, user, org);

            await _roleRepo.InsertAsync(role);

            return InvokeResult.Success;
        }

        public async Task<Role> GetRoleAsync(string id, EntityHeader org, EntityHeader user)
        {
            var role = await _roleRepo.GetRoleAsync(id);

            await AuthorizeAsync(role, AuthorizeResult.AuthorizeActions.Create, user, org);

            return role;
        }

        public async Task<List<RoleSummary>> GetRolesAsync(EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(Role), Actions.Read); 
            return await _roleRepo.GetRolesAsync(org.Id);
        }

        public async Task<InvokeResult> UpdateRoleAsync(Role role, EntityHeader org, EntityHeader user)
        {
            ValidationCheck(role, Actions.Create);
            await AuthorizeAsync(role, AuthorizeResult.AuthorizeActions.Update, user, org);
            
            await _roleRepo.UpdateAsync(role);

            return InvokeResult.Success;
        }


    }
}
