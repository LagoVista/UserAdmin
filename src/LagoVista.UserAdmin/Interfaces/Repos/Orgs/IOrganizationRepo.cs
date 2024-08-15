using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
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
        Task<List<EntityHeader>> GetBillingContactOrgsForUserAsync(string userId);
        Task<string> GetLandingPageFororgAsync(string orgId);


    }
}