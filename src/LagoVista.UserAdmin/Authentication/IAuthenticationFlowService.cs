using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication
{
    public interface IAuthenticationFlowService
    {
        Task<InvokeResult> RequestPasswordRecoveryAsync(SendResetPasswordLink request);
    }
}
