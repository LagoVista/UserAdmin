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
