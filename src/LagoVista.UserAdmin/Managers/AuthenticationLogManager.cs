using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class AuthenticationLogManager : IAuthenticationLogManager
    {
        private IAuthenticationLogRepo _authLogRepo;

        public AuthenticationLogManager(IAuthenticationLogRepo authLogRepo)
        {
            _authLogRepo = authLogRepo ?? throw new ArgumentNullException(nameof(authLogRepo));
        }

        public Task AddAsync(AuthenticationLog authLog)
        {
            return _authLogRepo.AddAsync(authLog);
        }

        public Task AddAsync(AuthLogTypes type, string userId = "?", string userName = "?", string orgId = "?", string orgName = "?", string oauthProvier = "", string errors = "", string extras = "")
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

            return AddAsync(auth);
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
