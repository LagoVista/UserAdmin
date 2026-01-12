using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos
{
    public interface IAppUserTrustedDeviceRepo
    {
        Task<InvokeResult> AddAsync(string orgId, string userId, string tokenHash, string expiresUtc);
        Task<InvokeResult> RemoveAsync(string orgId, string userId, string tokenHash);
        Task<InvokeResult> RemoveAllAsync(string orgId, string userId);

        // Returns null if not found.
        Task<InvokeResult<string>> GetExpiresUtcAsync(string orgId, string userId, string tokenHash);
    }
}
