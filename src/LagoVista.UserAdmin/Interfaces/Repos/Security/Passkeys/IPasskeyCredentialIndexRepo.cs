using LagoVista.UserAdmin.Models.Security.Passkeys;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys
{
    public interface IPasskeyCredentialIndexRepo
    {
        Task<PasskeyCredentialIndex> FindAsync(string rpId, string credentialId);
        Task RemoveAsync(string rpId, string credentialId);
        Task InsertAsync(PasskeyCredentialIndex entity);
    }
}
