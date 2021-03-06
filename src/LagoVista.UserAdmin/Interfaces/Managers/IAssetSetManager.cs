﻿using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IAssetSetManager
    {
        Task<InvokeResult> AddAssetSetAsync(AssetSet assetSet, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateAssetSetAsync(AssetSet assetSet, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteAssetSetAsync(String id, EntityHeader org, EntityHeader user);
        Task<DependentObjectCheckResult> CheckInUseAsync(String id, EntityHeader org, EntityHeader user);
        Task<AssetSet> GetAssetSetAsync(String id, EntityHeader org, EntityHeader user);

        Task<IEnumerable<AssetSetSummary>> GetAssetSetsForOrgAsync(string orgId, EntityHeader user);

        Task<IEnumerable<ManagedAssetSummary>> GetManagedAssetsFromAssetSetAsync(String assetSetId, EntityHeader org, EntityHeader user);

        Task<IEnumerable<AssetSetSummary>> GetAssetSetsForManagedAssetAsync(String assetId, EntityHeader org, EntityHeader user);

        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org);

        Task<InvokeResult> AddManagedAssetAsync(ManagedAsset asset, EntityHeader org, EntityHeader user);

        Task<InvokeResult> RemoveManagedAssetAsync(String assetSetId, String assetId, EntityHeader org, EntityHeader user);
    }
}
