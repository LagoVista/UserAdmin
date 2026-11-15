using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    /// <summary>
    /// Read-only bridge for invitations issued before Mongo cutover. New invitations
    /// and all state changes write only to Mongo. Remove after cutover reconciliation.
    /// </summary>
    public sealed class LegacyInvitationReader : TableStorageBase<Invitation>
    {
        public LegacyInvitationReader(IUserAdminSettings settings, IAdminLogger logger)
            : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger) { }

        public Task<Invitation> GetLegacyAsync(string invitationId)
            => GetAsync(invitationId, false);

        public async Task<Invitation> FindByOrgEmailAsync(string orgId, string email)
        {
            return (await GetByFilterAsync(
                FilterOptions.Create(nameof(Invitation.Email), FilterOptions.Operators.Equals, email),
                FilterOptions.Create(nameof(Invitation.OrganizationId), FilterOptions.Operators.Equals, orgId)))
                .FirstOrDefault();
        }

        public Task<ListResponse<Invitation>> GetOrgPageAsync(string orgId, ListRequest request)
            => GetPagedResultsAsync(orgId, request);
    }
}
