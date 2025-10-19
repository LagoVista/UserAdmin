// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ba405edc7d40eb849efc7f55707a4b89e83eb2e806fbd8fdfca8863375231b7b
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class MostRecentUsedRepo : DocumentDBRepoBase<MostRecentlyUsed>, IMostRecentlyUsedRepo
    {
        bool _shouldConsolidateCollections;
        public MostRecentUsedRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;


        public Task AddMostRecentlyUsedAsync(MostRecentlyUsed mostRecentlyUsed)
        {
           mostRecentlyUsed.Id = MostRecentlyUsed.GenerateId(mostRecentlyUsed.OwnerOrganization, mostRecentlyUsed.OwnerUser);
           return CreateDocumentAsync(mostRecentlyUsed);
        }

        public Task DeleteMostRecentlyUsedAsync(string orgId, string userId)
        {
            return DeleteDocumentAsync(MostRecentlyUsed.GenerateId(orgId, userId));
        }

        public Task<MostRecentlyUsed> GetMostRecentlyUsedAsync(string orgId, string userId)
        {
            return GetDocumentAsync(MostRecentlyUsed.GenerateId(orgId, userId), false);
        }

        public async Task UpdateMostRecentlyUsedAsync(MostRecentlyUsed mostRecentlyUsed)
        {
            await UpsertDocumentAsync(mostRecentlyUsed);
        }
    }
}
