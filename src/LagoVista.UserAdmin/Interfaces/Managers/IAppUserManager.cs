using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Account;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public interface IAppUserManager
    {
        Task<AppUser> GetUserByIdAsync(String id, EntityHeader requestedByUser);

        Task<AppUser> GetUserByUserNameAsync(string userName, EntityHeader requestedByUser);

        Task<InvokeResult> UpdateUserAsync(AppUser user, EntityHeader updatedByUser);

        Task<InvokeResult> AddUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUserId);

        Task<InvokeResult> DeleteUserAsync(String id, EntityHeader deletedByUser);

        Task<IEnumerable<EntityHeader>> SearchUsers(string firstName, string lastName, EntityHeader searchedBy);
    }
}
