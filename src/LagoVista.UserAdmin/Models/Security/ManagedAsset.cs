using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
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

        public String AssetSetId { get; set; }
        public string AssetSetName { get; set; }
        public String AssetId { get; set; }
        public string AssetName { get; set; }
        public String AssetType { get; set; }

        public static String CreateRowKey(EntityHeader assetSet, EntityHeader asset)
        {
            return $"{assetSet.Id}.{asset.Id}";
        }

        public static String CreateRowKey(string assetSetId, string assetId)
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

    public class ManagedAssetSummary : SummaryData
    {
        public String AssetType { get; set; }

    }
}
