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
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class AuthenticationLogManager : IAuthenticationLogManager
    {
        private const string AnonymousOrganizationId = "anonymous";
        private const string AnonymousOrganizationName = "Anonymous";

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

        public Task AddAsync(AuthLogTypes type, string userId = "?", string userName = "?", string orgId = AnonymousOrganizationId, string orgName = AnonymousOrganizationName, string oauthProvider = "", 
        string errors = "", string extras = "", string redirectUri = "none", string inviteId = "none", string credentialId = "none", string challengeId = "none", string assertionId = "none")
        {
            String ip = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            if (userName == null)
                userName = "?";

            if (String.IsNullOrWhiteSpace(orgId) || orgId == "?")
                orgId = AnonymousOrganizationId;

            if (String.IsNullOrWhiteSpace(orgName) || orgName == "?")
                orgName = AnonymousOrganizationName;

            var auth = new AuthenticationLog(type)
            {
                UserId = userId,
                UserName = userName.ToLower(),
                OrganizationId = orgId,
                IPAddress = ip,
                Organization = orgName,
                Errors = errors,
                Extras = extras,
                InviteId  = inviteId,
                OAuthProvider = oauthProvider,
                RedirectUri = redirectUri,
                ChallengeId = challengeId,
                CredentialId = credentialId,
                AssertionId = assertionId
            };

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, this.Tag(), $"{type}",
                userId.ToKVP("userId"), userName.ToKVP("username"), orgId.ToKVP("orgId"), orgName.ToKVP("orgName"), errors.ToKVP("errors"),
                extras.ToKVP("extras"), oauthProvider.ToKVP("oauthProvider"), type.ToString().ToKVP("authType"), challengeId.ToKVP("challengeId"),
                credentialId.ToKVP("credentialId"), assertionId.ToKVP("assertionId"), redirectUri.ToKVP("redirect"), "true".ToKVP("authlog")); 

            return AddAsync(auth);
        }

        public Task AddAsync(AuthLogTypes type, EntityHeader user = null, EntityHeader org = null, string oauthProvider = "", string errors = "", string extras = "", 
        string redirectUri = "", string inviteId = "none", string credentialId = "none", string challengeId = "none", string assertionId = "none")
        {
            var orgId = org == null ? AnonymousOrganizationId : org.Id;
            var orgName = org == null ? AnonymousOrganizationName : org.Text;

            var userId = user == null ? "?" : user.Id;
            var userName = user == null ? "?" : user.Text;

            return AddAsync(type,
                userId: userId,
                userName: userName,
                orgId: orgId,
                orgName: orgName,
                oauthProvider: oauthProvider,
                errors: errors,
                extras: extras,
                redirectUri: redirectUri,
                inviteId: inviteId,
                credentialId: credentialId,
                challengeId: challengeId,
                assertionId: assertionId);
        }

        public Task AddAsync(AuthLogTypes type, AppUser user, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "", string inviteId = "none",
              string credentialId = "none", string challengeId = "none",  string assertionId = "none")
        {
            if (user == null) throw new ArgumentNullException(nameof(user));

            var orgId = user.CurrentOrganization == null ? AnonymousOrganizationId : user.CurrentOrganization.Id;
            var orgName = user.CurrentOrganization == null ? AnonymousOrganizationName : user.CurrentOrganization.Text;

            return AddAsync(type,
                userId: user.Id,
                userName: user.UserName,
                orgId: orgId,
                orgName: orgName,
                oauthProvider: oauthProvider,
                errors: errors,
                extras: extras,
                redirectUri: redirectUri,
                inviteId: inviteId,
                credentialId: credentialId,
                challengeId: challengeId,
                assertionId: assertionId);
        }

        public Task<ListResponse<AuthenticationLog>> GetAllAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetAllAsync(RequireOrganizationId(org), listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetAllAsync(string orgId, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetAllAsync(orgId, listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetForUserIdAsync(string userId, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetForUserIdAsync(RequireOrganizationId(org), userId, listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetForUserNameAsync(string userName, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetForUserNameAsync(RequireOrganizationId(org), userName, listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetAsync(AuthLogTypes type, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetAsync(RequireOrganizationId(org), type, listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetAsync(string orgId, AuthLogTypes type, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            return _authLogRepo.GetAsync(orgId, type, listRequest);
        }

        private static string RequireOrganizationId(EntityHeader organization)
        {
            if (organization == null || String.IsNullOrWhiteSpace(organization.Id))
            {
                throw new ArgumentNullException(nameof(organization), "Authentication log queries require an organization context.");
            }

            return organization.Id;
        }
    }
}