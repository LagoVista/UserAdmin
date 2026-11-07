using LagoVista.CloudStorage.Storage;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Commo
{
    public class SentEmailXRefRepo
    {
        private readonly IOperationalDataStore<SentEmailXRefOperationalRecord> _store;

        public SentEmailXRefRepo(IOperationalDataStore<SentEmailXRefOperationalRecord> store)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
        }

        public Task AddAsync(string externalMessageId, string orgId, string sentEmailRowKey)
        {
            if (String.IsNullOrWhiteSpace(externalMessageId))
                throw new ArgumentException("External message ID is required.", nameof(externalMessageId));
            if (String.IsNullOrWhiteSpace(orgId))
                throw new ArgumentException("Organization ID is required.", nameof(orgId));
            if (String.IsNullOrWhiteSpace(sentEmailRowKey))
                throw new ArgumentException("Sent email row key is required.", nameof(sentEmailRowKey));

            var now = DateTime.UtcNow;
            return _store.UpsertAsync(new SentEmailXRefOperationalRecord
            {
                Id = externalMessageId,
                OrganizationId = externalMessageId,
                SentEmailOrganizationId = orgId,
                SentEmailRowKey = sentEmailRowKey,
                CreationDate = now,
                LastUpdatedDate = now
            });
        }

        public async Task<SentEmailXRef> ResolveAsync(string externalMessageId)
        {
            if (String.IsNullOrWhiteSpace(externalMessageId))
                return null;

            var record = await _store.GetAsync(externalMessageId, externalMessageId).ConfigureAwait(false);
            return record == null ? null : new SentEmailXRef
            {
                OrgId = record.SentEmailOrganizationId,
                SentEmailRowKey = record.SentEmailRowKey
            };
        }
    }
}
