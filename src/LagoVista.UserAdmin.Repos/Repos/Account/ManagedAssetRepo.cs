using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Repos.Users
{
    public class ManagedAssetRepo : TableStorageBase<ManagedAsset>, IManagedAssetRepo
    {
        public ManagedAssetRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task AddManagedAssetAsync(ManagedAsset newMember)
        {
            return InsertAsync(newMember);
        }

        public async Task<IEnumerable<AssetSetSummary>> GetAssetSetsForManagedAssetAsync(string assetId)
        {
            var assetSets = await GetByFilterAsync(FilterOptions.Create(nameof(ManagedAsset.AssetId), FilterOptions.Operators.Equals, assetId));
            return from assetSet in assetSets
                   select new AssetSetSummary() { Id = assetSet.AssetSetId, Name = assetSet.AssetSetName };
        }

        public async Task<IEnumerable<ManagedAssetSummary>> GetManagedAssetsAsync(string assetSetId)
        {
            var managedAssets = await GetByParitionIdAsync(assetSetId);
            return from managedAsset in managedAssets
                   select managedAsset.CreateSummary();
        }

        public async Task RemoveManagedAssetAsync(string assetSetId, string assetId)
        {
            var managedAsset = await GetAsync(ManagedAsset.CreateRowKey(assetSetId, assetId));
            await RemoveAsync(managedAsset);
        }
    }
}
