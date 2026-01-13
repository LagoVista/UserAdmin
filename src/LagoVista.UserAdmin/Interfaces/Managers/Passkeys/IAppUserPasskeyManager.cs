using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers.Passkeys
{
    public interface IAppUserPasskeyManager
    {
        Task<InvokeResult<string>> BeginRegistrationOptionsAsync(string userId, string passkeyUrl, EntityHeader org, EntityHeader user);
        Task<InvokeResult> CompleteRegistrationAsync(string userId, string attestationResponseJson, EntityHeader org, EntityHeader user);

        Task<InvokeResult<string>> BeginAuthenticationOptionsAsync(string userId, bool isStepUp, string passkeyUrl, EntityHeader org, EntityHeader user);
        Task<InvokeResult> CompleteAuthenticationAsync(string userId, string assertionResponseJson, bool isStepUp, EntityHeader org, EntityHeader user);
    }
}
