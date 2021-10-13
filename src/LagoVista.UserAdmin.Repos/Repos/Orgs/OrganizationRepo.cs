using System.Threading.Tasks;
using LagoVista.CloudStorage.DocumentDB;
using System.Linq;
using Microsoft.Azure.Documents;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using LagoVista.Core.Models.UIMetaData;
using System.Collections.Generic;
using LagoVista.Core.Models;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class OrganizationRepo : DocumentDBRepoBase<Organization>, IOrganizationRepo
    {
        private readonly bool _shouldConsolidateCollections;
        private readonly IRDBMSManager _rdbmsUserManager;

        public OrganizationRepo(IRDBMSManager rdbmsUserManager, IUserAdminSettings userAdminSettings, IAdminLogger logger) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            _rdbmsUserManager = rdbmsUserManager ?? throw new ArgumentNullException(nameof(rdbmsUserManager));
        }

        protected override bool ShouldConsolidateCollections
        {
            get { return _shouldConsolidateCollections; }
        }

        public async Task AddOrganizationAsync(Organization org)
        {
            await _rdbmsUserManager.AddOrgAsync(org);
            await CreateDocumentAsync(org);
        }

        public async Task DeleteOrgAsync(string orgId)
        {
            await _rdbmsUserManager.DeleteOrgAsync(orgId);
            await DeleteDocumentAsync(orgId);
        }

        public async Task<ListResponse<Organization>> GetAllOrgsAsync(ListRequest listRequest)
        {
            return await base.QueryAsync(qry => true, listRequest);
        }

        public Task<List<EntityHeader>> GetBillingContactOrgsForUserAsync(string userId)
        {
            return _rdbmsUserManager.GetBillingContactOrgsForUserAsync(userId);
        }

        public Task<Organization> GetOrganizationAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<bool> HasBillingRecords(string orgId)
        {
            return _rdbmsUserManager.HasBillingRecords(orgId);
        }

        public async Task<bool> QueryNamespaceInUseAsync(string namespaceText)
        {
            try
            {
                var organization = (await QueryAsync(org => org.Namespace == namespaceText));
                var list = organization.ToList();
                return list.Any();
            }
            catch (DocumentClientException)
            {
                /* If the collection doesn't exist, it will throw this exception */
                return false;
            }
        }

        public async Task<bool> QueryOrganizationExistAsync(string id)
        {
            return (await GetOrganizationAsync(id)) != null;
        }

        public async Task UpdateOrganizationAsync(Organization org)
        {
            await _rdbmsUserManager.UpdateOrgAsync(org);
            await UpsertDocumentAsync(org);
        }
    }
}