using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal class AnonymousVisitorInstallationIndexRepo : TableStorageBase<AnonymousVisitorLookupEntity>, IAnonymousVisitorInstallationIndexRepo
    {
        private const string LookupType = "INS";

        public AnonymousVisitorInstallationIndexRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "anonymousvisitorinstallationidx";
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
