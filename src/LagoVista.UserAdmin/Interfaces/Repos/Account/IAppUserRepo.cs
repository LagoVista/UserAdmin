using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Users
{
    public interface IAppUserRepo : IDisposable
    {
        Task<IEnumerable<UserInfoSummary>> GetUserSummaryForListAsync(IEnumerable<OrgUser> orgUsers);
        Task<ListResponse<UserInfoSummary>> GetDeviceUsersAsync(string deviceRepoId, ListRequest listRequest);
        Task CreateAsync(AppUser user);
        Task<AppUser> FindByIdAsync(string userId);
        Task<AppUser> FindByNameAsync(string userName);
        Task<AppUser> FindByEmailAsync(string email);
        Task<AppUser> FindByThirdPartyLogin(string providerId, string providerKey);
        Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest);
        Task<ListResponse<UserInfoSummary>> GetAllUsersAsync(ListRequest listRequest, bool? emailConfirmed, bool? phoneConfirmed);
        Task UpdateAsync(AppUser user);
        Task DeleteAsync(AppUser user);
    }
}
