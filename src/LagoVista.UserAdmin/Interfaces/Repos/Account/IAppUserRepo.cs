using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Users
{
    public interface IAppUserRepo 
    {
        Task<IEnumerable<UserInfoSummary>> GetUserSummaryForListAsync(IEnumerable<OrgUser> orgUsers);
        Task<ListResponse<UserInfoSummary>> GetDeviceUsersAsync(string deviceRepoId, ListRequest listRequest);
        Task<ListResponse<UserInfoSummary>> GetActiveUsersAsync(ListRequest listRequest);
        Task CreateAsync(AppUser user);
        Task<AppUser> GetCachedAppUserAsync(string id);
        Task<AppUser> FindByIdAsync(string userId);
        Task<AppUser> FindByNameAsync(string userName);
        Task<AppUser> FindByEmailAsync(string email);
        Task<AppUser> FindByThirdPartyLogin(string providerId, string providerKey);
        Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest);
        Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest, bool? emailConfirmed, bool? phoneConfirmed);
        Task<ListResponse<UserInfoSummary>> GetUsersWithoutOrgsAsync(ListRequest listRequest);
        Task UpdateAsync(AppUser user);
        Task DeleteAsync(AppUser user);
        Task DeleteAsync(string userId);
        Task<AppUser> GetUserByExternalLoginAsync(ExternalLoginTypes loginType, string id);
        Task<AppUser> AssociateExternalLoginAsync(string userId, ExternalLogin external);
        Task<AppUser> RemoveExternalLoginAsync(string userId, string externalLoginId);
    }
}
