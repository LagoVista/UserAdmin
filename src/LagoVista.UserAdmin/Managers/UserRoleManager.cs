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
using LagoVista.UserAdmin.Interfaces.Managers;

namespace LagoVista.UserAdmin.Managers
{
    public class UserRoleManager : ManagerBase, IUserRoleManager
    {
        private readonly IUserRoleRepo _userRoleRepo;
        private readonly IRoleRepo _roleRepo;
        private readonly IAppUserRepo _userRepo;
        private readonly IDefaultRoleList _defaultRoleList;
        private readonly IAuthenticationLogManager _authLogMgr;

        public UserRoleManager(IRoleRepo roleRepo, IUserRoleRepo userRoleRepo, IDefaultRoleList defaultRole, IAppUserRepo appUserRepo, ILogger logger, IAppConfig appConfig, IAuthenticationLogManager authLogMgr, IDependencyManager dependencyManager, ISecurity security) :
            base(logger, appConfig, dependencyManager, security)
        {
            _userRoleRepo = userRoleRepo ?? throw new ArgumentNullException(nameof(userRoleRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo));
            _userRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _defaultRoleList = defaultRole ?? throw new ArgumentNullException(nameof(defaultRole));
            _authLogMgr = authLogMgr ?? throw new ArgumentNullException(nameof(authLogMgr));
        }

        public Task<List<UserRole>> GetRolesForUserAsync(string userId, EntityHeader org, EntityHeader user)
        {
            return _userRoleRepo.GetRolesForUserAsync(userId, org.Id);
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

            await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.GrantRole, userId, roleUser.Name, org.Id, org.Text, extras: $"granted role: {role.Name} by user id: {user.Id} name: {user.Text}");

            return InvokeResult<UserRole>.Create(appUserRole);
        }

        public async Task<List<InvokeResult<UserRole>>> GrantUserRolesAsync(string userId, List<string> roleIds, EntityHeader org, EntityHeader user)
        {
            if (roleIds == null)
                throw new ArgumentNullException(nameof(roleIds));

            var existingRoles = await GetRolesForUserAsync(userId, org, user);

            var addedRoles = new List<InvokeResult<UserRole>>();

            var roleUser = await _userRepo.FindByIdAsync(userId);

            foreach(var roleId in roleIds)
            {
                var existingRole = existingRoles.FirstOrDefault(ext => ext.Role.Id == roleId);

                if (existingRole == null)
                {
                    var role = _defaultRoleList.GetStandardRoles().FirstOrDefault(rl => rl.Id == roleId);
                    if (role == null)
                        role = await _roleRepo.GetRoleAsync(roleId);

                    addedRoles.Add( await GrantUserRoleAsync(userId, roleId, org, user));
                    await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.GrantRole, userId, roleUser.Name, org.Id, org.Text, extras: $"granted role: {role.Name} by user id: {user.Id} name: {user.Text}");
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
            var role = await _userRoleRepo.GetRoleAssignmentAsync(userRoleId, org.Id);

            await _userRoleRepo.RemoveUserRole(userRoleId, org.Id);

            await _authLogMgr.AddAsync(Models.Security.AuthLogTypes.RevokeRole, role.User.Id, role.User.Text, role.Organization.Id, role.Organization.Text, extras: $"removed role: {role.Role.Text} by user id: {user.Id} name: {user.Text}");

            return InvokeResult.Success;
        }

        public async Task<bool> UserHasRoleAsync(string roleId, string userId, string orgId)
        {
            var roles = await _userRoleRepo.GetRolesForUserAsync(userId, orgId);
            return roles.Any(rl=>rl.Role.Id == roleId);
        }
    }
}
