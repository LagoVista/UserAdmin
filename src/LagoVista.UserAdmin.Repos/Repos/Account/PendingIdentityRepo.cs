using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using Security.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class PendingIdentityRepo : DocumentDBRepoBase<PendingIdentity>, IPendingIdentityRepo
    {
        bool _shouldConsolidateCollections;
        public PendingIdentityRepo(IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;


        public Task AddPendingIdentityAsync(PendingIdentity identity)
        {
            return CreateDocumentAsync(identity);
        }

        public Task DeletePendingIdentityAsync(string pendingIdentityId)
        {
            return DeleteDocumentAsync(pendingIdentityId);
        }

        public Task<PendingIdentity> GetPendingIdentityAsync(string pendingIdentityId)
        {
            return GetDocumentAsync(pendingIdentityId);
        }

        public Task UpdatePendingIndentiyAsync(PendingIdentity identity)
        {
            return UpsertDocumentAsync(identity);
        }
    }
}
