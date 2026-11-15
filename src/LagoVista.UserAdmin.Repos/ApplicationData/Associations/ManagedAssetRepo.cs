using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.Associations
{
    /// <summary>
    /// Mongo-only asset-set associations. Supports indexed lookup in both
    /// directions and deliberate deletion, without legacy Table Storage.
    /// </summary>
    public sealed class ManagedAssetRepo : IManagedAssetRepo
    {
        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;

        public ManagedAssetRepo(IApplicationDataStore store, IAppConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public Task AddManagedAssetAsync(ManagedAsset newMember)
            => _store.InsertAsync(ManagedAssetAssociationRecord.FromModel(newMember,
                AssociationRecordIdentity.RequirePlatformOrg(_config)));

        public async Task<IEnumerable<AssetSetSummary>> GetAssetSetsForManagedAssetAsync(string assetId)
        {
            if (String.IsNullOrWhiteSpace(assetId)) throw new ArgumentException("AssetId is required.", nameof(assetId));
            var query = InPlatformOrg()
                .Where(record => record.AssetId, StorageFilterOperator.Equal, assetId)
                .OrderBy(record => record.AssetSetId);
            var matches = await ReadAllAsync(query).ConfigureAwait(false);
            return matches.Select(record => new AssetSetSummary
            {
                Id = record.AssetSetId,
                Name = record.Association?.AssetSetName
            }).ToList();
        }

        public async Task<IEnumerable<ManagedAssetSummary>> GetManagedAssetsAsync(string assetSetId)
        {
            if (String.IsNullOrWhiteSpace(assetSetId))
                throw new ArgumentException("AssetSetId is required.", nameof(assetSetId));
            var query = InPlatformOrg()
                .Where(record => record.AssetSetId, StorageFilterOperator.Equal, assetSetId)
                .OrderBy(record => record.AssetId);
            var matches = await ReadAllAsync(query).ConfigureAwait(false);
            return matches.Where(record => record.Association != null)
                .Select(record => record.Association.CreateSummary()).ToList();
        }

        public Task RemoveManagedAssetAsync(string assetSetId, string assetId)
            => _store.DeleteAsync<ManagedAssetAssociationRecord>(new StorageKey(
                AssociationRecordIdentity.Create("managed-asset", assetSetId, assetId).Value,
                AssociationRecordIdentity.RequirePlatformOrg(_config).Id));

        private StorageQuery<ManagedAssetAssociationRecord> InPlatformOrg()
            => new StorageQuery<ManagedAssetAssociationRecord>()
                .Where(record => record.Organization.Id, StorageFilterOperator.Equal,
                    AssociationRecordIdentity.RequirePlatformOrg(_config).Id);

        private async Task<List<ManagedAssetAssociationRecord>> ReadAllAsync(
            StorageQuery<ManagedAssetAssociationRecord> query)
        {
            var result = new List<ManagedAssetAssociationRecord>();
            string next = null;
            do
            {
                var page = await _store.QueryAsync(
                    query.WithPage(new StoragePageRequest(500, next))).ConfigureAwait(false);
                result.AddRange(page.Items);
                next = page.ContinuationToken;
            } while (!String.IsNullOrWhiteSpace(next));
            return result;
        }
    }
}
