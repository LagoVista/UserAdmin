using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.Passkeys
{
    public class PasskeyCredentialRepo : TableStorageBase<PasskeyCredentialEntity>, IAppUserPasskeyCredentialRepo
    {
        private readonly IPasskeyCredentialIndexRepo _indexRepo;
        private readonly IAdminLogger _adminLogger;

        public PasskeyCredentialRepo(IUserAdminSettings settings, IAdminLogger logger, IPasskeyCredentialIndexRepo indexRepo) : 
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            _indexRepo = indexRepo ?? throw new ArgumentNullException(nameof(indexRepo));
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<InvokeResult> AddAsync(PasskeyCredential credential)
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            if (String.IsNullOrEmpty(credential.UserId)) return InvokeResult.FromError("missing_user_id");
            if (String.IsNullOrEmpty(credential.RpId)) return InvokeResult.FromError("missing_rp_id");
            if (String.IsNullOrEmpty(credential.CredentialId)) return InvokeResult.FromError("missing_credential_id");
            if (String.IsNullOrEmpty(credential.PublicKey)) return InvokeResult.FromError("missing_public_key");

            var entity = ToEntity(credential);

            _adminLogger.Trace($"{this.Tag()} - Adding PasskeyCredential for UserId: {credential.UserId}, RpId: {credential.RpId}, CredentialId: {credential.CredentialId}");

            await InsertAsync(entity);

            var indexModel = PasskeyCredentialIndexRepo.ToIndexModel(credential);
            await _indexRepo.InsertAsync(indexModel);

            return InvokeResult.Success;
        }

        public async Task<IEnumerable<PasskeyCredential>> GetByUserAsync(string userId, string rpId)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (String.IsNullOrEmpty(rpId)) throw new ArgumentNullException(nameof(rpId));

            _adminLogger.Trace($"{this.Tag()} - Getting PasskeyCredential for UserId: {userId}, RpId: {rpId}");

            var partitionKey = PasskeyCredentialEntity.CreatePartitionKey(userId, rpId);
            var entities = await GetByPartitionIdAsync(partitionKey);
            return entities.Select(ToModel);
        }

        public async Task<PasskeyCredential> FindByCredentialIdAsync(string rpId, string credentialId)
        {
            if (String.IsNullOrEmpty(rpId)) throw new ArgumentNullException(nameof(rpId));
            if (String.IsNullOrEmpty(credentialId)) throw new ArgumentNullException(nameof(credentialId));

            _adminLogger.Trace($"{this.Tag()} - Finding PasskeyCredential for RpId: {rpId}, CredentialId: {credentialId}");

            var index = await _indexRepo.FindAsync(rpId, credentialId);
            if (index == null) return null;

            var partitionKey = PasskeyCredentialEntity.CreatePartitionKey(index.UserId, rpId);
            var rowKey = PasskeyCredentialEntity.CreateRowKey(credentialId);
            var entity = await GetAsync(partitionKey, rowKey);
            return entity == null ? null : ToModel(entity);
        }

        public async Task<InvokeResult> RemovePasskeyCredentialAsync(string userId, string rpId, string credentialId)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (String.IsNullOrEmpty(rpId)) throw new ArgumentNullException(nameof(rpId));
            if (String.IsNullOrEmpty(credentialId)) throw new ArgumentNullException(nameof(credentialId));

            var partitionKey = PasskeyCredentialEntity.CreatePartitionKey(userId, rpId);
            var rowKey = PasskeyCredentialEntity.CreateRowKey(credentialId);
            var entity = await GetAsync(partitionKey, rowKey);
            if (entity != null) await RemoveAsync(entity);

            await _indexRepo.RemoveAsync(rpId, credentialId);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateSignCountAsync(string userId, string rpId, string credentialId, uint signCount, string lastUsedUtc)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (String.IsNullOrEmpty(rpId)) throw new ArgumentNullException(nameof(rpId));
            if (String.IsNullOrEmpty(credentialId)) throw new ArgumentNullException(nameof(credentialId));

            _adminLogger.Trace($"{this.Tag()} - Updating SignCount for UserId: {userId}, RpId: {rpId}, CredentialId: {credentialId}");

            var partitionKey = PasskeyCredentialEntity.CreatePartitionKey(userId, rpId);
            var rowKey = PasskeyCredentialEntity.CreateRowKey(credentialId);
            var entity = await GetAsync(partitionKey, rowKey);
            if (entity == null) return InvokeResult.FromError("credential_not_found");

            entity.SignCount = signCount;
            entity.LastUsedUtc = lastUsedUtc;

            await UpdateAsync(entity);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdateNameAsync(string userId, string rpId, string credentialId, string name)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (String.IsNullOrEmpty(rpId)) throw new ArgumentNullException(nameof(rpId));
            if (String.IsNullOrEmpty(credentialId)) throw new ArgumentNullException(nameof(credentialId));
            if (String.IsNullOrEmpty(name)) return InvokeResult.FromError("missing_name");

            var partitionKey = PasskeyCredentialEntity.CreatePartitionKey(userId, rpId);
            var rowKey = PasskeyCredentialEntity.CreateRowKey(credentialId);
            var entity = await GetAsync(partitionKey, rowKey);
            if (entity == null) return InvokeResult.FromError("credential_not_found");

            entity.Name = name;
            await UpdateAsync(entity);
            return InvokeResult.Success;
        }

        private static PasskeyCredentialEntity ToEntity(PasskeyCredential credential)
        {
            return new PasskeyCredentialEntity()
            {
                PartitionKey = PasskeyCredentialEntity.CreatePartitionKey(credential.UserId, credential.RpId),
                RowKey = PasskeyCredentialEntity.CreateRowKey(credential.CredentialId),
                UserId = credential.UserId,
                RpId = credential.RpId,
                CredentialId = credential.CredentialId,
                PublicKey = credential.PublicKey,
                SignCount = credential.SignCount,
                Name = credential.Name,
                CreatedUtc = credential.CreatedUtc,
                LastUsedUtc = credential.LastUsedUtc,
            };
        }

        private static PasskeyCredential ToModel(PasskeyCredentialEntity entity)
        {
            return new PasskeyCredential()
            {
                UserId = entity.UserId,
                RpId = entity.RpId,
                CredentialId = entity.CredentialId,
                PublicKey = entity.PublicKey,
                SignCount = entity.SignCount,
                Name = entity.Name,
                CreatedUtc = entity.CreatedUtc,
                LastUsedUtc = entity.LastUsedUtc,
            };
        }
    }
}
