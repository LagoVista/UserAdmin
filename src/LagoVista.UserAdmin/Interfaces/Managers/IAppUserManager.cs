using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
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
        Task<InvokeResult> SetApprovedAsync(string userId, EntityHeader org, EntityHeader approvingUser);

        Task<AppUser> GetUserByUserNameAsync(string userName, EntityHeader org, EntityHeader user);

        Task<DependentObjectCheckResult> CheckInUse(string id, EntityHeader org, EntityHeader user);
        Task<ListResponse<UserInfoSummary>> GetActiveUsersAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<ListResponse<UserInfoSummary>> GetUsersWithoutOrgsAsync(EntityHeader user, ListRequest listRequest);
        Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(bool? emailConfirmed, bool? smsConfirmed, EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<InvokeResult> AddMediaResourceAsync(string userId, EntityHeader mediaResource, EntityHeader org, EntityHeader updatedByUser);
        Task<InvokeResult> UpdateUserAsync(UserInfo user, EntityHeader org, EntityHeader updatedByUser);
        Task<InvokeResult> UpdateUserAsync(CoreUserInfo user, EntityHeader org, EntityHeader updatedByUser);
        Task<InvokeResult> UpdateUserAsync(AppUser user, EntityHeader org, EntityHeader updatedByUser);
        Task<InvokeResult> DeleteUserAsync(String id, EntityHeader org, EntityHeader deletedByUser);
        Task<InvokeResult> DisableAccountAsync(string userId, EntityHeader org, EntityHeader adminUser);

        Task<IEnumerable<EntityHeader>> SearchUsers(string firstName, string lastName, EntityHeader searchedBy);

        Task<InvokeResult<CreateUserResponse>> CreateUserAsync(RegisterUser newUser, bool autoLogin = true, bool sendConfirmEmail = true, ExternalLogin externalLogin = null);

        Task<ListResponse<UserInfoSummary>> GetDeviceUsersAsync(string deviceRepoId, EntityHeader org, EntityHeader user, ListRequest listRequest);

        Task<AppUser> AssociateExternalLoginAsync(string userId, ExternalLogin external, EntityHeader user);
        Task<InvokeResult<AppUser>> RemoveExternalLoginAsync(string userId, string externalLoginId, EntityHeader user);
        Task<AppUser> GetUserByExternalLoginAsync(ExternalLoginTypes loginType, string id);
        Task<InvokeResult<AppUser>> AcceptTermsAndConditionsAsync(string ipAddress, EntityHeader org, EntityHeader userEH);

        Task<InvokeResult<PaymentAccounts>> GetPaymentAccountsAsync(string userId, EntityHeader org, EntityHeader user);

        Task<InvokeResult<string>> GetUserSSNAsync(string userId, EntityHeader org, EntityHeader user);

        Task<InvokeResult> MarkUserViewedSystemSerialIndex(int idx, EntityHeader org, EntityHeader user);

        Task<InvokeResult> UpdatePaymentAccountsAsync(string userId, PaymentAccounts accounts, EntityHeader org, EntityHeader user);
    }
}
