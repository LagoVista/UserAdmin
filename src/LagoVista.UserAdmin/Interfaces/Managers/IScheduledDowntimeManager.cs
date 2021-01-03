using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IScheduledDowntimeManager
    {
        Task<InvokeResult> AddScheduledDowntimeAsync(ScheduledDowntime scheduledDowntime, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateScheduledDowntimeAsync(ScheduledDowntime scheduledDowntime, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteScheduledDowntimeAsync(String scheduledDowntimeId, EntityHeader org, EntityHeader user);
        Task<ScheduledDowntime> GetScheduledDowntimeAsync(string scheduledDowntimeId, EntityHeader org, EntityHeader user);
        Task<ListResponse<ScheduledDowntimeSummary>> GetScheduledDowntimesForOrgAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org);
        Task<DependentObjectCheckResult> CheckScheduledDowntimeInUseAsync(String scheduledDowntimeTeamId, EntityHeader org, EntityHeader user);
    }
}
