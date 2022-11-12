using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class UserRoleManager : ManagerBase, IUserRoleManager
    {
        private readonly IUserRoleRepo _userRoleRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly IAppUserRepo _userRepo;

        public UserRoleManager(IRoleRepo roleRepo, IUserRoleRepo userRoleRepo, IAppUserRepo appUserRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) :
            base(logger, appConfig, dependencyManager, security)
        {
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _userRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
        }

        public Task<List<UserRole>> GetRolesForUserAsync(string userId, EntityHeader org, EntityHeader user)
        {
            return _userRoleRepo.GetRolesForUseAsyncr(userId, org.Id);
        }

        public async Task<InvokeResult<UserRole>> GrantUserRoleAsync(string userId, string roleId, EntityHeader org, EntityHeader user)
        {
            var roleUser = await _userRepo.FindByIdAsync(userId);
            var role = await _roleRepo.GetRoleAsync(roleId);
            var appUserRole = new UserRole()
            {
                CreatedBy = user,
                Organization = org,
                Id = DateTime.UtcNow.ToInverseTicksRowKey(),
                User = roleUser.ToEntityHeader(),
                CreeatedOn = DateTime.UtcNow.ToJSONString(),
                Role = role.ToEntityHeader(),
            };

            await _userRoleRepo.AddUserRole(appUserRole);

            return InvokeResult<UserRole>.Create(appUserRole);
        }

        public async Task<InvokeResult> RevokeUserRoleAsync(string userRoleId, EntityHeader org, EntityHeader user)
        {
            await _userRoleRepo.RemoveUserRole(userRoleId, org.Id);
            return InvokeResult.Success;
        }
    }
}
