// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ceb32db3e2d953ab7e32dc98b3389c9798bdaa5f2e53be9124b53efb516c886e
// IndexVersion: 2
// --- END CODE INDEX META ---
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{

    /// <summary>
    /// We need a real simple one of these that doesn't use all the document db services 
    /// document services can get the org w/o a circular reference.
    /// </summary>
    public interface IOrganizationLoaderRepo
    {
        Task<Organization> GetOrganizationAsync(string id);
    }

    public interface IOrganizationRepo
    {
        Task AddOrganizationAsync(Organization org);
        Task<Organization> GetOrganizationAsync(string orgId);
        Task<Organization> GetOrganizationFromNamespaceAsync(string orgNs);
        Task<string> GetOrganizationIdForNamespaceAsync(string orgId);
        Task UpdateOrganizationAsync(Organization org);
        Task<bool> QueryOrganizationExistAsync(string orgId);
        Task<bool> QueryNamespaceInUseAsync(string namespaceText);
        Task DeleteOrgAsync(string orgId);
        Task<bool> HasBillingRecords(string orgId);
        Task<ListResponse<OrganizationSummary>> GetAllOrgsAsync(ListRequest listRequest);
        Task<ListResponse<OrganizationSummary>> GetAllOrgsAsync(string orgSearch, ListRequest listRequest);
        Task<List<EntityHeader>> GetBillingContactOrgsForUserAsync(string orgId, string userId);
        Task<EntityHeader> GetDefaultIndustryForOrgAsync(string orgId);
        Task<string> GetHomePageForOrgAsync(string orgId);
        Task<OrgHostNameRedirect> GetDefaultLandingPageForHostAsync(string hostName);
    }
}