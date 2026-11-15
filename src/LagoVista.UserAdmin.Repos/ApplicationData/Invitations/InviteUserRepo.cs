using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Repos.Orgs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.Invitations
{
    /// <summary>
    /// Durable Mongo invitations with indexed organization, email and status
    /// queries. Legacy invitations import on access; new writes never use Tables.
    /// </summary>
    public sealed class InviteUserRepo : IInviteUserRepo
    {
        private static readonly ConcurrentDictionary<string, byte> ImportedOrganizations =
            new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);
        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;
        private readonly LegacyInvitationReader _legacy;

        public InviteUserRepo(IApplicationDataStore store, IAppConfig config,
            LegacyInvitationReader legacy)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _legacy = legacy ?? throw new ArgumentNullException(nameof(legacy));
        }

        public async Task InsertInvitationAsync(Invitation invitation)
        {
            if (invitation == null) throw new ArgumentNullException(nameof(invitation));
            invitation.Email = InvitationRecord.NormalizeEmail(invitation.Email);
            await _store.InsertAsync(InvitationRecord.FromModel(invitation, RequireOrg()))
                .ConfigureAwait(false);
        }

        public async Task UpdateInvitationAsync(Invitation invitation)
        {
            if (invitation == null) throw new ArgumentNullException(nameof(invitation));
            invitation.Email = InvitationRecord.NormalizeEmail(invitation.Email);
            var record = InvitationRecord.FromModel(invitation, RequireOrg());
            var existing = await _store.GetAsync<InvitationRecord>(
                new StorageKey(record.Id.Value, RequireOrg().Id)).ConfigureAwait(false);
            if (existing == null)
            {
                // A legacy invitation may be updated before any explicit read through the new repo.
                await ImportIfMissingAsync(invitation).ConfigureAwait(false);
            }
            await _store.UpdateAsync(record).ConfigureAwait(false);
        }

        public async Task<Invitation> GetInvitationAsync(string invitationId)
        {
            if (String.IsNullOrWhiteSpace(invitationId)) return null;
            var stored = await _store.GetAsync<InvitationRecord>(
                new StorageKey(InvitationRecord.CreateId(invitationId).Value, RequireOrg().Id))
                .ConfigureAwait(false);
            if (stored != null)
                return String.Equals(stored.Invitation?.RowKey, invitationId, StringComparison.Ordinal)
                    ? stored.Invitation : null;

            return await ImportIfMissingAsync(await _legacy.GetLegacyAsync(invitationId)
                .ConfigureAwait(false)).ConfigureAwait(false);
        }

        public async Task<Invitation> GetInviteByOrgIdAndEmailAsync(string orgId, string email)
        {
            if (String.IsNullOrWhiteSpace(orgId) || String.IsNullOrWhiteSpace(email)) return null;
            var emailKey = InvitationRecord.NormalizeEmail(email);
            var query = ForOrganization(orgId)
                .Where(r => r.EmailKey, StorageFilterOperator.Equal, emailKey)
                .OrderBy(r => r.IssuedUtc, StorageSortDirection.Descending)
                .WithPage(new StoragePageRequest(10));
            var page = await _store.QueryAsync(query).ConfigureAwait(false);
            if (page.Items.Count != 0) return page.Items[0].Invitation;

            var old = await _legacy.FindByOrgEmailAsync(orgId, emailKey).ConfigureAwait(false);
            return old == null ? null : await ImportIfMissingAsync(old).ConfigureAwait(false);
        }

        public Task<ListResponse<Invitation>> GetInvitationsForOrgAsync(
            string orgId, ListRequest listRequest, Invitation.StatusTypes? byStatus)
            => ListAsync(orgId, listRequest, byStatus?.ToString(), false);

        public Task<ListResponse<Invitation>> GetActiveInvitationsForOrgAsync(
            string orgId, ListRequest listRequest)
            => ListAsync(orgId, listRequest, null, true);

        private async Task<ListResponse<Invitation>> ListAsync(
            string orgId, ListRequest request, string status, bool activeOnly)
        {
            if (String.IsNullOrWhiteSpace(orgId)) throw new ArgumentNullException(nameof(orgId));
            if (request == null || request.PageIndex < 1 || request.PageSize < 1 || request.PageSize > 1000)
                return ListResponse<Invitation>.FromError("PageIndex must be at least 1 and PageSize from 1 to 1000.");
            var scope = "invitations:" + orgId + ":" + (status ?? (activeOnly ? "active" : "all"));
            var hasScope = !String.IsNullOrWhiteSpace(request.NextPartitionKey);
            var hasCursor = !String.IsNullOrWhiteSpace(request.NextRowKey);
            if (hasScope != hasCursor ||
                (hasScope && !String.Equals(request.NextPartitionKey, scope, StringComparison.Ordinal)) ||
                (request.PageIndex > 1 && !hasCursor))
                return ListResponse<Invitation>.FromError("Invitation continuation cursor is invalid.");

            // Import old organization rows in bounded Azure pages on first use.
            // A process-local marker avoids re-reading legacy data on each request.
            await EnsureOrgImportedAsync(orgId).ConfigureAwait(false);

            var query = ForOrganization(orgId)
                .OrderBy(r => r.IssuedUtc, StorageSortDirection.Descending)
                .WithPage(new StoragePageRequest(request.PageSize,
                    hasCursor ? request.NextRowKey : null));
            if (status != null)
                query = query.Where(r => r.StatusKey, StorageFilterOperator.Equal, status);
            if (activeOnly)
                query = query.Where(r => r.VisibleAsActive, StorageFilterOperator.Equal, true);

            var page = await _store.QueryAsync(query).ConfigureAwait(false);
            return ListResponse<Invitation>.Create(
                page.Items.Where(r => r.Invitation != null).Select(r => r.Invitation).ToList(),
                request, page.HasMoreRecords, page.HasMoreRecords ? scope : String.Empty,
                page.ContinuationToken ?? String.Empty);
        }

        private async Task EnsureOrgImportedAsync(string orgId)
        {
            if (ImportedOrganizations.ContainsKey(orgId)) return;

            var request = new ListRequest { PageIndex = 1, PageSize = 500 };
            for (var pageIndex = 0; pageIndex < 1000; pageIndex++)
            {
                var page = await _legacy.GetOrgPageAsync(orgId, request).ConfigureAwait(false);
                foreach (var invitation in page.Model)
                    await ImportIfMissingAsync(invitation).ConfigureAwait(false);

                if (!page.HasMoreRecords) break;
                if (String.IsNullOrWhiteSpace(page.NextPartitionKey) ||
                    String.IsNullOrWhiteSpace(page.NextRowKey))
                    throw new InvalidOperationException("Legacy invitation cursor was incomplete.");

                request = new ListRequest
                {
                    PageIndex = request.PageIndex + 1,
                    PageSize = 500,
                    NextPartitionKey = page.NextPartitionKey,
                    NextRowKey = page.NextRowKey
                };
                if (pageIndex == 999)
                    throw new InvalidOperationException("Legacy invitation import exceeded its safety page limit.");
            }
            ImportedOrganizations.TryAdd(orgId, 1);
        }

        private async Task<Invitation> ImportIfMissingAsync(Invitation invitation)
        {
            if (invitation == null) return null;
            var record = InvitationRecord.FromModel(invitation, RequireOrg());
            var key = new StorageKey(record.Id.Value, RequireOrg().Id);
            var existing = await _store.GetAsync<InvitationRecord>(key).ConfigureAwait(false);
            if (existing != null) return existing.Invitation;
            try { await _store.InsertAsync(record).ConfigureAwait(false); }
            catch
            {
                // A competing reader may have imported the same legacy invitation.
                var winner = await _store.GetAsync<InvitationRecord>(key).ConfigureAwait(false);
                if (winner != null) return winner.Invitation;
                throw;
            }
            return invitation;
        }

        private StorageQuery<InvitationRecord> ForOrganization(string orgId)
            => new StorageQuery<InvitationRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal, RequireOrg().Id)
                .Where(r => r.OrgId, StorageFilterOperator.Equal, orgId);

        private EntityHeader RequireOrg()
        {
            var org = _config.SystemOwnerOrg;
            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for invitation storage.");
            return org;
        }
    }
}
