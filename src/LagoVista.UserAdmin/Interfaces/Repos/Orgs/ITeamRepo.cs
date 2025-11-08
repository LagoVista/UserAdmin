// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4e9a579a903e6b72c61bf3a9854e6e9b9e735a6cffe19c6c4bc311cce7425fed
// IndexVersion: 2
// --- END CODE INDEX META ---
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