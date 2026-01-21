using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers.Passkeys
{
    public interface IAppUserPasskeyManager
    {
        /* Existing user-bound flows (attach/use passkeys for a known user) */
        Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginRegistrationOptionsAsync(string userId, string passkeyUrl, EntityHeader org, EntityHeader user);
        Task<InvokeResult> CompleteRegistrationAsync(string userId, PasskeyRegistrationCompleteRequest request, EntityHeader org, EntityHeader user);

        Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginAuthenticationOptionsAsync(string userId, bool isStepUp, string passkeyUrl, EntityHeader org, EntityHeader user);
        Task<InvokeResult> CompleteAuthenticationAsync(string userId, PasskeyAuthenticationCompleteRequest payload, bool isStepUp, EntityHeader org, EntityHeader user);

        /* Passwordless (discoverable/resident) flows */
        Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginPasswordlessRegistrationOptionsAsync(string passkeyUrl, EntityHeader org, EntityHeader user);
        Task<InvokeResult<PasskeySignInResult>> CompletePasswordlessRegistrationAsync(PasskeyRegistrationCompleteRequest attestationResponseJson, EntityHeader org, EntityHeader user);

        Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginPasswordlessAuthenticationOptionsAsync(string passkeyUrl, EntityHeader org, EntityHeader user);
        Task<InvokeResult<PasskeySignInResult>> CompletePasswordlessAuthenticationAsync(PasskeyAuthenticationCompleteRequest assertionResponseJson, EntityHeader org, EntityHeader user);

        /* Management */
        Task<InvokeResult<PasskeyCredentialSummary[]>> ListPasskeysAsync(string userId, EntityHeader org, EntityHeader user);
        Task<InvokeResult> RenamePasskeyAsync(string userId, string credentialId, string name, EntityHeader org, EntityHeader user);
        Task<InvokeResult> RemovePasskeyAsync(string userId, string credentialId, EntityHeader org, EntityHeader user);
    }
}
