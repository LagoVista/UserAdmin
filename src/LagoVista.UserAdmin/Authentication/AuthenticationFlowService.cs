using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication
{
    [CriticalCoverage]
    public class AuthenticationFlowService : IAuthenticationFlowService
    {
        private readonly IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest> _passwordRecoveryRequestHandler;

        public AuthenticationFlowService(IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest> passwordRecoveryRequestHandler)
        {
            _passwordRecoveryRequestHandler = passwordRecoveryRequestHandler ?? throw new ArgumentNullException(nameof(passwordRecoveryRequestHandler));
        }

        public async Task<InvokeResult> RequestPasswordRecoveryAsync(SendResetPasswordLink request)
        {
            var result = await _passwordRecoveryRequestHandler.HandleAsync(new PasswordRecoveryRequestFlowRequest(request));
            if (result.TransitionKey != PasswordRecoveryRequestFlowHandler.TransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }
    }
}
