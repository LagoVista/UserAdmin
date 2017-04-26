using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ITeamRepo
    {
        Task AddTeamAsync(Team team);
        Task UpdateTeamAsync(Team team);
        Task DeleteTeamAsync(string id);

        Task<Team> GetTeamAsync(String id);

        Task<IEnumerable<TeamSummary>> GetTeamsForOrgAsync(string orgId);

        Task<bool> QueryKeyInUseAsync(string key, string orgId);
    }
}