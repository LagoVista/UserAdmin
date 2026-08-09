using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal class AnonymousVisitorContinuityIndexRepo : TableStorageBase<AnonymousVisitorLookupEntity>, IAnonymousVisitorContinuityIndexRepo
    {
        private const string LookupType = "CON";

        public AnonymousVisitorContinuityIndexRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "anonymousvisitorcontinuityidx";
        }

        public Task InsertAsync(string lookupHash, string actorId, DateTime createdUtc)
        {
            if (String.IsNullOrEmpty(actorId)) throw new ArgumentNullException(nameof(actorId));
            return base.InsertAsync(new AnonymousVisitorLookupEntity { PartitionKey = AnonymousVisitorLookupEntity.CreatePartitionKey(LookupType, lookupHash), RowKey = AnonymousVisitorLookupEntity.CreateRowKey(LookupType, lookupHash), ActorId = actorId, LookupHash = lookupHash, CreatedUtc = createdUtc.ToUniversalTime().ToString("O") });
        }

        public async Task<string> FindActorIdAsync(string lookupHash)
        {
            var entity = await GetAsync(AnonymousVisitorLookupEntity.CreatePartitionKey(LookupType, lookupHash), AnonymousVisitorLookupEntity.CreateRowKey(LookupType, lookupHash), false);
            return entity?.ActorId;
        }

        public Task DeleteAsync(string lookupHash)
        {
            return RemoveAsync(AnonymousVisitorLookupEntity.CreatePartitionKey(LookupType, lookupHash), AnonymousVisitorLookupEntity.CreateRowKey(LookupType, lookupHash));
        }
    }
}
