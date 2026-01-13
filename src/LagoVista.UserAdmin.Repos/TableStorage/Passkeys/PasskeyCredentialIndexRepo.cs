using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.Passkeys
{
    public class PasskeyCredentialIndexRepo : TableStorageBase<PasskeyCredentialIndexEntity>
    {
        public PasskeyCredentialIndexRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        public async Task<PasskeyCredentialIndexEntity> FindAsync(string rpId, string credentialId)
        {
            var partitionKey = PasskeyCredentialIndexEntity.CreatePartitionKey(rpId, credentialId);
            var rowKey = PasskeyCredentialIndexEntity.CreateRowKey(credentialId);
            return await GetAsync(partitionKey, rowKey);
        }

        public async Task RemoveAsync(string rpId, string credentialId)
        {
            var entity = await FindAsync(rpId, credentialId);
            if (entity != null) await RemoveAsync(entity);
        }

        public static PasskeyCredentialIndexEntity ToIndexEntity(PasskeyCredential credential)
        {
            return new PasskeyCredentialIndexEntity()
            {
                PartitionKey = PasskeyCredentialIndexEntity.CreatePartitionKey(credential.RpId, credential.CredentialId),
                RowKey = PasskeyCredentialIndexEntity.CreateRowKey(credential.CredentialId),
                UserId = credential.UserId,
                RpId = credential.RpId,
                CredentialId = credential.CredentialId,
                CreatedUtc = credential.CreatedUtc,
            };
        }
    }
}
