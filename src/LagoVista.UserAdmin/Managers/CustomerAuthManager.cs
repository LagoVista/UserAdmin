using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    internal sealed class CustomerAuthManager : ICustomerAuthManager
    {
        private readonly IAuthenticationFlowService _authenticationFlowService;

        public CustomerAuthManager(IAuthenticationFlowService authenticationFlowService)
        {
            _authenticationFlowService = authenticationFlowService ?? throw new ArgumentNullException(nameof(authenticationFlowService));
        }

        public Task<InvokeResult<AuthenticationResponse>> LoginAsync(CustomerLoginRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (String.IsNullOrWhiteSpace(request.EndUserAppOrgId)) throw new InvalidOperationException("EndUserAppOrgId is required for customer login.");

            return _authenticationFlowService.LoginWithPasswordAsync(new AuthLoginRequest
            {
                Email = request.Email,
                Password = request.Password,
                EndUserAppOrgId = request.EndUserAppOrgId,
                RememberMe = request.RememberMe,
                LockoutOnFailure = request.LockoutOnFailure,
            });
        }

        public Task<InvokeResult> ForgotPasswordAsync(CustomerForgotPasswordRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (String.IsNullOrWhiteSpace(request.Email)) throw new InvalidOperationException("Email is required for customer password recovery.");
            if (String.IsNullOrWhiteSpace(request.EndUserAppOrgId)) throw new InvalidOperationException("EndUserAppOrgId is required for customer password recovery.");

            return _authenticationFlowService.RequestPasswordRecoveryAsync(new SendResetPasswordLink
            {
                Email = request.Email,
                UserName = $"{request.Email}@{request.EndUserAppOrgId}",
            });
        }
    }
}
