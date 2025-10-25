// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4d72d263b48e24adc2f2d6d8d9467d8a40c23bf6dd48bb791a3c85f5a0f67b05
// IndexVersion: 0
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
