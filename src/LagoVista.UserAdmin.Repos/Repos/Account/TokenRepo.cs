using System.Threading.Tasks;
using LagoVista.Core.PlatformSupport;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Users
{
    public class TokenRepo : TableStorageBase<RefreshToken>, ITokenRepo
    {
        public TokenRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        
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
