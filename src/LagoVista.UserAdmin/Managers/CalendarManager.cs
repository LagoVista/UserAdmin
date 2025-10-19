// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9b149e61d3ff458bd39dedc005d53a285a56e4ffa06446e57a7104e48f1c5670
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Calendar;
using System;
using System.Threading.Tasks;
using static LagoVista.Core.Models.AuthorizeResult;

namespace LagoVista.UserAdmin.Managers
{
    internal class CalendarManager : ManagerBase, ICalendarManager
    {
        private readonly ICalendarRepo _calendarRepo;

        public CalendarManager(ICalendarRepo calendarRepo, ILogger logger, IAppConfig appConfig, IDependencyManager dependencyManager, ISecurity security) : base(logger, appConfig, dependencyManager, security)
        {
            _calendarRepo = calendarRepo ?? throw new ArgumentNullException(nameof(calendarRepo));
        }

        public async Task<InvokeResult<CalendarEvent>> AddCalendarEventAsync(CalendarEvent calendarEvent, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(calendarEvent, AuthorizeActions.Create, user, org);
            ValidationCheck(calendarEvent, Actions.Create);

            await _calendarRepo.AddCalendarEventAsync(calendarEvent);

            return InvokeResult<CalendarEvent>.Create(calendarEvent);
        }

        public async Task<InvokeResult> DeleteCalendarEventAsync(string id, EntityHeader org, EntityHeader user)
        {
            var module = await _calendarRepo.GetCalendarEventAsync(id);
            await AuthorizeAsync(module, AuthorizeActions.Delete, user, org);
            await ConfirmNoDepenenciesAsync(module);
            await _calendarRepo.DeleteCalendarEventAsync(id);
            return InvokeResult.Success;
        }

        public async Task<CalendarEvent> GetCalendarEventAsync(string id, EntityHeader org, EntityHeader user)
        {
            var calendarEvent = await _calendarRepo.GetCalendarEventAsync(id);
            await AuthorizeAsync(calendarEvent, AuthorizeActions.Read, user, org);
            return calendarEvent;
        }

        public async Task<ListResponse<CalendarEventSummary>> GetEventsForDayAsync(int year, int month, int day, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(CalendarEvent), Actions.Read);
            return await _calendarRepo.GetEventsForDayAsync(year, month, day, org.Id);
        }

        public async Task<ListResponse<CalendarEventSummary>> GetEventsForMonthAsync(int year, int month, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(CalendarEvent), Actions.Read);
            return await _calendarRepo.GetEventsForMonthAsync(year, month, org.Id);
        }

        public async Task<ListResponse<CalendarEventSummary>> GetEventsForWeekAsync(int year, int month, int day, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org, typeof(CalendarEvent), Actions.Read);
            return await _calendarRepo.GetEventsForWeekAsync(year, month, day, org.Id);
        }

        public async Task<InvokeResult<CalendarEvent>> UpdateCalendarEventAsync(CalendarEvent calendarEvent, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(calendarEvent, AuthorizeActions.Update, user, org);
            ValidationCheck(calendarEvent, Actions.Update);

            await _calendarRepo.UpdateCalendarEventAsync(calendarEvent);

            return InvokeResult<CalendarEvent>.Create(calendarEvent);
        }
    }
}
