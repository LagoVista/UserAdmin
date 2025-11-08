// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 905f78c6f3143d344e0f288b61412b8e27bb2ff0581d8f61231cf43160d47bf2
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class AuthenticationLogRepo : TableStorageBase<AuthenticationLog>, IAuthenticationLogRepo
    {
        public AuthenticationLogRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {}

        public Task AddAsync(AuthenticationLog authLog)
        {
            return InsertAsync(authLog);
        }

        public Task<ListResponse<AuthenticationLog>> GetAllAsync(ListRequest listRequest)
        {
            return base.GetPagedResultsAsync(listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetAllAsync(string orgId, ListRequest listRequest)
        {
            return base.GetPagedResultsAsync(listRequest, FilterOptions.Create( nameof(AuthenticationLog.OrgId), FilterOptions.Operators.Equals, orgId));
        }

        public Task<ListResponse<AuthenticationLog>> GetAsync(AuthLogTypes type, ListRequest listRequest)
        {
            return base.GetPagedResultsAsync(Enum.GetName(typeof(AuthLogTypes), type).ToString().ToLower(), listRequest);
        }

        public Task<ListResponse<AuthenticationLog>> GetAsync(string orgId, AuthLogTypes type, ListRequest listRequest)
        {
            return base.GetPagedResultsAsync(Enum.GetName(typeof(AuthLogTypes), type).ToString().ToLower(), listRequest, FilterOptions.Create(nameof(AuthenticationLog.OrgId), FilterOptions.Operators.Equals, orgId));
        }

        public Task<ListResponse<AuthenticationLog>> GetForUserIdAsync(string userId, ListRequest listRequest)
        {
            return base.GetPagedResultsAsync(listRequest, FilterOptions.Create(nameof(AuthenticationLog.UserId), FilterOptions.Operators.Equals, userId));
        }

        public Task<ListResponse<AuthenticationLog>> GetForUserNameAsync(string userName, ListRequest listRequest)
        {
            return base.GetPagedResultsAsync(listRequest, FilterOptions.Create(nameof(AuthenticationLog.UserName), FilterOptions.Operators.Equals, userName));
        }

    }
}
