using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class UserRoleRepo : TableStorageBase<UserRoleDTO>, IUserRoleRepo
    {
        public UserRoleRepo(IUserAdminSettings settings, IAdminLogger logger) : 
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        public Task AddUserRole(UserRole role)
        {
            return InsertAsync(role.ToDTO());
        }

        public async Task<List<UserRole>> GetRolesForUserAsync(string userId, string organizationId)
        {
            var results = await this.GetByFilterAsync(FilterOptions.Create(nameof(UserRoleDTO.UserId), FilterOptions.Operators.Equals, userId), 
                                                                                  FilterOptions.Create(nameof(UserRoleDTO.PartitionKey), FilterOptions.Operators.Equals, organizationId));
            return results.Select(usr => usr.ToUserRole()).OrderBy(usr => usr.Role.Text).ToList();
        }

        public Task RemoveUserRole(string userRoleId, string organizationId)
        {
            return RemoveAsync(organizationId, userRoleId);
        }
    }
}
