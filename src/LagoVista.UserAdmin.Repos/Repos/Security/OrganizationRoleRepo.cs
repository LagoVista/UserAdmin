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

        public Task AddRoleForUserAsync(OrganizationUserRole orgUserRole)
        {
            return InsertAsync(orgUserRole);
        }

        public Task<bool> ConfirmUserInRoleAsync(string locationId, string userId, string roleId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrganizationUserRole>> GetUserForRoleAsync(string roleId, string orgId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OrganizationUserRole>> GetRolesForUserAsync(string usertId, string orgId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAllRolesForUserInOrgAsync(string orgId, string userId)
        {
            throw new NotImplementedException();
        }

        public Task RevokeRoleForUserInOrgAsync(string orgId, string userId, string roleId)
        {
            throw new NotImplementedException();
        }
    }
}
