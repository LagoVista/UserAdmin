using LagoVista.CloudStorage;
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Calendar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LagoVista.Core;
using LagoVista.Core.Interfaces;

namespace LagoVista.UserAdmin.Repos.Repos.Calendar
{
    public class CalendarRepo : DocumentDBRepoBase<CalendarEvent>, ICalendarRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public CalendarRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyManager) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider, dependencyManager)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddCalendarEventAsync(CalendarEvent calendarEvent)
        {
            return CreateDocumentAsync(calendarEvent);
            throw new NotImplementedException();
        }

        public Task DeleteCalendarEventAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<CalendarEvent> GetCalendarEventAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public  async Task<ListResponse<CalendarEventSummary>> GetEventsForDayAsync(int year, int month, int day, string orgId)
        {
            var date = new DateTime(year, month, day);

            var items = await QueryAsync(rec => rec.OwnerOrganization.Id == orgId && rec.Date == date.ToDateOnly());
            return ListResponse<CalendarEventSummary>.Create(items.Select(evt => evt.CreateSummary()));
        }

        public async Task<ListResponse<CalendarEventSummary>> GetEventsForMonthAsync(int year, int month, string orgId)
        {
            var items = await QueryAsync(rec => rec.OwnerOrganization.Id == orgId && rec.Date.StartsWith($"{year}/{month:00}"));
            return ListResponse<CalendarEventSummary>.Create(items.Select(evt => evt.CreateSummary()));
        }

        public async Task<ListResponse<CalendarEventSummary>> GetEventsForWeekAsync(int year, int month, int day, string orgId)
        {
            var date = new DateTime(year, month, day);
            var weekDates = new List<string>();
            for(var idx = 0; idx < 7; ++idx)
            {
                weekDates.Add(date.ToDateOnly());
                date = date.AddDays(1);
            }

            var items = await QueryAsync(rec => rec.OwnerOrganization.Id == orgId && weekDates.Contains(rec.Date));
            return ListResponse<CalendarEventSummary>.Create(items.Select(evt => evt.CreateSummary()));
        }

        public Task UpdateCalendarEventAsync(CalendarEvent calendarEvent)
        {
            return UpsertDocumentAsync(calendarEvent);
        }
    }
}
