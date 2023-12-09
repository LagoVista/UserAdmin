using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IUserFavoritesManager
    {
        Task<UserFavorites> GetUserFavoritesAsync(EntityHeader user, EntityHeader org);
        Task<UserFavorites> AddUserFavoriteAsync(EntityHeader user, EntityHeader org, UserFavorite favorite);
        Task<UserFavorites> RemoveUserFavoriteAsync(EntityHeader user, EntityHeader org, string id);
    }
}
