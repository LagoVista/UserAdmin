// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c420b987d2d6432cc4cf33feb1eb043f3f70d41a22d10ce67a08e81da6b4610a
// IndexVersion: 0
// --- END CODE INDEX META ---
using System.Threading.Tasks;
using LagoVista.Core.PlatformSupport;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.IoT.Logging.Loggers;
using System;

namespace LagoVista.UserAdmin.Repos.Users
{
    public class RefreshTokenRepo : TableStorageBase<RefreshToken>, IRefreshTokenRepo
    {
        public RefreshTokenRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }
        
        public Task<RefreshToken> GetRefreshTokenAsync(string tokenId, string userId)
        {
            return GetAsync(userId, tokenId, false);
        }

        public Task RemoveAllForUserAsync(string userId)
        {            
            return RemoveByPartitionKeyAsync(userId);
        }

        public Task RemoveRefreshTokenAsync(string tokenId, string userId)
        {
            return RemoveAsync(userId, tokenId);
        }

        public Task SaveRefreshTokenAsync(RefreshToken token)
        {
            return InsertAsync(token);
        }
    }
}
