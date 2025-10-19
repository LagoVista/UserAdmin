// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ee971ef2687409f1b51ac8f8546a284743a9048c949d4ec4521fabe5c15bb9ae
// IndexVersion: 0
// --- END CODE INDEX META ---
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
