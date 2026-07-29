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
        private readonly IAuthenticationFlowHandler<PasswordLoginFlowRequest> _passwordLoginHandler;
        private readonly IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest> _passwordRecoveryRequestHandler;

        public AuthenticationFlowService(IAuthenticationFlowHandler<PasswordLoginFlowRequest> passwordLoginHandler, IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest> passwordRecoveryRequestHandler)
        {
            _passwordLoginHandler = passwordLoginHandler ?? throw new ArgumentNullException(nameof(passwordLoginHandler));
            _passwordRecoveryRequestHandler = passwordRecoveryRequestHandler ?? throw new ArgumentNullException(nameof(passwordRecoveryRequestHandler));
        }

        public async Task<InvokeResult<AuthenticationResponse>> LoginWithPasswordAsync(AuthLoginRequest request)
        {
            var result = await _passwordLoginHandler.HandleAsync(new PasswordLoginFlowRequest(request));
            if (result.TransitionKey != PasswordLoginFlowHandler.TransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            if (result.PublicResult is InvokeResult<AuthenticationResponse> publicResult)
                return publicResult;

            throw new InvalidOperationException($"Authentication flow transition [{result.TransitionKey}] returned an unsupported result type [{result.PublicResult.GetType().FullName}].");
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
