// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fa77bdf2427ffe355976c370eb6f690565caa1bf82c15504232e33623842d149
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IScheduledDowntimeRepo
    {
        Task AddScheduledDowntimeAsync(ScheduledDowntime scheduledDowntime);
        Task UpdateScheudledDowntimeAsync(ScheduledDowntime scheduledDowntime);
        Task DeleteScheduledDowntimeAsync(string id);

        Task<ScheduledDowntime> GetScheduledDowntimeAsync(String id);

        Task<ListResponse<ScheduledDowntimeSummary>> GetScheduledDowntimesAsync(string orgId, ListRequest listRequest);

        Task<bool> QueryKeyInUseAsync(string key, string orgId);
    }
}
