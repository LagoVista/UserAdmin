// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a41c2ef281534cbf5a7bccbf5ef25b437967e6caed3e8bcdb928b3a4c1943bea
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISignInManager
    {
        Task<InvokeResult<UserLoginResponse>> CompleteSignInToAppAsync(AppUser appUser, Stopwatch sw = null, string inviteId = "", string orgId = "");

        Task<InvokeResult<UserLoginResponse>> PasswordSignInAsync(AuthLoginRequest loginRequest);

        Task SignInAsync(AppUser user, bool isPersistent = false);

        Task SignOutAsync();

        /// <summary>
        /// Method can be called to refresh the user claims
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task RefreshUserLoginAsync(AppUser user);
    
       
    }
}
