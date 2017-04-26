using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System.Linq;
using System.Collections.Generic;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class TeamAccountRepo : TableStorageBase<TeamAccount>, ITeamAccountRepo
    {
        public TeamAccountRepo(IUserAdminSettings settings, ILogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task AddTeamMemberAsync(TeamAccount newMember)
        {
            return InsertAsync(newMember);
        }

        public async Task<IEnumerable<TeamSummary>> GetMembersTeamsAsync(string accountId)
        {
            var teams = await  GetByFilterAsync(FilterOptions.Create(nameof(TeamAccount.AccountId), FilterOptions.Operators.Equals, accountId));
            return from team in teams
                   select new TeamSummary(){Id = team.TeamId,Name = team.TeamName};
        }

        public async Task<IEnumerable<TeamAccountSummary>> GetTeamMembersAsync(string teamId)
        {
            var members = await GetByParitionIdAsync(teamId);
            return from member in members
                   select member.CreateSummary();
        }

        public async Task RemoveMemberAsync(string teamId, string memberId)
        {
            var rowKey = TeamAccount.CreateRowKey(teamId, memberId);
            var locationAccount = await GetAsync(rowKey);
            await RemoveAsync(locationAccount);
        }
    }
}
