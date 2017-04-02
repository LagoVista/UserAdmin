using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IOrganizationLocationRepo : IDisposable
    {
        Task AddLocationAsync(OrganizationLocation account);
        Task<OrganizationLocation> GetLocationAsync(string id);
        Task UpdateLocationAsync(OrganizationLocation account);
        Task<IEnumerable<OrganizationLocation>> GetOrganizationLocationAsync(String organizationId);
        Task<bool> QueryNamespaceInUseAsync(string orgId, string namespaceText);
    }
}