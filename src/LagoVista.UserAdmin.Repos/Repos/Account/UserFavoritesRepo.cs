﻿using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core;
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
        public UserFavoritesRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
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
            return CreateDocumentAsync(userFavorites);
        }

        public async Task<UserFavorites> GetUserFavoritesAsync(string userId, string orgId)
        {
            var result = await QueryAsync(doc => doc.OwnerUser.Id == userId && doc.OwnerOrganization.Id == orgId);
            return result.SingleOrDefault();
        }
    }
}
