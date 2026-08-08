using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentCreationIndexRepo : TableStorageBase<ProvisionalEnvironmentCreationIndexEntity>, IProvisionalEnvironmentCreationIndexRepo
    {
        public ProvisionalEnvironmentCreationIndexRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "provisionalenvironmentcreationidx";
        }

        public Task InsertAsync(string creationRequestId, string environmentId, DateTime createdUtc)
        {
            if (String.IsNullOrEmpty(environmentId)) throw new ArgumentNullException(nameof(environmentId));

            return base.InsertAsync(new ProvisionalEnvironmentCreationIndexEntity
            {
                PartitionKey = ProvisionalEnvironmentCreationIndexEntity.CreatePartitionKey(creationRequestId),
                RowKey = ProvisionalEnvironmentCreationIndexEntity.CreateRowKey(creationRequestId),
                CreationRequestId = creationRequestId,
                EnvironmentId = environmentId,
                CreatedUtc = createdUtc.ToString("O")
            });
        }

        public async Task<string> FindEnvironmentIdAsync(string creationRequestId)
        {
            var entity = await GetAsync(ProvisionalEnvironmentCreationIndexEntity.CreatePartitionKey(creationRequestId), ProvisionalEnvironmentCreationIndexEntity.CreateRowKey(creationRequestId), false);
            return entity?.EnvironmentId;
        }

        public Task DeleteAsync(string creationRequestId)
        {
            return RemoveAsync(ProvisionalEnvironmentCreationIndexEntity.CreatePartitionKey(creationRequestId), ProvisionalEnvironmentCreationIndexEntity.CreateRowKey(creationRequestId));
        }
    }
}
