using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IOrgHelper
    {
        Task<InvokeResult> SetUserOrgAsync(AuthRequest authRequest, AppUser appUser);
    }
}
