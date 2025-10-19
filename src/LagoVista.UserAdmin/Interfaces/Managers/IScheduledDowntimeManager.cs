// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a83139fd81485aa49e18fce03eb0e413b6870e4c84d452d9f57d183cb3b023b6
// IndexVersion: 0
// --- END CODE INDEX META ---
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
