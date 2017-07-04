using System.Threading.Tasks;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IOrganizationRepo
    {
        Task AddOrganizationAsync(Organization org);
        Task<Organization> GetOrganizationAsync(string orgId);
        Task UpdateOrganizationAsync(Organization org);
        Task<bool> QueryOrganizationExistAsync(string orgId);
        Task<bool> QueryNamespaceInUseAsync(string namespaceText);
    }
}