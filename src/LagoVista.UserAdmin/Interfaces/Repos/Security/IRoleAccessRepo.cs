using LagoVista.UserAdmin.Models.Security;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IRoleAccessRepo
    {
        Task<List<RoleAccess>> GetRoleAccessForRoleAsync(string roleId, string organizationId);

        Task AddRoleAccess(RoleAccess roleAccess);

        Task RemoveRoleAccess(string roleAccessId, string organizationId);
    }
}
