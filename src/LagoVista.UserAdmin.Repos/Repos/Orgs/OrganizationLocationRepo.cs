using LagoVista.CloudStorage.DocumentDB;

using System;
using System.Threading.Tasks;
using LagoVista.Core.PlatformSupport;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class OrganizationLocationRepo : DocumentDBRepoBase<OrganizationLocation>, IOrganizationLocationRepo
    {
        bool _shouldConsolidateCollections;

        public OrganizationLocationRepo(IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public Task AddLocationAsync(OrganizationLocation account)
        {
            return CreateDocumentAsync(account);
        }

        public Task UpdateLocationAsync(OrganizationLocation account)
        {
            return UpsertDocumentAsync(account);
        }

        public Task<OrganizationLocation> GetLocationAsync(String id)
        {
            return GetDocumentAsync(id);
        }

        public Task<IEnumerable<OrganizationLocation>> GetOrganizationLocationAsync(String organizationId)
        {
            return QueryAsync(act => act.Organization.Id == organizationId);
        }

        public async Task<bool> QueryNamespaceInUseAsync(string orgId, string namespaceText)
        {
            try
            {
                var organization = (await QueryAsync(loc => loc.Namespace == namespaceText && loc.Organization.Id == orgId));
                return organization.ToList().Any();
            }
            catch(DocumentClientException)
            {
                /* If the collection doesn't exist, it will throw this exception */
                return false;
            }
        }
    }
}
