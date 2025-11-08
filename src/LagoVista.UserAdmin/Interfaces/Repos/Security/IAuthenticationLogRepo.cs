// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 07f23964ea9483d2fdb0047ed79bad3ddfa18e7fdb7744c9894ffe7940a77111
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{

    public interface IAuthenticationLogRepo
    {
        Task AddAsync(AuthenticationLog authLog);
        Task<ListResponse<AuthenticationLog>> GetAllAsync(ListRequest listRequest);
        Task<ListResponse<AuthenticationLog>> GetForUserNameAsync(string userName, ListRequest listRequest);
        Task<ListResponse<AuthenticationLog>> GetForUserIdAsync(string userId, ListRequest listRequest);
        Task<ListResponse<AuthenticationLog>> GetAllAsync(string orgId, ListRequest listRequest);
        Task<ListResponse<AuthenticationLog>> GetAsync(AuthLogTypes type, ListRequest listRequest);
        Task<ListResponse<AuthenticationLog>> GetAsync(string orgId, AuthLogTypes type, ListRequest listRequest);
    }
}
