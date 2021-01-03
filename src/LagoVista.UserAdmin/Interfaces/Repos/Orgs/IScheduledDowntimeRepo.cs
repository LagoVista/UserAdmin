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
