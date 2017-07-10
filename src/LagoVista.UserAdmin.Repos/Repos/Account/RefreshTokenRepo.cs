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

        public async Task RemoveAllForUserAsync(string userId)
        {
            var tokens = await GetByParitionIdAsync(userId);
            foreach(var token in tokens)
            {
                await RemoveAsync(token);
            }
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
