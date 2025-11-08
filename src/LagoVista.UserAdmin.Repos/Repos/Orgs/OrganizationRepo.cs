// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1f2b77f8fead09f7850878b617fe625e3b4bc958f279aac00813c7f484c9af76
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using Microsoft.Azure.Cosmos;
using LagoVista.CloudStorage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Exceptions;
using Newtonsoft.Json;
using MongoDB.Bson.Serialization.IdGenerators;


namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class OrganizationRepo : DocumentDBRepoBase<Organization>, IOrganizationRepo
    {
        private readonly bool _shouldConsolidateCollections;
        private readonly IRDBMSManager _rdbmsUserManager;
        private readonly ICacheProvider _cacheProvider;

        public OrganizationRepo(IRDBMSManager rdbmsUserManager, IUserAdminSettings userAdminSettings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            _rdbmsUserManager = rdbmsUserManager ?? throw new ArgumentNullException(nameof(rdbmsUserManager));
            _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
        }

        private string GetCacheKey(String orgId)
        {
            return $"OrgForLandingPage_{orgId}";
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

        public async Task<ListResponse<OrganizationSummary>> GetAllOrgsAsync(ListRequest listRequest)
        {
            return await base.QuerySummaryAsync<OrganizationSummary, Organization>(qry => true, org => org.Name, listRequest);
        }

        public Task<List<EntityHeader>> GetBillingContactOrgsForUserAsync(string orgId, string userId)
        {
            return _rdbmsUserManager.GetBillingContactOrgsForUserAsync(orgId, userId);
        }

        public async Task<string> GetHomePageForOrgAsync(string orgId)
        {
            var landingPage = await _cacheProvider.GetAsync(GetCacheKey(orgId));
            if(String.IsNullOrEmpty(landingPage))
            {
                var org = await GetOrganizationAsync(orgId);
                landingPage = (String.IsNullOrEmpty(org.HomePage)) ? "/home" : org.HomePage;
                await _cacheProvider.AddAsync(GetCacheKey(orgId), landingPage);
            }

            return landingPage;
        }

        public const string ORGS_WITH_NAMESPACE = "orgs_with_hostnames";

        public async Task<EntityHeader> GetDefaultIndustryForOrgAsync(string orgId)
        {
            var org = await GetOrganizationAsync(orgId);
            if(EntityHeader.IsNullOrEmpty(org.DefaultIndustry))
            {
                throw new RecordNotFoundException("Organization.DefaultIndustry", orgId);
            }

            return org.DefaultIndustry;
        }

        public async Task<OrgHostNameRedirect> GetDefaultLandingPageForHostAsync(string hostName)
        {
            var json = await _cacheProvider.GetAsync(ORGS_WITH_NAMESPACE);
            if(!String.IsNullOrEmpty(json))
            {
                var orgHostNames = JsonConvert.DeserializeObject<List<OrgHostNameRedirect>>(json);
                return orgHostNames.FirstOrDefault(o => o.HostName == hostName);   
            }

            var orgs = await QueryAsync(o => null != o.LandingPageHostName);
            var os = orgs.Select(o => new OrgHostNameRedirect() { HostName = o.LandingPageHostName, OrgNs = o.Namespace, LandingPage = o.DefaultLandingPage });
            json = JsonConvert.SerializeObject(os);
            await _cacheProvider.AddAsync(ORGS_WITH_NAMESPACE, json);
            return os.FirstOrDefault(o => o.HostName == hostName);
        }

        public async Task<Organization> GetOrganizationAsync(string id)
        {
            var org = await GetDocumentAsync(id);
            if(!String.IsNullOrEmpty(org.Key))
            {
                org.Key = org.Key.ToLower();
            }
            return org;
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
            catch (CosmosException)
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
            await _cacheProvider.RemoveAsync(GetCacheKey(org.Id));
            await _cacheProvider.RemoveAsync(ORGS_WITH_NAMESPACE);
        }

        public Task<ListResponse<OrganizationSummary>> GetAllOrgsAsync(string orgSearch, ListRequest listRequest)
        {
            return QuerySummaryAsync<OrganizationSummary, Organization>(org=>org.Name.ToLower().Contains(orgSearch.ToLower()), org => org.Name, listRequest);
        }

        public async Task<string> GetOrganizationIdForNamespaceAsync(string orgNameSpace)
        {
            var cacheKey = $"org_id_for_namespace_{orgNameSpace}";

            var orgId = await _cacheProvider.GetAsync(cacheKey);
            if(!String.IsNullOrEmpty(orgId))
            {
                return orgId;
            }

            var organization = (await QueryAsync(org => org.Namespace == orgNameSpace)).ToList();
            if (organization.Any())
            {
                orgId = organization.First().Id;
                await _cacheProvider.AddAsync(cacheKey, orgId);
                return orgId;
            }
            else
            {
                throw new RecordNotFoundException("Org Namespace Does Not Exist", orgNameSpace);
            }
        }

        public async Task<Organization> GetOrganizationFromNamespaceAsync(string orgNameSpace)
        {
            var cacheKey = $"org_id_for_namespace_{orgNameSpace}";

            var orgId = await _cacheProvider.GetAsync(cacheKey);
            if (!String.IsNullOrEmpty(orgId))
            {
                return await GetOrganizationAsync(orgId);
            }

            var organization = (await QueryAsync(org => org.Namespace == orgNameSpace)).ToList();
            if (organization.Any())
            {
                orgId = organization.First().Id;
                await _cacheProvider.AddAsync(cacheKey, orgId);
                return await GetOrganizationAsync(orgId);
            }
            else
            {
                throw new RecordNotFoundException("Org Namespace Does Not Exist", orgNameSpace);
            }
        }
    }
}