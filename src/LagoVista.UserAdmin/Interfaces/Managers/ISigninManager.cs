using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
using System.Diagnostics;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ISignInManager
    {
        Task<InvokeResult<AuthenticationResponse>> CompleteSignInToAppAsync(AppUser appUser, Stopwatch sw = null, string inviteId = "", string orgId = "");
        Task<InvokeResult<AuthenticationResponse>> PasswordSignInAsync(AuthLoginRequest loginRequest);
        Task<InvokeResult<AppUser>> VerifyPasswordForMfaAsync(AuthLoginRequest loginRequest);
        Task SignInAsync(AppUser user, bool isPersistent = false);
        Task SignInProvisionalAsync(AppUser user, string actorId, bool isPersistent = false);
        Task SignOutAsync();
        Task RefreshUserLoginAsync(AppUser user);
    }
}
