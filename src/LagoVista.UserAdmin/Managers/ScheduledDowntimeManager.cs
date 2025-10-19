// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ed00dc2e6985bb2bb91c50f2ecd2f1f94466dcacc375b555324e6ae1024ba2fd
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;
using static LagoVista.Core.Models.AuthorizeResult;

namespace LagoVista.UserAdmin.Managers
{
    class ScheduledDowntimeManager : ManagerBase, IScheduledDowntimeManager
    {
        private readonly IScheduledDowntimeRepo _repo;

        public ScheduledDowntimeManager(IScheduledDowntimeRepo scheduledDowntimeRepo, IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig) :
            base(logger, appConfig, depManager, security)
        {
            _repo = scheduledDowntimeRepo ?? throw new ArgumentNullException(nameof(scheduledDowntimeRepo));
        }

        public async Task<InvokeResult> AddScheduledDowntimeAsync(ScheduledDowntime scheduledDowntime, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(scheduledDowntime, AuthorizeActions.Create, user, org);
            ValidationCheck(scheduledDowntime, Actions.Create);
            await _repo.AddScheduledDowntimeAsync(scheduledDowntime);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckScheduledDowntimeInUseAsync(string scheduledDowntimeTeamId, EntityHeader org, EntityHeader user)
        {
            var record = await _repo.GetScheduledDowntimeAsync(scheduledDowntimeTeamId);
            await AuthorizeAsync(record, AuthorizeResult.AuthorizeActions.Read, user, org);
            return await CheckForDepenenciesAsync(record);
        }

        public async Task<InvokeResult> DeleteScheduledDowntimeAsync(string scheduledDowntimeTeamId, EntityHeader org, EntityHeader user)
        {
            var team = await _repo.GetScheduledDowntimeAsync(scheduledDowntimeTeamId);
            await AuthorizeAsync(team, AuthorizeActions.Delete, user, org);
            await ConfirmNoDepenenciesAsync(team);
            await _repo.DeleteScheduledDowntimeAsync(scheduledDowntimeTeamId);
            return InvokeResult.Success;
        }

        public async Task<ScheduledDowntime> GetScheduledDowntimeAsync(string scheduledDowntimeId, EntityHeader org, EntityHeader user)
        {
            var scheduledDowntime = await _repo.GetScheduledDowntimeAsync(scheduledDowntimeId);
            await AuthorizeAsync(scheduledDowntime, AuthorizeActions.Read, user, org);
            return scheduledDowntime;
        }

        public async Task<ListResponse<ScheduledDowntimeSummary>> GetScheduledDowntimesForOrgAsync(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(ScheduledDowntime));
            return await _repo.GetScheduledDowntimesAsync(org.Id, listRequest);

        }

        public Task<bool> QueryKeyInUseAsync(string key, EntityHeader org)
        {
            return _repo.QueryKeyInUseAsync(key, org.Id);
        }

        public async Task<InvokeResult> UpdateScheduledDowntimeAsync(ScheduledDowntime scheduledDowntime, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(scheduledDowntime, AuthorizeActions.Create, user, org);
            ValidationCheck(scheduledDowntime, Actions.Create);
            await _repo.UpdateScheudledDowntimeAsync(scheduledDowntime);

            return InvokeResult.Success;
        }
    }
}
