using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public interface IAppUserManager
    {
        Task<AppUser> GetUserByIdAsync(String id, EntityHeader org, EntityHeader user);

        Task<AppUser> GetUserByUserNameAsync(string userName, EntityHeader org, EntityHeader user);

        Task<DependentObjectCheckResult> CheckInUse(string id, EntityHeader org, EntityHeader user);

        Task<InvokeResult> UpdateUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUser);

        Task<InvokeResult> AddUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUserId);

        Task<InvokeResult> DeleteUserAsync(String id, EntityHeader org, EntityHeader deletedByUser);

        Task<IEnumerable<EntityHeader>> SearchUsers(string firstName, string lastName, EntityHeader searchedBy);
    }
}
