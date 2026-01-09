// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 47cc8a19b5c9d51274b278dcc6269ae77c1aed6db1554ab0cc06c266ba06de34
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using System;
using System.Threading.Tasks;
using System.Linq;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.IoT.Logging.Loggers;
using Microsoft.Azure.Cosmos;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Interfaces;
using LagoVista.CloudStorage.Interfaces;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class OrgLocationRepo : DocumentDBRepoBase<OrgLocation>, IOrgLocationRepo
    {
        bool _shouldConsolidateCollections;

        public OrgLocationRepo(IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public Task AddLocationAsync(OrgLocation orgLocation)
        {
            return CreateDocumentAsync(orgLocation);
        }

        public Task UpdateLocationAsync(OrgLocation org)
        {
            return UpsertDocumentAsync(org);
        }

        public Task<OrgLocation> GetLocationAsync(String id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<bool> QueryNamespaceInUseAsync(string orgId, string namespaceText)
        {
            try
            {
                var organization = (await QueryAsync(loc => loc.Namespace == namespaceText && loc.Organization.Id == orgId));
                return organization.ToList().Any();
            }
            catch(CosmosException)
            {
                /* If the collection doesn't exist, it will throw this exception */
                return false;
            }
        }

        public Task<ListResponse<OrgLocationSummary>> GetOrganizationLocationAsync(string orgId, ListRequest listRequest)
        {
            return QuerySummaryAsync<OrgLocationSummary, OrgLocation>(ol => ol.Organization.Id == orgId, ol => ol.Name, listRequest);
        }

        public Task<ListResponse<OrgLocationSummary>> GetOrganizationLocationsForCustomerAsync(string orgId, string customerId, ListRequest listRequest)
        {
            return QuerySummaryAsync<OrgLocationSummary, OrgLocation>(ol => ol.Organization.Id == orgId && ol.Customer.Id == customerId, ol => ol.Name, listRequest);
        }

        public Task DeleteOrgLocationAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }
    }
}
