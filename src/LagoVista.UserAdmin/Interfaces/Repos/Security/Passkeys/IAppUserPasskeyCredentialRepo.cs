using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys
{
    public interface IAppUserPasskeyCredentialRepo
    {
        Task<InvokeResult> AddAsync(PasskeyCredential credential);
        Task<IEnumerable<PasskeyCredential>> GetByUserAsync(string userId, string rpId);
        Task<PasskeyCredential> FindByCredentialIdAsync(string rpId, string credentialId);
        Task<InvokeResult> RemoveAsync(string userId, string rpId, string credentialId);
        Task<InvokeResult> UpdateSignCountAsync(string userId, string rpId, string credentialId, uint signCount, string lastUsedUtc);
    }
}
