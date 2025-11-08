// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c48cc4c8c4b5496315fcd9a7742cde638bea9c2e9acacb8f0fec3cd900eb999a
// IndexVersion: 2
// --- END CODE INDEX META ---
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
