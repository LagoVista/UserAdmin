using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class UserFavoritesManager : ManagerBase, IUserFavoritesManager
    {
        private readonly IUserFavoritesRepo _userFavoritesRepo;
        public UserFavoritesManager(IUserFavoritesRepo userFavoritesRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) : base(logger, appConfig, dependencyManager, security)
        {
            _userFavoritesRepo = userFavoritesRepo ?? throw new ArgumentNullException(nameof(userFavoritesRepo));
        }

        public async Task<UserFavorites> AddUserFavoriteAsync(EntityHeader user, EntityHeader org, UserFavorite favorite)
        {
            var userFavorites = await GetUserFavoritesAsync(user, org);

            var existingFavorite = userFavorites.Favorites.SingleOrDefault(userFavorites => userFavorites.Id == favorite.Id);
            if (existingFavorite != null)
                userFavorites.Favorites.Remove(existingFavorite);

            userFavorites.Favorites.Add(favorite);
            userFavorites.LastUpdatedBy = user;
            userFavorites.LastUpdatedDate = DateTime.UtcNow.ToJSONString();
            await _userFavoritesRepo.UpdateUserFavoritesAsync(userFavorites);
            return userFavorites;
        }

        public async Task<UserFavorites> GetUserFavoritesAsync(EntityHeader user, EntityHeader org)
        {
            var result = await _userFavoritesRepo.GetUserFavoritesAsync(user.Id, org.Id);
            if (result != null)
                return result;
            
            var timeStamp = DateTime.UtcNow.ToJSONString();
            var userFavorites = new UserFavorites()
            {
                Id = Guid.NewGuid().ToId(),
                CreatedBy = user,
                CreationDate = timeStamp,
                LastUpdatedBy = user,
                LastUpdatedDate = timeStamp,
                OwnerOrganization = org,
                OwnerUser = user,
                Name = $"{user.Text}/{org.Text} - Favorites"
            };

            await _userFavoritesRepo.AddUserFavoritesAsync(userFavorites);

            return userFavorites;
        }

        public async Task<UserFavorites> RemoveUserFavoriteAsync(EntityHeader user, EntityHeader org, string id)
        {
            var userFavorites = await GetUserFavoritesAsync(user, org);
            var userFavorite = userFavorites.Favorites.SingleOrDefault(userFavorites => userFavorites.Id == id);
            if (userFavorite != null)
            {
                userFavorites.Favorites.Remove(userFavorite);
                await _userFavoritesRepo.UpdateUserFavoritesAsync(userFavorites);
            }

            return userFavorites;
        }
    }
}
