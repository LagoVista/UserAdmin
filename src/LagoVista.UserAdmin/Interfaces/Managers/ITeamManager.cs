// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 930086f54e23834347ae2f762189fdab870d7f7f5ca86238224f5f81495ad440
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ITeamManager
    {
        Task<InvokeResult> AddTeamAsync(Team team, EntityHeader org, EntityHeader user);

        Task<InvokeResult> UpdateTeamAsync(Team team, EntityHeader org, EntityHeader user);

        Task<InvokeResult> DeleteTeamAsync(String teamId, EntityHeader org, EntityHeader user);

        Task<IEnumerable<TeamSummary>> GetTeamsForOrgAsync(String orgId, EntityHeader org, EntityHeader user);

        Task<IEnumerable<TeamUserSummary>> GetMembersForTeamAsync(String teamId, EntityHeader org, EntityHeader user);

        Task<Team> GetTeamAsync(String id, EntityHeader org, EntityHeader user);

        Task<InvokeResult> AddTeamMemberAsync(string teamId, string userId, EntityHeader org, EntityHeader addedByMemberId);

        Task<InvokeResult> RemoveTeamMemberAsync(string teamId, string userId, EntityHeader org, EntityHeader addedByMemberId);

        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org);

        Task<DependentObjectCheckResult> CheckTeamInUseAsync(String teamId, EntityHeader org, EntityHeader user);
    }
}
