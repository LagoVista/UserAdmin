using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.CloudStorage.Storage;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.IoT.Logging.Loggers;

namespace LagoVista.UserAdmin.Repos.Security
{
    public class OrganizationRoleRepo : TableStorageBase<OrganizationUserRole>, IOrganizationRoleRepo
    {
        public OrganizationRoleRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task AddRoleForAccountAsync(OrganizationUserRole orgUserRole)
        {
            return InsertAsync(orgUserRole);
        }

        public Task<bool> ConfirmUserInRoleAsync(string locationId, string accountId, string roleId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrganizationUserRole>> GetAccountsForRoleAsync(string roleId, string organziationId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrganizationUserRole>> GetRolesForAccountAsync(string accountId, string organizationId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAllRolesForAccountInOrgAsync(string orgId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRoleForAccountInOrgAsync(string orgId, string userId, string roleId)
        {
            throw new NotImplementedException();
        }
    }
}
