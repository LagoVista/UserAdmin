using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Orgs
{
    public class HolidaySetRepo : DocumentDBRepoBase<HolidaySet>, IHolidaySetRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public HolidaySetRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public Task AddHolidaySetAsync(HolidaySet holidaySet)
        {
            return CreateDocumentAsync(holidaySet);
        }

        public Task DeleteHolidaySetAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public async Task<ListResponse<HolidaySetSummary>> GetAllHolidaySetsAsync(ListRequest listRequest)
        {
            var lists = await QueryAsync(rec => rec.IsPublic, listRequest);

            return new ListResponse<HolidaySetSummary>()
            {
                Model = lists.Model.Select(dls => dls.CreateSummary()),
                NextPartitionKey = lists.NextPartitionKey,
                NextRowKey = lists.NextRowKey,
                PageCount = lists.PageCount,
                PageIndex = lists.PageIndex,
                HasMoreRecords = lists.HasMoreRecords,
                PageSize = lists.PageSize,
                ResultId = lists.ResultId
            };
        }

        public Task<HolidaySet> GetHolidaySetAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<bool> QueryKeyInUseAsync(string key, string orgId)
        {
            return (await base.QueryAsync(attr => attr.Key == key)).Any();
        }

        public Task UpdateHolidaySetAsync(HolidaySet holidaySet)
        {
            return UpsertDocumentAsync(holidaySet);
        }
    }
}
