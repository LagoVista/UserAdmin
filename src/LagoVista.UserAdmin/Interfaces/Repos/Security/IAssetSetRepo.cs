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
