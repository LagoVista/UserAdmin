using LagoVista.UserAdmin.Interfaces.Repos.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;
using LagoVista.CloudStorage.Storage;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.PlatformSupport;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class AssetSetRepo : DocumentDBRepoBase<AssetSet>, IAssetSetRepo
    {
        bool _shouldConsolidateCollections;
        public AssetSetRepo(IUserAdminSettings userAdminSettings, ILogger logger) :
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
