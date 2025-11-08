// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c5811eb87b9cc459ba16540635d026df036c6a6b3fd9d50217c28f45abcead79
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IUserFavoritesManager
    {
        Task<UserFavorites> GetUserFavoritesAsync(EntityHeader user, EntityHeader org);
        Task<UserFavorites> AddUserFavoriteAsync(EntityHeader user, EntityHeader org, UserFavorite favorite);
        Task<UserFavorites> UpdateUserFavoritesAsync(UserFavorites userFavorites, EntityHeader user, EntityHeader org);
        Task<UserFavorites> RemoveUserFavoriteAsync(EntityHeader user, EntityHeader org, string id);
    }
}
