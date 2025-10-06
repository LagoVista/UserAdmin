using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public interface IUserRegistrationManager
    {
        Task<InvokeResult<CreateUserResponse>> CreateUserAsync(RegisterUser newUser, bool autoLogin = true, ExternalLogin externalLogin = null);
    }
}
