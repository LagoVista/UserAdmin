// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: bad6225bb610e2a95b0c8166395965d6d2cd08ac41d8a5e491dd6bc8caf05c22
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class UserFavoritesRepo : DocumentDBRepoBase<UserFavorites>, IUserFavoritesRepo
    {
            public UserFavoritesRepo(IDocumentCloudCachedServices services) :
                base(services)
            {
            }

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
