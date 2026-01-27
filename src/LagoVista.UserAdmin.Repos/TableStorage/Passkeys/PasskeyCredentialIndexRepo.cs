using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.Passkeys
{
    public class PasskeyCredentialIndexRepo : TableStorageBase<PasskeyCredentialIndexEntity>, IPasskeyCredentialIndexRepo
    {
        IAdminLogger _adminLogger;

        public PasskeyCredentialIndexRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task InsertAsync(PasskeyCredentialIndex index)
        {
            if (index == null) throw new ArgumentNullException(nameof(index));
            var entity = ToEntity(index);
            _adminLogger.Trace($"{this.Tag()} - Inserting PasskeyCredentialIndex for UserId: {index.UserId}, RpId: {index.RpId}, CredentialId: {index.CredentialId}");
            await InsertAsync(entity);
        }

        private async Task<PasskeyCredentialIndexEntity> FindEntityAsync(string rpId, string credentialId)
        {
            var partitionKey = PasskeyCredentialIndexEntity.CreatePartitionKey(rpId, credentialId);
            var rowKey = PasskeyCredentialIndexEntity.CreateRowKey(credentialId);
            return await GetAsync(partitionKey, rowKey);
        }

        private static PasskeyCredentialIndexEntity ToEntity(PasskeyCredentialIndex index)
        {
            return new PasskeyCredentialIndexEntity()
            {
                PartitionKey = PasskeyCredentialIndexEntity.CreatePartitionKey(index.RpId, index.CredentialId),
                RowKey = PasskeyCredentialIndexEntity.CreateRowKey(index.CredentialId),
                UserId = index.UserId,
                RpId = index.RpId,
                CredentialId = index.CredentialId,
                CreatedUtc = index.CreatedUtc,
            };
        }

        private static PasskeyCredentialIndex ToModel(PasskeyCredentialIndexEntity entity)
        {
            return new PasskeyCredentialIndex()
            {
                UserId = entity.UserId,
                RpId = entity.RpId,
                CredentialId = entity.CredentialId,
                CreatedUtc = entity.CreatedUtc,
            };
        }
        public async Task<PasskeyCredentialIndex> FindAsync(string rpId, string credentialId)
        {
            _adminLogger.Trace($"{this.Tag()} - Finding PasskeyCredentialIndex for RpId: {rpId}, CredentialId: {credentialId}");
        
            var partitionKey = PasskeyCredentialIndexEntity.CreatePartitionKey(rpId, credentialId);
            var rowKey = PasskeyCredentialIndexEntity.CreateRowKey(credentialId);
            var entity = await GetAsync(partitionKey, rowKey);
            return entity == null ? null : ToModel(entity);
        }

        public async Task RemoveAsync(string rpId, string credentialId)
        {
            _adminLogger.Trace($"{this.Tag()} - Removing PasskeyCredentialIndex for RpId: {rpId}, CredentialId: {credentialId}");
        
         var entity = await FindEntityAsync(rpId, credentialId);
            if (entity != null) await RemoveAsync(entity);
        }

        public static PasskeyCredentialIndex ToIndexModel(PasskeyCredential credential)
        {
            return new PasskeyCredentialIndex()
            {
                UserId = credential.UserId,
                RpId = credential.RpId,
                CredentialId = credential.CredentialId,
                CreatedUtc = credential.CreatedUtc,
            };
        }
    }
}
