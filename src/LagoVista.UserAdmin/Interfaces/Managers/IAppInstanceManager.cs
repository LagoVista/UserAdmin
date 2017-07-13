using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IAppInstanceManager
    {
        Task<InvokeResult<AppInstance>> CreateForUserAsync(string appUserId, AuthRequest authRequest);

        Task<InvokeResult<AppInstance>> UpdateLastLoginAsync(string appUserId, AuthRequest authRequeset);

        Task<InvokeResult<AppInstance>> UpdateLastAccessTokenRefreshAsync(string appUserId, AuthRequest authRequest);

    }
}
