// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1000e3f2accf281b7fade47ab7bba6f7728093f89a6a61103cae62aa7f33c9d4
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IAssetSetRepo
    {
        Task AddAssetSetAsync(AssetSet assetSet);
        Task UpdateAssetSetAsync(AssetSet assetSet);
        Task DeleteAssetSetAsync(string id);
        Task<AssetSet> GetAssetSetAsync(String id);
        Task<IEnumerable<AssetSetSummary>> GetAssetSetsForOrgAsync(string orgId);
        Task<bool> QueryKeyInUseAsync(string key, string orgId);
    }
}
