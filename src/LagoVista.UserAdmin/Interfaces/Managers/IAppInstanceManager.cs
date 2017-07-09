using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IAppInstanceManager
    {
        Task<InvokeResult<AppInstance>> CreateForUserAsync(AppUser appUser, AuthRequest authRequest);

        Task<InvokeResult> UpdateLastLoginAsync(string appUserId, string appInstanceId);

        Task<InvokeResult> UpdateLastAccessTokenRefreshAsync(string appUserId, string appInstanceId);

    }
}
