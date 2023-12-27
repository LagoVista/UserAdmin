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
