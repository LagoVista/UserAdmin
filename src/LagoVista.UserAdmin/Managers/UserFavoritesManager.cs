// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 06effde8f82ec694a2d4c533ac9678fce77f3d7aa412645b58afdba839113006
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using Newtonsoft.Json;
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
            favorite.DateAdded = DateTime.UtcNow.ToJSONString();

            var existingFavorite = userFavorites.Favorites.SingleOrDefault(userFavorites => userFavorites.Link == favorite.Link);
            if (existingFavorite != null)
                userFavorites.Favorites.Remove(existingFavorite);

            if (!String.IsNullOrEmpty(favorite.ModuleKey))
            {
                var byModule = userFavorites.Modules.SingleOrDefault(mod => mod.ModuleKey == favorite.ModuleKey);
                if (byModule == null)
                {
                    byModule = new FavoritesByModule()
                    {
                        ModuleKey = favorite.ModuleKey,
                    };
                    userFavorites.Modules.Add(byModule);
                }

                var existingByModule = byModule.Items.Where(mod => mod.Link == favorite.Link).SingleOrDefault();
                if (existingByModule != null)
                    byModule.Items.Remove(existingByModule);

                byModule.Items.Add(favorite);
            }

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
            {
                if (String.IsNullOrEmpty(result.Key))
                    result.Key = "userfavorite";

                return result;
            }

            var timeStamp = DateTime.UtcNow.ToJSONString();
            var userFavorites = new UserFavorites()
            {
                CreatedBy = user,
                CreationDate = timeStamp,
                LastUpdatedBy = user,
                LastUpdatedDate = timeStamp,
                OwnerOrganization = org,
                OwnerUser = user,
                IsPublic = false,
                Name = $"{user.Text}/{org.Text} - Favorites",
            };

            await _userFavoritesRepo.AddUserFavoritesAsync(userFavorites);

            return userFavorites;
        }

        public async Task<UserFavorites> RemoveUserFavoriteAsync(EntityHeader user, EntityHeader org, string id)
        {
            var userFavorites = await GetUserFavoritesAsync(user, org);
            if(userFavorites == null)
            {
                throw new RecordNotFoundException("UserFavorites",$"UID={user.Id} ORGID={org.Id}");
            }

            Console.WriteLine("1111");


            var userFavorite = userFavorites.Favorites.SingleOrDefault(userFavorites => userFavorites.Id == id);
            if (userFavorite != null)
            {
                userFavorites.Favorites.Remove(userFavorite);

                if (!String.IsNullOrEmpty(userFavorite.ModuleKey))
                {
                    var byModule = userFavorites.Modules.SingleOrDefault(mod => mod.ModuleKey == userFavorite.ModuleKey);
                    if (byModule != null)
                    {
                        var existing = byModule.Items.FirstOrDefault(item => item.Id == id);
                        if (existing != null)
                            byModule.Items.Remove(existing);
                    }
                }
                await _userFavoritesRepo.UpdateUserFavoritesAsync(userFavorites);
            }

            return userFavorites;
        }

        public async Task<UserFavorites> UpdateUserFavoritesAsync(UserFavorites userFavorites, EntityHeader user, EntityHeader org)
        {
            await _userFavoritesRepo.UpdateUserFavoritesAsync(userFavorites);
            return userFavorites;
        }
    }
}
