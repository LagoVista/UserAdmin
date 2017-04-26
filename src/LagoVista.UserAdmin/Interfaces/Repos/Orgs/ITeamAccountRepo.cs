using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ITeamAccountRepo
    {
        Task AddTeamMemberAsync(TeamAccount newMember);
        Task RemoveMemberAsync(String teamId, string memberId);

        Task<IEnumerable<TeamAccountSummary>> GetTeamMembersAsync(String teamId);

        Task<IEnumerable<TeamSummary>> GetMembersTeamsAsync(String teamId);
    }
}
