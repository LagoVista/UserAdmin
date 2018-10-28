using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public interface IAppUserManagerReadOnly
    {
        Task<AppUser> GetUserByIdAsync(String id, EntityHeader org, EntityHeader user);

        Task<AppUser> GetUserByUserNameAsync(string userName, EntityHeader org, EntityHeader user);

        Task<IEnumerable<EntityHeader>> SearchUsers(string firstName, string lastName, EntityHeader searchedBy);
    }

    public interface IAppUserManager
    {
        Task<AppUser> GetUserByIdAsync(String id, EntityHeader org, EntityHeader user);

        Task<AppUser> GetUserByUserNameAsync(string userName, EntityHeader org, EntityHeader user);

        Task<DependentObjectCheckResult> CheckInUse(string id, EntityHeader org, EntityHeader user);

        Task<InvokeResult> UpdateUserAsync(UserInfo user, EntityHeader org, EntityHeader updatedByUser);

        Task<InvokeResult> DeleteUserAsync(String id, EntityHeader org, EntityHeader deletedByUser);

        Task<IEnumerable<EntityHeader>> SearchUsers(string firstName, string lastName, EntityHeader searchedBy);

        Task<InvokeResult<AuthResponse>> CreateUserAsync(RegisterUser newUser);

        Task<ListResponse<UserInfoSummary>> GetDeviceUsersAsync(string deviceRepoId, EntityHeader org, EntityHeader user, ListRequest listRequest);
    }
}
