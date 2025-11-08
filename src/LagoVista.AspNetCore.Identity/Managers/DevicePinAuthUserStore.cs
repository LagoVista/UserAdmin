// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 032ceab06d473874ec70b6943327bd40729a5918979486789c2ec9436c5bdee7
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Identity;
using RingCentral;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    public class DeviceOwnerUserStore : IUserStore<DeviceOwnerUser>
    {
        IDeviceOwnerRepo _ownerRepo;

        public DeviceOwnerUserStore(IDeviceOwnerRepo ownerRepo)
        {
            _ownerRepo = ownerRepo;
        }

        public async Task<IdentityResult> CreateAsync(DeviceOwnerUser user, CancellationToken cancellationToken)
        {
            await _ownerRepo.AddUserAsync(user);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(DeviceOwnerUser user, CancellationToken cancellationToken)
        {
            await _ownerRepo.DeleteUserAsync(user.OwnerOrganization.Id, user.Id);

            return IdentityResult.Success;
        }

        public void Dispose()
        {
        }

        public Task<DeviceOwnerUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return _ownerRepo.FindByIdAsync(userId);;
        }

        public Task<DeviceOwnerUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return _ownerRepo.FindByIdAsync(normalizedUserName); ;
        }

        public Task<string> GetNormalizedUserNameAsync(DeviceOwnerUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailAddress.ToUpper());
        }

        public Task<string> GetUserIdAsync(DeviceOwnerUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id);
        }

        public Task<string> GetUserNameAsync(DeviceOwnerUser user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailAddress);
        }

        public Task SetNormalizedUserNameAsync(DeviceOwnerUser user, string normalizedName, CancellationToken cancellationToken)
        {
            user.UserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(DeviceOwnerUser user, string userName, CancellationToken cancellationToken)
        {
            user.EmailAddress = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(DeviceOwnerUser user, CancellationToken cancellationToken)
        {
            await _ownerRepo.UpdateUserAsync(user);
            return IdentityResult.Success;
        }
    }
}
