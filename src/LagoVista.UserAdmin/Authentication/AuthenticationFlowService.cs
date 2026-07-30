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
        private readonly IPasswordLoginFlowHandler _passwordLoginHandler;
        private readonly IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest> _passwordRecoveryRequestHandler;
        private readonly IAuthenticationFlowHandler<PasswordRecoveryCompletionFlowRequest> _passwordRecoveryCompletionHandler;
        private readonly IAuthenticationFlowHandler<InvitationAcceptanceFlowRequest> _invitationAcceptanceHandler;

        public AuthenticationFlowService(IPasswordLoginFlowHandler passwordLoginHandler, IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest> passwordRecoveryRequestHandler, IAuthenticationFlowHandler<PasswordRecoveryCompletionFlowRequest> passwordRecoveryCompletionHandler = null, IAuthenticationFlowHandler<InvitationAcceptanceFlowRequest> invitationAcceptanceHandler = null)
        {
            _passwordLoginHandler = passwordLoginHandler ?? throw new ArgumentNullException(nameof(passwordLoginHandler));
            _passwordRecoveryRequestHandler = passwordRecoveryRequestHandler ?? throw new ArgumentNullException(nameof(passwordRecoveryRequestHandler));
            _passwordRecoveryCompletionHandler = passwordRecoveryCompletionHandler;
            _invitationAcceptanceHandler = invitationAcceptanceHandler;
        }

        public Task<InvokeResult<AuthenticationResponse>> LoginWithPasswordAsync(AuthLoginRequest request)
        {
            return _passwordLoginHandler.HandleAsync(request);
        }

        public async Task<InvokeResult> RequestPasswordRecoveryAsync(SendResetPasswordLink request)
        {
            var result = await _passwordRecoveryRequestHandler.HandleAsync(new PasswordRecoveryRequestFlowRequest(request));
            if (result.TransitionKey != PasswordRecoveryRequestFlowHandler.TransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }

        public async Task<InvokeResult> CompletePasswordRecoveryAsync(ResetPassword request)
        {
            if (_passwordRecoveryCompletionHandler == null)
                throw new InvalidOperationException("Password recovery completion flow handler is not configured.");

            var result = await _passwordRecoveryCompletionHandler.HandleAsync(new PasswordRecoveryCompletionFlowRequest(request));
            if (result.TransitionKey != PasswordRecoveryCompletionFlowHandler.TransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }

        public async Task<InvokeResult<AcceptInviteResponse>> AcceptInvitationAsync(string inviteId, string userId)
        {
            if (_invitationAcceptanceHandler == null)
                throw new InvalidOperationException("Invitation acceptance flow handler is not configured.");

            var result = await _invitationAcceptanceHandler.HandleAsync(new InvitationAcceptanceFlowRequest(inviteId, userId));
            if (result.TransitionKey != InvitationAcceptanceFlowHandler.TransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return (InvokeResult<AcceptInviteResponse>)result.PublicResult;
        }
    }
}
