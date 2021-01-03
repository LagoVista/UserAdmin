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
using System.Collections.Generic;
using System.Text;
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

        public async Task<ListResponse<ScheduledDowntimeSummary>> GetScheduledDowntimesForOrgAsync(string orgId, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, orgId, typeof(ScheduledDowntime));
            return await _repo.GetScheduledDowntimesAsync(orgId, listRequest);

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
