// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e9bb9abd260e54bdeb81629faaba8874e3d152728cfc314722f26b92d1fe0a16
// IndexVersion: 0
// --- END CODE INDEX META ---
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