using System.Threading.Tasks;
using LagoVista.Core.PlatformSupport;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Interfaces.Repos;

namespace LagoVista.UserAdmin.Repos.Account
{
    public class TokenRepo : TableStorageBase<RefreshToken>, ITokenRepo
    {
        public TokenRepo(IAppUserManagementSettings settings, ILogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        private const string REFRESH_TOKENS = "RefreshTokens";
        
        public Task<RefreshToken> GetRefreshTokenAsync(string tokenId, string userId)
        {
            return GetAsync(userId, tokenId);
        }

        public Task RemoveRefreshTokenAsync(string userId, string tokenId)
        {
            return RemoveAsync(userId, tokenId);
        }

        public Task SaveRefreshTokenAsync(RefreshToken token)
        {
            return InsertAsync(token);
        }
    }
}
