using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ITeamUserRepo
    {
        Task AddTeamMemberAsync(TeamUser newMember);
        Task RemoveMemberAsync(String teamId, string memberId);

        Task<IEnumerable<TeamUserSummary>> GetTeamMembersAsync(String teamId);

        Task<IEnumerable<TeamSummary>> GetMembersTeamsAsync(String teamId);
    }
}
