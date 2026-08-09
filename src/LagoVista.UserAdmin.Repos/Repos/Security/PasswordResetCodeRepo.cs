using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    internal class PasswordResetCodeEntity : TableStorageEntity
    {
        public string CodeHash { get; set; }
        public string CreatedUtc { get; set; }
        public string ExpiresUtc { get; set; }
        public int AttemptCount { get; set; }
        public string ConsumedUtc { get; set; }

        public static PasswordResetCodeEntity FromModel(PasswordResetCode resetCode)
        {
            return new PasswordResetCodeEntity
            {
                PartitionKey = resetCode.UserId,
                RowKey = resetCode.Id,
                CodeHash = resetCode.CodeHash,
                CreatedUtc = resetCode.CreatedUtc.ToString("O"),
                ExpiresUtc = resetCode.ExpiresUtc.ToString("O"),
                AttemptCount = resetCode.AttemptCount,
                ConsumedUtc = resetCode.ConsumedUtc?.ToString("O")
            };
        }

        public PasswordResetCode ToModel()
        {
            return new PasswordResetCode
            {
                Id = RowKey,
                UserId = PartitionKey,
                CodeHash = CodeHash,
                CreatedUtc = DateTime.Parse(CreatedUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
                ExpiresUtc = DateTime.Parse(ExpiresUtc, null, System.Globalization.DateTimeStyles.RoundtripKind),
                AttemptCount = AttemptCount,
                ConsumedUtc = String.IsNullOrEmpty(ConsumedUtc) ? (DateTime?)null : DateTime.Parse(ConsumedUtc, null, System.Globalization.DateTimeStyles.RoundtripKind)
            };
        }
    }

    internal class PasswordResetCodeRepo : TableStorageBase<PasswordResetCodeEntity>, IPasswordResetCodeRepo
    {
        public PasswordResetCodeRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "passwordresetcode";
        }

        public Task StoreAsync(PasswordResetCode resetCode)
        {
            return InsertAsync(PasswordResetCodeEntity.FromModel(resetCode));
        }

        public async Task<PasswordResetCode> GetLatestAsync(string userId)
        {
            var entities = await GetByPartitionIdAsync(userId);
            return entities.OrderByDescending(entity => entity.CreatedUtc).FirstOrDefault()?.ToModel();
        }

        public async Task UpdateAsync(PasswordResetCode resetCode)
        {
            var entity = await GetAsync(resetCode.UserId, resetCode.Id, false);
            if (entity == null) return;

            entity.AttemptCount = resetCode.AttemptCount;
            entity.ConsumedUtc = resetCode.ConsumedUtc?.ToString("O");
            await base.UpdateAsync(entity);
        }

        public async Task ClearAsync(string userId)
        {
            var entities = await GetByPartitionIdAsync(userId);
            foreach (var entity in entities) await DeleteAsync(entity);
        }
    }
}
