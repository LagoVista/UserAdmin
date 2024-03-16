using LagoVista.CloudStorage.Storage;
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
        {

        }

        public Task WriteAsync(string userName, string userId, string orgId, string orgName, AuthLogTypes type, string extras, string errors)
        {
            throw new NotImplementedException();
        }
    }
}
