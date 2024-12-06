using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class SecureLinkRepo : TableStorageBase<SecureLink>
    {
        public SecureLinkRepo(IUserAdminSettings settings, IAdminLogger logger) : base(accountName, accountKey, logger)
        {
        }
    }
}
