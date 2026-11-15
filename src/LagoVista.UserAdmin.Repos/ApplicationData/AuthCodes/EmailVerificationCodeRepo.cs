using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.AuthCodes
{
    /// <summary>
    /// Mongo-backed EmailVerificationCode repository. Revision-safe consumption and attempt
    /// updates preserve one-time semantics under concurrent verification.
    /// </summary>
    public sealed class EmailVerificationCodeRepo : IEmailVerificationCodeRepo
    {
        private readonly VerificationCodeStore _codes;
        private readonly ConditionalWeakTable<EmailVerificationCode,
            VersionedApplicationDataRecord<VerificationCodeRecord>> _versions =
            new ConditionalWeakTable<EmailVerificationCode, VersionedApplicationDataRecord<VerificationCodeRecord>>();

        public EmailVerificationCodeRepo(IApplicationDataStore store, IAppConfig config)
        {
            _codes = new VerificationCodeStore(store, config);
        }

        public Task StoreAsync(EmailVerificationCode code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            return _codes.StoreAsync(VerificationCodeRecord.EmailVerificationKind,
                code.Id, code.UserId, code.CodeHash, code.CreatedUtc,
                code.ExpiresUtc, code.AttemptCount, code.ConsumedUtc);
        }

        public async Task<EmailVerificationCode> GetLatestAsync(string userId)
        {
            var read = await _codes.GetLatestAsync(
                VerificationCodeRecord.EmailVerificationKind, userId).ConfigureAwait(false);
            if (read == null) return null;

            var code = new EmailVerificationCode
            {
                Id = read.Record.Id.Value,
                UserId = read.Record.UserId,
                CodeHash = read.Record.CodeHash,
                CreatedUtc = read.Record.IssuedUtc,
                ExpiresUtc = read.Record.ExpiresUtc,
                AttemptCount = read.Record.AttemptCount,
                ConsumedUtc = read.Record.ConsumedUtc
            };
            _versions.Add(code, read);
            return code;
        }

        public Task UpdateAsync(EmailVerificationCode code)
        {
            if (code == null) throw new ArgumentNullException(nameof(code));
            if (!_versions.TryGetValue(code, out var current))
                throw new InvalidOperationException(
                    "EmailVerificationCode must be loaded from GetLatestAsync before updating.");
            if (!String.Equals(current.Record.Id.Value, code.Id, StringComparison.Ordinal) ||
                !String.Equals(current.Record.UserId, code.UserId, StringComparison.Ordinal))
                throw new InvalidOperationException("Verification code identity cannot be changed.");

            return _codes.UpdateAsync(current, code.AttemptCount, code.ConsumedUtc);
        }

        public Task ClearAsync(string userId)
            => _codes.ClearAsync(VerificationCodeRecord.EmailVerificationKind, userId);
    }
}
