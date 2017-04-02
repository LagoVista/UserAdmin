using System.Threading.Tasks;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IOrganizationRepo
    {
        Task AddOrganizationAsync(Organization account);
        Task<Organization> GetOrganizationAsync(string id);
        Task UpdateOrganizationAsync(Organization account);
        Task<bool> QueryOrganizationExistAsync(string id);
        Task<bool> QueryNamespaceInUseAsync(string namespaceText);
    }
}