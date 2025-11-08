// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5018cafc1f6894281f768a3652fb268773e06cc11b0115af4541be43cfc8372e
// IndexVersion: 2
// --- END CODE INDEX META ---
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
