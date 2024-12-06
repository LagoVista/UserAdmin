using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface ISecureLinkRepo
    {
        Task AddSecureLinkAsync(SecureLink secureLink);
        Task UpdateSecureLinkAsync(SecureLink secureLink);
        Task<SecureLink> GetSecureLinkAsync(string orgId, string linkId);
        Task RevokeLinkAsync(string orgId, string linkId);
    }
}
