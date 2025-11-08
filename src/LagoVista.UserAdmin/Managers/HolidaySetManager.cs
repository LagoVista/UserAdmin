// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8344a12e0e3b1f53b346fc12340583144285bd936720fb7f912db91080b62168
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Exceptions;
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
    public class HolidaySetManager : ManagerBase, IHolidaySetManager
    {
        private readonly IHolidaySetRepo _repo;
        private readonly IScheduledDowntimeManager _scheduledDownTimeManager;
        private readonly IScheduledDowntimeRepo _scheduledDowntimeRepo;
        private readonly IAppUserManager _userManager;

        public HolidaySetManager(IHolidaySetRepo holidaySetRepo, IScheduledDowntimeManager scheduledDowntimeManager, IAppUserManager userManager,  IScheduledDowntimeRepo scheduledDowntimeRepo,
            IDependencyManager depManager, ISecurity security, IAdminLogger logger, IAppConfig appConfig) :
            base(logger, appConfig, depManager, security)
        {
            _repo = holidaySetRepo ?? throw new ArgumentNullException(nameof(holidaySetRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _scheduledDownTimeManager = scheduledDowntimeManager ?? throw new ArgumentNullException(nameof(scheduledDowntimeManager));
            _scheduledDowntimeRepo = scheduledDowntimeRepo ?? throw new ArgumentNullException(nameof(scheduledDowntimeRepo));
        }

        public async Task<InvokeResult> AddHolidaySetAsync(HolidaySet holidaySet, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(holidaySet, AuthorizeActions.Create, user, org);
            ValidationCheck(holidaySet, Actions.Create);
            await _repo.AddHolidaySetAsync(holidaySet);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckHolidaySetInUseAsync(string holidaySetId, EntityHeader org, EntityHeader user)
        {
            var record = await _repo.GetHolidaySetAsync(holidaySetId);
            await AuthorizeAsync(record, AuthorizeResult.AuthorizeActions.Read, user, org);
            return await CheckForDepenenciesAsync(record);
        }

        public async Task<InvokeResult> CopyToOrgAsync(string holidaySetId, string destOrgId, EntityHeader org, EntityHeader user)
        {
            var timeStamp = DateTime.UtcNow.ToJSONString();

            if(destOrgId != org.Id)
            {
                var currentUser = await _userManager.GetUserByIdAsync(user.Id, org, user);
                if(!currentUser.IsSystemAdmin)
                {
                    throw new NotAuthorizedException("To copy holiday sets to an org, must belong to that org, or be a system admin.");
                }
            }

            var holidaySet = await _repo.GetHolidaySetAsync(holidaySetId);
            foreach(var downTime in holidaySet.Holidays)
            {
                if(!await _scheduledDowntimeRepo.QueryKeyInUseAsync(downTime.Key, org.Id))
                {
                    downTime.OriginalId = downTime.Id;
                    downTime.Id = Guid.NewGuid().ToId();
                    downTime.CreationDate = timeStamp;
                    downTime.LastUpdatedDate = timeStamp;
                    downTime.LastUpdatedBy = user;
                    downTime.CreatedBy = user;
                    downTime.IsPublic = false;
                    downTime.OwnerOrganization = org;
                   
                    await _scheduledDownTimeManager.AddScheduledDowntimeAsync(downTime, org, user);
                }
            }

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteHolidaySetAsync(string holidaySetId, EntityHeader org, EntityHeader user)
        {
            var team = await _repo.GetHolidaySetAsync(holidaySetId);
            await AuthorizeAsync(team, AuthorizeActions.Delete, user, org);
            await ConfirmNoDepenenciesAsync(team);
            await _repo.DeleteHolidaySetAsync(holidaySetId);
            return InvokeResult.Success;
        }

        public Task<ListResponse<HolidaySetSummary>> GetAllHolidaySets(EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            return _repo.GetAllHolidaySetsAsync(org.Id, listRequest);
        }

        public async Task<HolidaySet> GetHolidaySetAsync(string holidaySetId, EntityHeader org, EntityHeader user)
        {
            var holidaySet = await _repo.GetHolidaySetAsync(holidaySetId);
            await AuthorizeAsync(holidaySet, AuthorizeActions.Read, user, org);
            return holidaySet;
        }

        public Task<bool> QueryKeyInUseAsync(string key, EntityHeader org)
        {
            return _repo.QueryKeyInUseAsync(key, org.Id);
        }

        public async Task<InvokeResult> UpdateHolidaySetAsync(HolidaySet holidaySet, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(holidaySet, AuthorizeActions.Create, user, org);
            ValidationCheck(holidaySet, Actions.Create);
            await _repo.UpdateHolidaySetAsync(holidaySet);

            return InvokeResult.Success;
        }
    }
}
