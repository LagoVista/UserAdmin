// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 872cdade797924a08f43e277d7aaf0a1f9a6d29d556f1f7693cf1b1aceff34ba
// IndexVersion: 0
// --- END CODE INDEX META ---
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
