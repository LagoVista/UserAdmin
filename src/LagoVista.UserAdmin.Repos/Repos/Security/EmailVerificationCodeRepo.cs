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
    internal class EmailVerificationCodeEntity : TableStorageEntity
    {
        public string CodeHash { get; set; }
        public string CreatedUtc { get; set; }
        public string ExpiresUtc { get; set; }
        public int AttemptCount { get; set; }
        public string ConsumedUtc { get; set; }

        public static EmailVerificationCodeEntity FromModel(EmailVerificationCode verificationCode)
        {
            return new EmailVerificationCodeEntity
            {
                PartitionKey = verificationCode.UserId,
                RowKey = verificationCode.Id,
                CodeHash = verificationCode.CodeHash,
                CreatedUtc = verificationCode.CreatedUtc.ToString("O"),
                ExpiresUtc = verificationCode.ExpiresUtc.ToString("O"),
                AttemptCount = verificationCode.AttemptCount,
                ConsumedUtc = verificationCode.ConsumedUtc?.ToString("O")
            };
        }

        public EmailVerificationCode ToModel()
        {
            return new EmailVerificationCode
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

    internal class EmailVerificationCodeRepo : TableStorageBase<EmailVerificationCodeEntity>, IEmailVerificationCodeRepo
    {
        public EmailVerificationCodeRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "emailverificationcode";
        }

        public Task StoreAsync(EmailVerificationCode verificationCode)
        {
            return InsertAsync(EmailVerificationCodeEntity.FromModel(verificationCode));
        }

        public async Task<EmailVerificationCode> GetLatestAsync(string userId)
        {
            var entities = await GetByPartitionIdAsync(userId);
            return entities.OrderByDescending(entity => entity.CreatedUtc).FirstOrDefault()?.ToModel();
        }

        public async Task UpdateAsync(EmailVerificationCode verificationCode)
        {
            var entity = await GetAsync(verificationCode.UserId, verificationCode.Id, false);
            if (entity == null) return;

            entity.AttemptCount = verificationCode.AttemptCount;
            entity.ConsumedUtc = verificationCode.ConsumedUtc?.ToString("O");
            await base.UpdateAsync(entity);
        }

        public async Task ClearAsync(string userId)
        {
            var entities = await GetByPartitionIdAsync(userId);
            foreach (var entity in entities) await DeleteAsync(entity);
        }
    }
}
