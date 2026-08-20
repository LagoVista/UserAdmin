using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IPasskeyMfaAuthenticationService
    {
        Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginAsync(string mfaChallengeId, string passkeyUrl, EntityHeader organization, EntityHeader user);
        Task<InvokeResult<AppUser>> CompleteAsync(string mfaChallengeId, PasskeyAuthenticationCompleteRequest request, EntityHeader organization, EntityHeader user);
    }
}
