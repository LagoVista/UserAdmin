// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4c2f5b43b0abda544983dc21e07223d6458e284cfbba32765ba9417c1a0947e5
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Account
{
    public interface IUserFavoritesRepo
    {
        Task AddUserFavoritesAsync(UserFavorites userFavorites);
        Task UpdateUserFavoritesAsync(UserFavorites userFavorites);

        Task<UserFavorites> GetUserFavoritesAsync(string userId, string orgId);
    }
}
