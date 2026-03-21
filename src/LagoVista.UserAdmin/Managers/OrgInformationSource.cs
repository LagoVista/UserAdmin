using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class OrgInformationSource : IOrgInformationSource
    {
        private readonly IOrganizationRepo _organizationRepo;
        private readonly ICacheProvider _cacheProvider;
        private readonly IOrgLocationRepo _locationRepo;
        private readonly IOrgUserRepo _orgUserRepo;

        public OrgInformationSource(IOrganizationRepo organizationRepo, ICacheProvider cacheProvider, IOrgLocationRepo locationRepo, IOrgUserRepo userRepo)
        {
            _cacheProvider = cacheProvider ?? throw new ArgumentNullException(nameof(cacheProvider));
            _organizationRepo = organizationRepo ?? throw new ArgumentNullException(nameof(organizationRepo));
            _locationRepo = locationRepo ?? throw new ArgumentNullException(nameof(locationRepo ));
            _orgUserRepo = userRepo ?? throw new ArgumentNullException(nameof(userRepo));
        }


        public async Task<InvokeResult<BasicTheme>> GetBasicThemeForOrgAsync(string orgid)
        {
            var json = await _cacheProvider.GetAsync($"basic_theme_org_{orgid}");
            if (string.IsNullOrEmpty(json))
            {
                var org = await _organizationRepo.GetOrganizationAsync(orgid);
                var basicTheme = new BasicTheme()
                {
                    PrimaryTextColor = org.PrimaryTextColor,
                    PrimryBGColor = org.PrimaryBgColor,
                    AccentColor = org.AccentColor
                };

                await _cacheProvider.AddAsync($"basic_theme_org_{orgid}", JsonConvert.SerializeObject(basicTheme));
                return InvokeResult<BasicTheme>.Create(basicTheme);
            }
            else
            {
                var theme = JsonConvert.DeserializeObject<BasicTheme>(json);
                return InvokeResult<BasicTheme>.Create(theme);

            }
        }

        public async Task<ListResponse<OrgLocationSummary>> GetLocationsForOrganizationsAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return await _locationRepo.GetOrganizationLocationAsync(org.Id, listRequest);
        }


        public async Task<OrgLocation> GetOrgLocationAsync(string id, EntityHeader org, EntityHeader user)
        {
            return await _locationRepo.GetLocationAsync(id);
        }

        public async Task<InvokeResult<string>> GetLandingPageForOrgAsync(string orgid)
        {
            var landingPage = await _organizationRepo.GetHomePageForOrgAsync(orgid);
            return InvokeResult<string>.Create(landingPage);
        }


        public async Task<EntityHeader> GetOrgEntityHeaderForNameSpaceAsync(string orgNameSpace)
        {
            var orgId = await _organizationRepo.GetOrganizationIdForNamespaceAsync(orgNameSpace);
            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            return org.ToEntityHeader();
        }

        public Task<string> GetOrgIdForNameSpaceAsync(string orgNameSpace)
        {
            return _organizationRepo.GetOrganizationIdForNamespaceAsync(orgNameSpace);
        }

        public async Task<string> GetOrgNameAsync(string orgId)
        {
            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            return org.Name;
        }
    
        public async Task<string> GetOrgNameSpaceAsync(string orgId)
        {
            var org = await _organizationRepo.GetOrganizationAsync(orgId);
            return org.Namespace;
        }

        public async Task<PublicOrgInformation> GetPublicOrginfoAsync(string orgns)
        {
            var id = await _organizationRepo.GetOrganizationIdForNamespaceAsync(orgns);
            var org = await _organizationRepo.GetOrganizationAsync(id);
            return org.ToPublicOrgInfo();
        }

        public Task<bool> QueryLocationNamespaceInUseAsync(string orgId, string namespaceText)
        {
            return _locationRepo.QueryNamespaceInUseAsync(orgId, namespaceText);
        }

        public Task<bool> QueryOrganizationHasUserAsync(string orgId, string userId, EntityHeader org, EntityHeader user)
        {
            return _orgUserRepo.QueryOrgHasUserAsync(orgId, userId);
        }

        public Task<bool> QueryOrgNamespaceInUseAsync(string namespaceText)
        {
            return  _organizationRepo.QueryNamespaceInUseAsync(namespaceText);
        }
    }
}
