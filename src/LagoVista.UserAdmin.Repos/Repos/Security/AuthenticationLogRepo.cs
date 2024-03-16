using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
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
    }
}
