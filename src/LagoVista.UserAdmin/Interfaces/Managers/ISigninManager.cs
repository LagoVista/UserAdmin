using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISignInManager
    {
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
