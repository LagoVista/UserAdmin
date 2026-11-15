using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.ApplicationData.Associations
{
    /// <summary>
    /// Mongo-only team membership store. Both directions are indexed queries;
    /// memberships are durable and deleted explicitly.
    /// </summary>
    public sealed class TeamUserRepo : ITeamUserRepo
    {
        private readonly IApplicationDataStore _store;
        private readonly IAppConfig _config;

        public TeamUserRepo(IApplicationDataStore store, IAppConfig config)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public Task AddTeamMemberAsync(TeamUser newMember)
            => _store.InsertAsync(TeamMembershipRecord.FromModel(newMember,
                AssociationRecordIdentity.RequirePlatformOrg(_config)));

        public async Task<IEnumerable<TeamSummary>> GetMembersTeamsAsync(string userId)
        {
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentException("UserId is required.", nameof(userId));
            var query = InPlatformOrg()
                .Where(record => record.UserId, StorageFilterOperator.Equal, userId)
                .OrderBy(record => record.TeamId);
            var matches = await ReadAllAsync(query).ConfigureAwait(false);
            return matches.Select(record => new TeamSummary
            {
                Id = record.TeamId,
                Name = record.Member?.TeamName
            }).ToList();
        }

        public async Task<IEnumerable<TeamUserSummary>> GetTeamMembersAsync(string teamId)
        {
            if (String.IsNullOrWhiteSpace(teamId)) throw new ArgumentException("TeamId is required.", nameof(teamId));
            var query = InPlatformOrg()
                .Where(record => record.TeamId, StorageFilterOperator.Equal, teamId)
                .OrderBy(record => record.UserId);
            var matches = await ReadAllAsync(query).ConfigureAwait(false);
            return matches.Where(record => record.Member != null)
                .Select(record => record.Member.CreateSummary()).ToList();
        }

        public Task RemoveMemberAsync(string teamId, string memberId)
            => _store.DeleteAsync<TeamMembershipRecord>(new StorageKey(
                AssociationRecordIdentity.Create("team-user", teamId, memberId).Value,
                AssociationRecordIdentity.RequirePlatformOrg(_config).Id));

        private StorageQuery<TeamMembershipRecord> InPlatformOrg()
            => new StorageQuery<TeamMembershipRecord>()
                .Where(r => r.Organization.Id, StorageFilterOperator.Equal,
                    AssociationRecordIdentity.RequirePlatformOrg(_config).Id);

        private async Task<List<TeamMembershipRecord>> ReadAllAsync(StorageQuery<TeamMembershipRecord> query)
        {
            var result = new List<TeamMembershipRecord>();
            string next = null;
            do
            {
                var page = await _store.QueryAsync(
                    query.WithPage(new StoragePageRequest(500, next))).ConfigureAwait(false);
                result.AddRange(page.Items);
                next = page.ContinuationToken;
            } while (!String.IsNullOrWhiteSpace(next));
            return result;
        }
    }
}
