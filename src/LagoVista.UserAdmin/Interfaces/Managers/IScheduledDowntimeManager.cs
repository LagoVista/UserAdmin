using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IScheduledDowntimeManager
    {
        Task<InvokeResult> AddScheduledDowntimeAsync(ScheduledDowntime scheduledDowntime, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateScheduledDowntimeAsync(ScheduledDowntime scheduledDowntime, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteScheduledDowntimeAsync(String scheduledDowntimeTeamId, EntityHeader org, EntityHeader user);
        Task<ListResponse<ScheduledDowntimeSummary>> GetScheduledDowntimesForOrgAsync(String orgId, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org);
        Task<DependentObjectCheckResult> CheckScheduledDowntimeInUseAsync(String scheduledDowntimeTeamId, EntityHeader org, EntityHeader user);
    }
}
