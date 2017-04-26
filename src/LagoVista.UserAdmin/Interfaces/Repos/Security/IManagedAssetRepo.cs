using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IManagedAssetRepo
    {
        Task AddManagedAssetAsync(ManagedAsset newMember);
        
        Task RemoveManagedAssetAsync(String assetSetId, String assetId);

        Task<IEnumerable<ManagedAssetSummary>> GetManagedAssetsAsync(String assetSetId);

        Task<IEnumerable<AssetSetSummary>> GetAssetSetsForManagedAssetAsync(String assetId);
    }
}
