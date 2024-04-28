﻿using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class AuthenticationLogManager : IAuthenticationLogManager
    {
        private IAuthenticationLogRepo _authLogRepo;
        private IAdminLogger _adminLogger;

        public AuthenticationLogManager(IAuthenticationLogRepo authLogRepo, IAdminLogger adminLogger)
        {
            _authLogRepo = authLogRepo ?? throw new ArgumentNullException(nameof(authLogRepo));
            _adminLogger = adminLogger ?? throw new ArgumentNullException(nameof(adminLogger));
        }

        public Task AddAsync(AuthenticationLog authLog)
        {
            return _authLogRepo.AddAsync(authLog);
        }

        public Task AddAsync(AuthLogTypes type, string userId = "?", string userName = "?", string orgId = "?", string orgName = "?", string oauthProvier = "", string errors = "", string extras = "", string redirectUri = "")
        {
            var auth = new AuthenticationLog(type)
            {
                UserId = userId,
                UserName = userName,
                OrgId = orgId,
                OrgName = orgName,
                Errors = errors,
                Extras = extras,
                OAuthProvider = oauthProvier
            };

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[AuthLog]", "Authentication Log",
                userId.ToKVP("userId"), userName.ToKVP("username"), orgId.ToKVP("orgId"), orgName.ToKVP("orgName"), errors.ToKVP("errors"),
                extras.ToKVP("extras"), oauthProvier.ToKVP("oauthProvider")
                ); 

            return AddAsync(auth);
        }

        public Task AddAsync(AuthLogTypes type, EntityHeader user = null, EntityHeader org = null, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "")
        {
            return AddAsync(type, user?.Id, user?.Text, org?.Id, org?.Text, oauthProvider, errors, extras, redirectUri);
        }

        public Task AddAsync(AuthLogTypes type, AppUser user, string oauthProvider = "", string errors = "", string extras = "", string redirectUri = "")
        {
            return AddAsync(type, user?.Id, user?.UserName, user.CurrentOrganization?.Id, user.CurrentOrganization?.Text, oauthProvider, errors, extras, redirectUri);
            
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
