using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class BusinessUnitRepo : DocumentDBRepoBase<BusinessUnit>, IBusinessUnitRepo
    {
        public BusinessUnitRepo(IDocumentCloudCachedServices services) : base(services)
        {
        }

        public Task AddBusinessUnitAsync(BusinessUnit businessUnit) => CreateDocumentAsync(businessUnit);

        public Task UpdateBusinessUnitAsync(BusinessUnit businessUnit) => UpsertDocumentAsync(businessUnit);

        public Task<BusinessUnit> GetBusinessUnitAsync(string id) => GetDocumentAsync(id);

        public Task DeleteBusinessUnitAsync(string id) => DeleteDocumentAsync(id);

        public Task<ListResponse<BusinessUnitSummary>> GetBusinessUnitsForOrgAsync(string orgId, ListRequest listRequest)
        {
            return QuerySummaryAsync<BusinessUnitSummary, BusinessUnit>(
                unit => unit.OwnerOrganization.Id == orgId, unit => unit.Name, listRequest);
        }

        public async Task<bool> QueryKeyInUseAsync(string key, string orgId, string excludeId = null)
        {
            var matching = await QueryAsync(unit => unit.OwnerOrganization.Id == orgId && unit.Key == key);
            return matching.Any(unit => unit.Id != excludeId);
        }
    }
}
