// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b1aaf23f553405ad927095e268ced450c4e7155fdcf709bcfe0a547939d3a815
// IndexVersion: 0
// --- END CODE INDEX META ---
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
