﻿using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class UserFavoritesRepo : DocumentDBRepoBase<UserFavorites>, IUserFavoritesRepo
    {
        bool _shouldConsolidateCollections;
        public UserFavoritesRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task UpdateUserFavoritesAsync(UserFavorites userFavorite)
        {
            return UpsertDocumentAsync(userFavorite);
        }

        public Task AddUserFavoritesAsync(UserFavorites userFavorites)
        {
            userFavorites.Id = UserFavorites.GenerateId(userFavorites.OwnerOrganization, userFavorites.OwnerUser);
          
            Console.WriteLine($"Adding user favorites {userFavorites.Name} {userFavorites.Id} {userFavorites.Key}");

            return CreateDocumentAsync(userFavorites);
        }

        public async Task<UserFavorites> GetUserFavoritesAsync(string userId, string orgId)
        {
            var id = UserFavorites.GenerateId(orgId, userId);
            return await GetDocumentAsync(id, false);
        }
    }
}
