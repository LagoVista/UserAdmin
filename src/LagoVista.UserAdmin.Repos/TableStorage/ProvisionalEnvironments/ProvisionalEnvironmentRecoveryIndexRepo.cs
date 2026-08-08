using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentRecoveryIndexRepo : TableStorageBase<ProvisionalEnvironmentRecoveryIndexEntity>, IProvisionalEnvironmentRecoveryIndexRepo
    {
        public ProvisionalEnvironmentRecoveryIndexRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "provisionalenvironmentrecoveryidx";
        }

        public Task InsertAsync(string recoveryTokenHash, string environmentId, DateTime createdUtc)
        {
            if (String.IsNullOrEmpty(environmentId)) throw new ArgumentNullException(nameof(environmentId));

            return base.InsertAsync(new ProvisionalEnvironmentRecoveryIndexEntity
            {
                PartitionKey = ProvisionalEnvironmentRecoveryIndexEntity.CreatePartitionKey(recoveryTokenHash),
                RowKey = ProvisionalEnvironmentRecoveryIndexEntity.CreateRowKey(recoveryTokenHash),
                RecoveryTokenHash = recoveryTokenHash,
                EnvironmentId = environmentId,
                CreatedUtc = createdUtc.ToString("O")
            });
        }

        public async Task<string> FindEnvironmentIdAsync(string recoveryTokenHash)
        {
            var entity = await GetAsync(ProvisionalEnvironmentRecoveryIndexEntity.CreatePartitionKey(recoveryTokenHash), ProvisionalEnvironmentRecoveryIndexEntity.CreateRowKey(recoveryTokenHash), false);
            return entity?.EnvironmentId;
        }

        public Task DeleteAsync(string recoveryTokenHash)
        {
            return RemoveAsync(ProvisionalEnvironmentRecoveryIndexEntity.CreatePartitionKey(recoveryTokenHash), ProvisionalEnvironmentRecoveryIndexEntity.CreateRowKey(recoveryTokenHash));
        }
    }
}
