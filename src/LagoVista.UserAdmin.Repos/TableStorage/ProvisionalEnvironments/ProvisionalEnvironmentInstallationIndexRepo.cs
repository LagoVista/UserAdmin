using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentInstallationIndexRepo : TableStorageBase<ProvisionalEnvironmentInstallationIndexEntity>, IProvisionalEnvironmentInstallationIndexRepo
    {
        public ProvisionalEnvironmentInstallationIndexRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "provisionalenvironmentinstallationidx";
        }

        public Task InsertAsync(string installationIdHash, string environmentId, DateTime createdUtc)
        {
            if (String.IsNullOrEmpty(environmentId)) throw new ArgumentNullException(nameof(environmentId));

            return base.InsertAsync(new ProvisionalEnvironmentInstallationIndexEntity
            {
                PartitionKey = ProvisionalEnvironmentInstallationIndexEntity.CreatePartitionKey(installationIdHash),
                RowKey = ProvisionalEnvironmentInstallationIndexEntity.CreateRowKey(installationIdHash),
                InstallationIdHash = installationIdHash,
                EnvironmentId = environmentId,
                CreatedUtc = createdUtc.ToString("O")
            });
        }

        public async Task<string> FindEnvironmentIdAsync(string installationIdHash)
        {
            var entity = await GetAsync(ProvisionalEnvironmentInstallationIndexEntity.CreatePartitionKey(installationIdHash), ProvisionalEnvironmentInstallationIndexEntity.CreateRowKey(installationIdHash), false);
            return entity?.EnvironmentId;
        }

        public Task DeleteAsync(string installationIdHash)
        {
            return RemoveAsync(ProvisionalEnvironmentInstallationIndexEntity.CreatePartitionKey(installationIdHash), ProvisionalEnvironmentInstallationIndexEntity.CreateRowKey(installationIdHash));
        }
    }
}
