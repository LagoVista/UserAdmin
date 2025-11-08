// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: dadfa16bd7370b948bac5d4f6b91087d7a183ee777644f4af584aea5d9b3a398
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using System.Linq;
using System.Collections.Generic;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Users
{
    public class TeamUserRepo : TableStorageBase<TeamUser>, ITeamUserRepo
    {
        public TeamUserRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task AddTeamMemberAsync(TeamUser newMember)
        {
            return InsertAsync(newMember);
        }

        public async Task<IEnumerable<TeamSummary>> GetMembersTeamsAsync(string userId)
        {
            var teams = await  GetByFilterAsync(FilterOptions.Create(nameof(TeamUser.UserId), FilterOptions.Operators.Equals, userId));
            return from team in teams
                   select new TeamSummary(){Id = team.TeamId,Name = team.TeamName};
        }

        public async Task<IEnumerable<TeamUserSummary>> GetTeamMembersAsync(string teamId)
        {
            var members = await GetByParitionIdAsync(teamId);
            return from member in members
                   select member.CreateSummary();
        }

        public async Task RemoveMemberAsync(string teamId, string memberId)
        {
            var rowKey = TeamUser.CreateRowKey(teamId, memberId);
            var teamUser = await GetAsync(rowKey);
            await RemoveAsync(teamUser);
        }
    }
}
