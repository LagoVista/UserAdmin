// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: bb4bb8c06fef52d35514604b7b876a35af6beb4f2ba8e2228d9c719ac6215c1d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(
        Domains.UserDomain,
        UserAdminResources.Names.ManagedAsset_Name,
        UserAdminResources.Names.ManagedAsset_Help,
        UserAdminResources.Names.ManagedAsset_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class ManagedAsset : TableStorageEntity
    {
        public ManagedAsset(EntityHeader assetSet, EntityHeader asset, String assetType)
        {
            RowKey = CreateRowKey(assetSet, asset);
            AssetId = asset.Id;
            AssetName = asset.Text;
            AssetType = assetType;
            AssetSetId = assetSet.Id;
            AssetSetName = assetSet.Text;
        }

        public ManagedAsset()
        {

        }

        public string AssetSetId { get; set; }
        public string AssetSetName { get; set; }
        public string AssetId { get; set; }
        public string AssetName { get; set; }
        public string AssetType { get; set; }

        public static string CreateRowKey(EntityHeader assetSet, EntityHeader asset)
        {
            return $"{assetSet.Id}.{asset.Id}";
        }

        public static string CreateRowKey(string assetSetId, string assetId)
        {
            return $"{assetSetId}.{assetId}";
        }

        public ManagedAssetSummary CreateSummary()
        {
            return new ManagedAssetSummary()
            {
                Id = AssetId,
                Name = AssetName,
                AssetType = AssetType
            };
        }
    }

    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.ManagedAssetSummary_Name,
        UserAdminResources.Names.ManagedAssetSummary_Help,
        UserAdminResources.Names.ManagedAssetSummary_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class ManagedAssetSummary : SummaryData
    {
        public string AssetType { get; set; }
    }
}
