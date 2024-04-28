using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IAuthenticationLogManager
    {
        Task AddAsync(AuthenticationLog authLog);
        Task<ListResponse<AuthenticationLog>> GetAllAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ListResponse<AuthenticationLog>> GetAllAsync(string orgId, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ListResponse<AuthenticationLog>> GetAsync(AuthLogTypes type, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ListResponse<AuthenticationLog>> GetAsync(string orgId, AuthLogTypes type, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task AddAsync(AuthLogTypes type, string userId = "?", string userName = "?", string orgId = "?", string orgName = "?", string oauthProvier = "", string errors = "", string extras = "", string redirectUri = "");
        Task AddAsync(AuthLogTypes type, EntityHeader user = null, EntityHeader org = null, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "");
        Task AddAsync(AuthLogTypes type, AppUser appUser, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "");
    }
}
