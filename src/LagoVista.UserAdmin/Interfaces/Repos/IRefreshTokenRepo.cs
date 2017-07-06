using LagoVista.Core.Authentication.Models;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos
{
    public interface IRefreshTokenRepo
    {
        Task<RefreshToken> GetRefreshTokenAsync(string tokenId, string userId);
        Task SaveRefreshTokenAsync(RefreshToken token);
        Task RemoveRefreshTokenAsync(string tokenId, string userId);
        Task RemoveAllForUserAsync(string userId);
    }
}