using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentStateIndexRepo : TableStorageBase<ProvisionalEnvironmentStateIndexEntity>, IProvisionalEnvironmentStateIndexRepo
    {
        public ProvisionalEnvironmentStateIndexRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "provisionalenvironmentstateidx";
        }

        public Task InsertAsync(ProvisionalEnvironmentState state, DateTime expiresUtc, string environmentId)
        {
            return base.InsertAsync(new ProvisionalEnvironmentStateIndexEntity
            {
                PartitionKey = ProvisionalEnvironmentStateIndexEntity.CreatePartitionKey(state, environmentId),
                RowKey = ProvisionalEnvironmentStateIndexEntity.CreateRowKey(expiresUtc, environmentId),
                EnvironmentId = environmentId,
                State = state.ToString(),
                ExpiresUtc = expiresUtc.ToUniversalTime().ToString("O")
            });
        }

        public async Task<bool> ExistsAsync(ProvisionalEnvironmentState state, DateTime expiresUtc, string environmentId)
        {
            var entity = await GetAsync(ProvisionalEnvironmentStateIndexEntity.CreatePartitionKey(state, environmentId), ProvisionalEnvironmentStateIndexEntity.CreateRowKey(expiresUtc, environmentId), false);
            return entity != null;
        }

        public async Task<IEnumerable<string>> FindEnvironmentIdsAsync(ProvisionalEnvironmentState state, DateTime? expiresBeforeUtc, int take)
        {
            if (take <= 0) return Enumerable.Empty<string>();

            var partitionReads = Enumerable.Range(0, ProvisionalEnvironmentStateIndexEntity.ShardCount).Select(shard => GetByPartitionIdAsync(ProvisionalEnvironmentStateIndexEntity.CreatePartitionKey(state, shard)));
            var partitions = await Task.WhenAll(partitionReads);
            var candidates = partitions.SelectMany(items => items);

            var query = candidates.Select(entity => new
            {
                Entity = entity,
                ExpiresUtc = DateTime.Parse(entity.ExpiresUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            });

            if (expiresBeforeUtc.HasValue)
            {
                var cutoffUtc = expiresBeforeUtc.Value.ToUniversalTime();
                query = query.Where(item => item.ExpiresUtc <= cutoffUtc);
            }

            return query.OrderBy(item => item.ExpiresUtc).Take(take).Select(item => item.Entity.EnvironmentId).ToList();
        }

        public Task DeleteAsync(ProvisionalEnvironmentState state, DateTime expiresUtc, string environmentId)
        {
            return RemoveAsync(ProvisionalEnvironmentStateIndexEntity.CreatePartitionKey(state, environmentId), ProvisionalEnvironmentStateIndexEntity.CreateRowKey(expiresUtc, environmentId));
        }
    }
}
