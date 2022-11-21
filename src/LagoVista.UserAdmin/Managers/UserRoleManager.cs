using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Linq;
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
        private readonly IDefaultRoleList _defaultRoleList;
        

        public UserRoleManager(IRoleRepo roleRepo, IUserRoleRepo userRoleRepo, IDefaultRoleList defaultRole, IAppUserRepo appUserRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) :
            base(logger, appConfig, dependencyManager, security)
        {
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _userRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _defaultRoleList = defaultRole ?? throw new ArgumentNullException(nameof(defaultRole));
        }

        public Task<List<UserRole>> GetRolesForUserAsync(string userId, EntityHeader org, EntityHeader user)
        {
            return _userRoleRepo.GetRolesForUseAsync(userId, org.Id);
        }

        public async Task<InvokeResult<UserRole>> GrantUserRoleAsync(string userId, string roleId, EntityHeader org, EntityHeader user)
        {
            var roleUser = await _userRepo.FindByIdAsync(userId);
            var role = _defaultRoleList.GetStandardRoles().FirstOrDefault(rl=>rl.Id == roleId);
            if(role == null)
                role = await _roleRepo.GetRoleAsync(roleId);
            
            var appUserRole = new UserRole()
            {
                CreatedBy = user,
                Organization = org,
                Id = DateTime.UtcNow.ToInverseTicksRowKey(),
                User = roleUser.ToEntityHeader(),
                CreationDate = DateTime.UtcNow.ToJSONString(),
                Role = role.ToEntityHeader(),
            };

            await _userRoleRepo.AddUserRole(appUserRole);

            return InvokeResult<UserRole>.Create(appUserRole);
        }

        public async Task<List<InvokeResult<UserRole>>> GrantUserRolesAsync(string userId, List<string> roles, EntityHeader org, EntityHeader user)
        {
            if (roles == null)
                throw new ArgumentNullException(nameof(roles));

            var existingRoles = await GetRolesForUserAsync(userId, org, user);

            var addedRoles = new List<InvokeResult<UserRole>>();

            foreach(var role in roles)
            {
                var existingRole = existingRoles.FirstOrDefault(ext => ext.Role.Id == role);

                if (existingRole == null)
                {
                    addedRoles.Add( await GrantUserRoleAsync(userId, role, org, user));
                }
                else
                {
                    addedRoles.Add(InvokeResult<UserRole>.FromError($"Role {existingRole.Role.Text} already granted."));
                }
            }

            return addedRoles;
        }

        public async Task<InvokeResult> RevokeUserRoleAsync(string userRoleId, EntityHeader org, EntityHeader user)
        {
            await _userRoleRepo.RemoveUserRole(userRoleId, org.Id);
            return InvokeResult.Success;
        }
    }
}
