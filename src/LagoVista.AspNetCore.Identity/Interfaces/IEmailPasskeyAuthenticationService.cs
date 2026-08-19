using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    /// <summary>
    /// Resolves the account identified by email and delegates passkey proof to the
    /// existing user-bound passkey manager. This service proves the credential only;
    /// callers remain responsible for establishing the appropriate session type.
    /// </summary>
    public interface IEmailPasskeyAuthenticationService
    {
        Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginAsync(string email, string passkeyUrl, EntityHeader organization, EntityHeader user);
        Task<InvokeResult<AppUser>> CompleteAsync(string email, PasskeyAuthenticationCompleteRequest request, EntityHeader organization, EntityHeader user);
    }
}
