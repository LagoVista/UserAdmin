using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class AspNetRoleStore : IRoleStore<Role>
    {
        IRoleRepo _roleRepo;

        public AspNetRoleStore(IRoleRepo roleRepo)
        {
            _roleRepo = roleRepo;
        }

        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken token)
        {
            await _roleRepo.InsertAsync(role);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken token)
        {
            await _roleRepo.RemoveAsync(role.Id);
            return IdentityResult.Success;
        }

        public async Task<Role> FindByIdAsync(string roleId, CancellationToken token)
        {
            return await _roleRepo.GetRoleAsync(roleId);
            
        }

        public async Task<Role> FindByNameAsync(string roleName, CancellationToken token)
        {
            return await _roleRepo.GetRoleAsync(roleName);
        }

        public async Task<IdentityResult> UpdateAsync(Role role, CancellationToken token)
        {
            await _roleRepo.UpdateAsync(role);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            _roleRepo.Dispose();
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.FromResult(0);
        }

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            role.Name = normalizedName;
            return Task.FromResult(0);
        }
    }
}
