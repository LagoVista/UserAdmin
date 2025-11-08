// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ce3ba1b95bf8fc73931858fc7cefffed38e36b351115055644a035345dd01c32
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Calendar;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ICalendarManager
    {
        Task<InvokeResult<CalendarEvent>> AddCalendarEventAsync(CalendarEvent calendarEvent, EntityHeader org, EntityHeader user);
        Task<CalendarEvent> GetCalendarEventAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult<CalendarEvent>> UpdateCalendarEventAsync(CalendarEvent calendarEvent, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteCalendarEventAsync(string id, EntityHeader org, EntityHeader user);


        Task<ListResponse<CalendarEventSummary>> GetEventsForMonthAsync(int year, int month, EntityHeader org, EntityHeader user);
        Task<ListResponse<CalendarEventSummary>> GetEventsForWeekAsync(int year, int month, int day, EntityHeader org, EntityHeader user);
        Task<ListResponse<CalendarEventSummary>> GetEventsForDayAsync(int year, int month, int day, EntityHeader org, EntityHeader user);
    }
}
