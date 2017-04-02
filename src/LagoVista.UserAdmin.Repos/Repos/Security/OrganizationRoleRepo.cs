using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.CloudStorage.Storage;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Security;

namespace LagoVista.UserAdmin.Repos.Security
{
    public class OrganizationRoleRepo : TableStorageBase<LocationAccountRoles>, IOrganizationRoleRepo
    {
        public OrganizationRoleRepo(IAppUserManagementSettings settings, ILogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task<OrganizationAccountRoles> AddRoleForAccountAsync(EntityHeader organization, EntityHeader account, EntityHeader role, EntityHeader addedBy)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrganizationAccountRoles>> GetAccountsForRoleAsync(string roleId, string organziationId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrganizationAccountRoles>> GetRolesForAccountAsync(string accountId, string organizationId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAllRolesForAccountInOrgAsync(EntityHeader org, EntityHeader account, EntityHeader revokedBy)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRoleForAccountInOrgAsync(EntityHeader org, EntityHeader role, EntityHeader account, EntityHeader revokedBy)
        {
            throw new NotImplementedException();
        }
    }
}
