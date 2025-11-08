// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 435b2e97b69c463a87fe24251bd69dfa01672e29b05f0a27b87511aa4d89df4a
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using System.Collections.Generic;
using System.Linq;
using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Repos.Users
{
    public class AssetSetRepo : DocumentDBRepoBase<AssetSet>, IAssetSetRepo
    {
        bool _shouldConsolidateCollections;
        public AssetSetRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddAssetSetAsync(AssetSet assetSet)
        {
            return CreateDocumentAsync(assetSet);
        }

        public Task DeleteAssetSetAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<AssetSet> GetAssetSetAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<IEnumerable<AssetSetSummary>> GetAssetSetsForOrgAsync(string orgId)
        {
            var items = await base.QueryAsync(qry => qry.OwnerOrganization.Id == orgId);
            return from item in items
                   select item.CreateSummary();
        }

        public async Task<bool> QueryKeyInUseAsync(string key, string orgId)
        {
            var items = await base.QueryAsync(attr => (attr.OwnerOrganization.Id == orgId || attr.IsPublic == true) && attr.Key == key);
            return items.Any();
        }

        public Task UpdateAssetSetAsync(AssetSet assetSet)
        {
            return UpsertDocumentAsync(assetSet);
        }
    }
}
