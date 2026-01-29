// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7784b1ea2793f4eb0e8ee2576b431f23112b1cbec942ea6dec64e63f16edb9f0
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Users
{

    public interface IAppUserLoaderRepo
    {
        Task<AppUser> FindByIdAsync(string userId);
    }

    public interface IAppUserRepo 
    {
        Task<IEnumerable<UserInfoSummary>> GetUserSummaryForListAsync(IEnumerable<OrgUser> orgUsers, bool useCache);
        Task<ListResponse<UserInfoSummary>> GetDeviceUsersAsync(string deviceRepoId, ListRequest listRequest);
        Task<ListResponse<UserInfoSummary>> GetActiveUsersAsync(ListRequest listRequest);
        Task CreateAsync(AppUser user);
        Task<AppUser> GetCachedAppUserAsync(string id);
        Task<AppUser> FindByIdAsync(string userId);
        Task<AppUser> FindByNameAsync(string userName);
        Task<AppUser> FindByEmailAsync(string email);
        Task<AppUser> FindByThirdPartyLogin(string providerId, string providerKey);
        Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest);
        Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest, bool? phoneConfirmed = true, bool? emailConfirmed = null);
        Task<ListResponse<UserInfoSummary>> GetUsersWithoutOrgsAsync(ListRequest listRequest);
        Task UpdateAsync(AppUser user);
        Task DeleteAsync(AppUser user, bool softDelete = true);
        Task DeleteAsync(string userId);
        Task<AppUser> GetUserByExternalLoginAsync(ExternalLoginTypes loginType, string id);
        Task<InvokeResult<long>> TryAcceptTotpTimeStepAsync(string userId, long candidateStep, bool updateLastMfaDateTimeUtc, string lastMfaDateTimeUtc);

        Task<AppUser> AssociateExternalLoginAsync(string userId, ExternalLogin external);
        Task<AppUser> RemoveExternalLoginAsync(string userId, string externalLoginId);
        Task<ListResponse<UserInfoSummary>> SearchUsersAsync(string firstName, string lastName, string email, ListRequest listRequest);
    }
}
