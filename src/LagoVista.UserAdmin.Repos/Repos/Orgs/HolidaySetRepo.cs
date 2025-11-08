// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6111a8c04949016a005453799782b21d382f1ac26444098c847ac798409d3566
// IndexVersion: 2
// --- END CODE INDEX META ---
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

        public async Task<ListResponse<HolidaySetSummary>> GetAllHolidaySetsAsync(string orgId, ListRequest listRequest)
        {
            return await QuerySummaryAsync<HolidaySetSummary, HolidaySet>(qry => qry.IsPublic || qry.OwnerOrganization.Id == orgId, qry => qry.Name, listRequest);
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
