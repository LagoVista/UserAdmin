using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.Core.Models.UIMetaData;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IOrgLocationRepo 
    {
        Task AddLocationAsync(OrgLocation orgLocation);
        Task<OrgLocation> GetLocationAsync(string id);
        Task UpdateLocationAsync(OrgLocation orgLocation);
        Task<ListResponse<OrgLocationSummary>> GetOrganizationLocationAsync(String orgId, ListRequest listRequest);
        Task<ListResponse<OrgLocationSummary>> GetOrganizationLocationsForCustomerAsync(string orgId, string customerId, ListRequest listRequest);
        Task<bool> QueryNamespaceInUseAsync(string orgId, string namespaceText);
    }
}