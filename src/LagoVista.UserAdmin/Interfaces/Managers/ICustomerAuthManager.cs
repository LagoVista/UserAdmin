using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public interface ICustomerAuthManager
    {
        Task<InvokeResult<AuthenticationResponse>> LoginAsync(CustomerLoginRequest request);
        Task<InvokeResult> ForgotPasswordAsync(CustomerForgotPasswordRequest request);
    }
}
