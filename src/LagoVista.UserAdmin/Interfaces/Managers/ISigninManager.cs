using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    /* Shoot me now....hack to make sign in manager testable */
    public interface ISignInManager
    {
        Task<InvokeResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure);

        Task SignInAsync(AppUser user);
    }
}
