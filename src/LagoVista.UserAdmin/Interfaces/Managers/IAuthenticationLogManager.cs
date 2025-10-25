// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a15a3068fbbf397d5f17fcd6cf4b8bcccdc9b04605cc30c980f1b47319062bf2
// IndexVersion: 0
// --- END CODE INDEX META ---
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
        Task<ListResponse<AuthenticationLog>> GetForUserIdAsync(string userId, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<ListResponse<AuthenticationLog>> GetForUserNameAsync(string userName, ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task AddAsync(AuthLogTypes type, string userId = "?", string userName = "?", string orgId = "?", string orgName = "?", string oauthProvier = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none");
        Task AddAsync(AuthLogTypes type, EntityHeader user = null, EntityHeader org = null, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none");
        Task AddAsync(AuthLogTypes type, AppUser appUser, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none");
    }
}
