using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{

    public interface IAuthenticationLogRepo
    {
        Task AddAsync(AuthenticationLog authLog);
        Task<ListResponse<AuthenticationLog>> GetAllAsync(ListRequest listRequest);
        Task<ListResponse<AuthenticationLog>> GetAllAsync(string orgId, ListRequest listRequest);
        Task<ListResponse<AuthenticationLog>> GetAsync(AuthLogTypes type, ListRequest listRequest);
        Task<ListResponse<AuthenticationLog>> GetAsync(string orgId, AuthLogTypes type, ListRequest listRequest);
    }
}
