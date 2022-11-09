using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.PlatformSupport;
using System.Threading.Tasks;
using System;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.IoT.Logging.Loggers;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.UserAdmin.Repos.Security
{
    public class RoleRepo : DocumentDBRepoBase<Role>, IRoleRepo
    {
        public RoleRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserStorage.Uri, settings.UserStorage.AccessKey, settings.UserStorage.ResourceName, logger)
        {

        }

        public Task AddRoleAsync(Role role)
        {
            return CreateDocumentAsync(role);
        }

        public Task<Role> GetAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<List<Role>> GetAllPublicRoles()
        {
            var roles = await QueryAsync(role => role.IsPublic);
            return roles.ToList();
        }

        public async Task<List<Role>> GetRolesForOrganization(string id)
        {
            var roles = await QueryAsync(role => role.OwnerOrganization.Id == id);
            return roles.ToList();
        }

        public Task InsertAsync(Role role)
        {
            return CreateDocumentAsync(role);
        }

        public Task RemoveAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task UpdateAsync(Role role)
        {
            return base.UpsertDocumentAsync(role);
        }
    }
}
