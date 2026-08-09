using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal class AnonymousVisitorStateIndexRepo : TableStorageBase<AnonymousVisitorStateIndexEntity>, IAnonymousVisitorStateIndexRepo
    {
        public AnonymousVisitorStateIndexRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "anonymousvisitorstateidx";
        }

        public Task InsertAsync(AnonymousVisitorState state, DateTime dueUtc, string actorId)
        {
            return base.InsertAsync(new AnonymousVisitorStateIndexEntity { PartitionKey = AnonymousVisitorStateIndexEntity.CreatePartitionKey(state, actorId), RowKey = AnonymousVisitorStateIndexEntity.CreateRowKey(dueUtc, actorId), ActorId = actorId, State = state.ToString(), DueUtc = dueUtc.ToUniversalTime().ToString("O") });
        }

        public async Task<bool> ExistsAsync(AnonymousVisitorState state, DateTime dueUtc, string actorId)
        {
            return await GetAsync(AnonymousVisitorStateIndexEntity.CreatePartitionKey(state, actorId), AnonymousVisitorStateIndexEntity.CreateRowKey(dueUtc, actorId), false) != null;
        }

        public async Task<IEnumerable<string>> FindActorIdsAsync(AnonymousVisitorState state, DateTime? dueBeforeUtc, int take)
        {
            if (take <= 0) return Enumerable.Empty<string>();
            var partitions = await Task.WhenAll(Enumerable.Range(0, AnonymousVisitorStateIndexEntity.ShardCount).Select(shard => GetByPartitionIdAsync(AnonymousVisitorStateIndexEntity.CreatePartitionKey(state, shard))));
            var query = partitions.SelectMany(items => items).Select(entity => new { Entity = entity, DueUtc = DateTime.Parse(entity.DueUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) });
            if (dueBeforeUtc.HasValue)
            {
                var cutoffUtc = dueBeforeUtc.Value.ToUniversalTime();
                query = query.Where(item => item.DueUtc <= cutoffUtc);
            }

            return query.OrderBy(item => item.DueUtc).Take(take).Select(item => item.Entity.ActorId).ToList();
        }

        public Task DeleteAsync(AnonymousVisitorState state, DateTime dueUtc, string actorId)
        {
            return RemoveAsync(AnonymousVisitorStateIndexEntity.CreatePartitionKey(state, actorId), AnonymousVisitorStateIndexEntity.CreateRowKey(dueUtc, actorId));
        }
    }
}
