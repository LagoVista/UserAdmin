// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1f2b77f8fead09f7850878b617fe625e3b4bc958f279aac00813c7f484c9af76
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Interfaces.AutoMapper;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Repos;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Models;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class OrganizationLoaderRepo : DocumentDBRepoBase<Organization>, IOrganizationLoaderRepo
    {
        private readonly bool _shouldConsolidateCollections;

        public OrganizationLoaderRepo(IUserAdminSettings settings, IAdminLogger logger, ICacheProvider cacheProvider) : 
            base(settings.UserStorage.Uri, settings.UserStorage.AccessKey, settings.UserStorage.ResourceName, logger, cacheProvider)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task<Organization> GetOrganizationAsync(string id)
        {
            return GetDocumentAsync(id);
        }
    }

    public class OrganizationRepo : DocumentDBRepoBase<Organization>, IOrganizationRepo
    {
        private readonly bool _shouldConsolidateCollections;
        private readonly ICacheProvider _cacheProvider;
        private readonly IOrganizationRelationalRepo _relationalRepo;
        private readonly ILagoVistaAutoMapper _autoMapper;
        private readonly ISystemUsers _systemUsers;
        private readonly IAppUserRelationalRepo _appUserRelationalRepo;

        public OrganizationRepo(IUserAdminSettings userAdminSettings, IOrganizationRelationalRepo relationalRepo, IAppUserRelationalRepo appUserRelationalRepo, IDocumentCloudCachedServices services, ISystemUsers systemUsers, ILagoVistaAutoMapper autoMapper) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
            _shouldConsolidateCollections = userAdminSettings.ShouldConsolidateCollections;
            _relationalRepo = relationalRepo ?? throw new ArgumentNullException(nameof(relationalRepo));
            _cacheProvider = services.CacheProvider;
            _autoMapper = autoMapper ?? throw new ArgumentNullException(nameof(autoMapper));
            _systemUsers = systemUsers ?? throw new ArgumentNullException(nameof(systemUsers));
            _appUserRelationalRepo = appUserRelationalRepo ?? throw new ArgumentNullException(nameof(appUserRelationalRepo));
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
            var dto = await _autoMapper.CreateAsync<Organization, OrganizationDTO>(org, _systemUsers.SystemOrg, _systemUsers.HostUser);
            await _relationalRepo.AddOrganizationAsync(dto, _systemUsers.SystemOrg, _systemUsers.HostUser);
            await CreateDocumentAsync(org);
        }

        public async Task DeleteOrgAsync(string orgId)
        {
            await _relationalRepo.DeleteOrganizationAsync(orgId, _systemUsers.SystemOrg, _systemUsers.HostUser);
            await DeleteDocumentAsync(orgId);
        }

        public async Task<ListResponse<OrganizationSummary>> GetAllOrgsAsync(ListRequest listRequest)
        {
            return await base.QuerySummaryAsync<OrganizationSummary, Organization>(qry => true, org => org.Name, listRequest);
        }

        public Task<List<EntityHeader>> GetBillingContactOrgsForUserAsync(string orgId, string userId)
        {
            return _appUserRelationalRepo.GetBillingContactOrgsForUserAsync(userId, _systemUsers.SystemOrg, _systemUsers.HostUser);
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
                org.Key = org.Key.Value.ToLower();
            }
            return org;
        }

        public Task<bool> HasBillingRecords(string orgId)
        {
            return _relationalRepo.DoesOrgHaveBillingRecords(orgId, _systemUsers.SystemOrg, _systemUsers.HostUser);
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
            var dto = await _autoMapper.CreateAsync<Organization, OrganizationDTO>(org, _systemUsers.SystemOrg, _systemUsers.HostUser);
            await _relationalRepo.UpdateOrganizationAsync(dto, org.ToEntityHeader(), _systemUsers.HostUser);
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