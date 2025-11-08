// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fc483431fcaa73ba64d684f6272af355a183b70239b2e700dc48b93daee6baf5
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public interface IAuthClient
    {
        Task<InvokeResult<AuthResponse>> LoginAsync(AuthRequest loginInfo, CancellationTokenSource cancellationTokenSource = null);
        Task<InvokeResult> ResetPasswordAsync(string emailAddress, CancellationTokenSource cancellationTokenSource = null);
    }
}
