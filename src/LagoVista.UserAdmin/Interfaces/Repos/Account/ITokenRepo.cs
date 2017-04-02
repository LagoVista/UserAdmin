using LagoVista.Core.Authentication.Models;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Account
{
    public interface ITokenRepo
    {
        Task<RefreshToken> GetRefreshTokenAsync(string tokenId, string userId);
        Task RemoveRefreshTokenAsync(string userId, string tokenId);
        Task SaveRefreshTokenAsync(RefreshToken token);
    }
}