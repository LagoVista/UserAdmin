using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Calendar;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface ICalendarRepo
    {
        Task AddCalendarEventAsync(CalendarEvent role);
        Task<CalendarEvent> GetCalendarEventAsync(string id);
        Task UpdateCalendarEventAsync(CalendarEvent role);
        Task DeleteCalendarEventAsync(string id);


        Task<ListResponse<CalendarEventSummary>> GetEventsForMonthAsync(int year, int month, string orgId);
        Task<ListResponse<CalendarEventSummary>> GetEventsForWeekAsync(int year, int month, int day, string orgId);
        Task<ListResponse<CalendarEventSummary>> GetEventsForDayAsync(int year, int month, int day, string orgId);
    }
}
