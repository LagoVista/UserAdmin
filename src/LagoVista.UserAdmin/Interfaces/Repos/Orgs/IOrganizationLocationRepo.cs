using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IOrgLocationRepo : IDisposable
    {
        Task AddLocationAsync(OrgLocation orgLocation);
        Task<OrgLocation> GetLocationAsync(string id);
        Task UpdateLocationAsync(OrgLocation orgLocation);
        Task<IEnumerable<OrgLocation>> GetOrganizationLocationAsync(String orgId);
        Task<bool> QueryNamespaceInUseAsync(string orgId, string namespaceText);
    }
}