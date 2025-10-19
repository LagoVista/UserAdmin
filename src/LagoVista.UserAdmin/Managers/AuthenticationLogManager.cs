// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 19a32a116bd4b2211b955a0ae6595d28e99d53f4465e19505debdff6cbf9065c
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Http;
using RingCentral;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class AuthenticationLogManager : IAuthenticationLogManager
    {
        private IAuthenticationLogRepo _authLogRepo;
        private IAdminLogger _adminLogger;

        private IHttpContextAccessor _httpContextAccessor;
        private IBackgroundServiceTaskQueue _bgServiceQueue;

        public AuthenticationLogManager(IHttpContextAccessor httpContextAccessor, IBackgroundServiceTaskQueue bgServiceQueue, IAuthenticationLogRepo authLogRepo, IAdminLogger adminLogger)
        {
            _authLogRepo = authLogRepo ?? throw new ArgumentNullException(nameof(authLogRepo));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _bgServiceQueue = bgServiceQueue ?? throw new ArgumentNullException(nameof(bgServiceQueue));
        }

        public Task AddAsync(AuthenticationLog authLog)
        {
            return _bgServiceQueue.QueueBackgroundWorkItemAsync((ct) => {
                return _authLogRepo.AddAsync(authLog);
            });
        }

        public Task AddAsync(AuthLogTypes type, string userId = "?", string userName = "?", string orgId = "?", string orgName = "?", string oauthProvier = "", string errors = "", string extras = "", string redirectUri = "none", string inviteId = "none")
        {
            String ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            if (userName == null)
                userName = "?";

            var auth = new AuthenticationLog(type)
            {
                UserId = userId,
                UserName = userName.ToLower(),
                OrgId = orgId,
                IPAddress = ip,
                OrgName = orgName,
                Errors = errors,
                Extras = extras,
                InviteId  = inviteId,
                OAuthProvider = oauthProvier,
                RedirectUri = redirectUri
            };

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AuthLog_AddAsync]", $"[AuthLog_AddAsync] - {type}",
                userId.ToKVP("userId"), userName.ToKVP("username"), orgId.ToKVP("orgId"), orgName.ToKVP("orgName"), errors.ToKVP("errors"),
                extras.ToKVP("extras"), oauthProvier.ToKVP("oauthProvider"), type.ToString().ToKVP("authType"), "true".ToKVP("authlog")); 

            return AddAsync(auth);
        }

        public Task AddAsync(AuthLogTypes type, EntityHeader user = null, EntityHeader org = null, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none")
        {
            var orgId = org == null ? "?" : org.Id;
            var orgName = org == null ? "?" : org.Text;

            var userId = user == null ? "?" : user.Id;
            var userName = user == null ? "?" : user.Text;

            return AddAsync(type, userId, userName, orgId, orgName, oauthProvider, errors, extras, redirectUri, inviteId);
        }

        public Task AddAsync(AuthLogTypes type, AppUser user, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none")
        {
            var orgId = user.CurrentOrganization == null ? "?" : user.CurrentOrganization.Id;
            var orgName = user.CurrentOrganization == null ? "?" : user.CurrentOrganization.Text;

            var userId = user == null ? "?" : user.Id;
            var userName = user == null ? "?" : user.UserName;


            return AddAsync(type, userId, userName, orgId, orgName, oauthProvider, errors, extras, redirectUri, inviteId);
        }

        public Task<ListResponse<AuthenticationLog>> GetAllAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetAllAsync(listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetAllAsync(string orgId, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetAllAsync(orgId, listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetAsync(AuthLogTypes type, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetAsync(type, listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetAsync(string orgId, AuthLogTypes type, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetAsync(orgId, type, listRequest);
        }
    }
}
