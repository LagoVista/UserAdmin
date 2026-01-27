// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0f5b3d34f4c51939ff35fafc0a690a08a5b21b10184f402605e58d0188d286b1
// IndexVersion: 2
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
        Task AddAsync(AuthLogTypes type, string userId = "?", string userName = "?", string orgId = "?", string orgName = "?", string oauthProvier = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none", string credentialId = "", string challengeId = "", string assertionId = "");
        Task AddAsync(AuthLogTypes type, EntityHeader user = null, EntityHeader org = null, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none", string credentialId = "", string challengeId = "",string assertionId = "");
        Task AddAsync(AuthLogTypes type, AppUser appUser, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none", string credentialId = "", string challengeId = "", string assertionId = "");
    }
}
