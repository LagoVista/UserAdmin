using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IUserRoleManager
    {
        Task<InvokeResult> AddRoleAsync(Role role, EntityHeader org, EntityHeader user);
        Task<Role> GetRoleAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateRoleAsync(Role role, EntityHeader org, EntityHeader user);
        Task<List<RoleSummary>> GetRolesAsync(EntityHeader org, EntityHeader user);
    }
}
