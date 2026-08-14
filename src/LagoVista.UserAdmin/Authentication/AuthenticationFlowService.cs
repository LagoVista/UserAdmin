using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
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
        private readonly IAuthenticationFlowHandler<InvitationAcceptanceFlowRequest, AcceptInviteResponse> _invitationAcceptanceHandler;
        private readonly IAuthenticationFlowHandler<EmailVerificationFlowRequest> _emailVerificationHandler;
        private readonly IAuthenticationFlowHandler<PasswordRecoveryVerificationFlowRequest, string> _passwordRecoveryVerificationHandler;
        private readonly IAuthenticationFlowHandler<PasswordChangeFlowRequest> _passwordChangeHandler;

        public AuthenticationFlowService(IPasswordLoginFlowHandler passwordLoginHandler, IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest> passwordRecoveryRequestHandler, IAuthenticationFlowHandler<PasswordRecoveryCompletionFlowRequest> passwordRecoveryCompletionHandler = null, IAuthenticationFlowHandler<InvitationAcceptanceFlowRequest, AcceptInviteResponse> invitationAcceptanceHandler = null, IAuthenticationFlowHandler<EmailVerificationFlowRequest> emailVerificationHandler = null, IAuthenticationFlowHandler<PasswordRecoveryVerificationFlowRequest, string> passwordRecoveryVerificationHandler = null, IAuthenticationFlowHandler<PasswordChangeFlowRequest> passwordChangeHandler = null)
        {
            _passwordLoginHandler = passwordLoginHandler ?? throw new ArgumentNullException(nameof(passwordLoginHandler));
            _passwordRecoveryRequestHandler = passwordRecoveryRequestHandler ?? throw new ArgumentNullException(nameof(passwordRecoveryRequestHandler));
            _passwordRecoveryCompletionHandler = passwordRecoveryCompletionHandler;
            _invitationAcceptanceHandler = invitationAcceptanceHandler;
            _emailVerificationHandler = emailVerificationHandler;
            _passwordRecoveryVerificationHandler = passwordRecoveryVerificationHandler;
            _passwordChangeHandler = passwordChangeHandler;
        }

        public async Task<InvokeResult<AuthenticationResponse>> LoginWithPasswordAsync(AuthLoginRequest request)
        {
            var result = await _passwordLoginHandler.HandleAsync(request);
            if (result.TransitionKey != PasswordLoginFlowHandler.SuccessTransitionKey && result.TransitionKey != PasswordLoginFlowHandler.RejectedTransitionKey && result.TransitionKey != PasswordLoginFlowHandler.LockedOutTransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }

        public async Task<InvokeResult> ChangePasswordAsync(ChangePassword request, EntityHeader organization, EntityHeader user)
        {
            if (_passwordChangeHandler == null)
                throw new InvalidOperationException("Password change flow handler is not configured.");

            var result = await _passwordChangeHandler.HandleAsync(new PasswordChangeFlowRequest(request, organization, user));
            if (result.TransitionKey != PasswordChangeFlowHandler.SuccessTransitionKey && result.TransitionKey != PasswordChangeFlowHandler.FailedTransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }

        public async Task<InvokeResult> RequestPasswordRecoveryAsync(SendResetPasswordLink request)
        {
            var result = await _passwordRecoveryRequestHandler.HandleAsync(new PasswordRecoveryRequestFlowRequest(request));
            if (result.TransitionKey != PasswordRecoveryRequestFlowHandler.TransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }

        public async Task<InvokeResult<string>> VerifyPasswordRecoveryAsync(VerifyPasswordResetCode request)
        {
            if (_passwordRecoveryVerificationHandler == null)
                throw new InvalidOperationException("Password recovery verification flow handler is not configured.");

            var result = await _passwordRecoveryVerificationHandler.HandleAsync(new PasswordRecoveryVerificationFlowRequest(request));
            if (result.TransitionKey != PasswordRecoveryVerificationFlowHandler.AcceptedTransitionKey && result.TransitionKey != PasswordRecoveryVerificationFlowHandler.RejectedTransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }

        public async Task<InvokeResult> CompletePasswordRecoveryAsync(ResetPassword request)
        {
            if (_passwordRecoveryCompletionHandler == null)
                throw new InvalidOperationException("Password recovery completion flow handler is not configured.");

            var result = await _passwordRecoveryCompletionHandler.HandleAsync(new PasswordRecoveryCompletionFlowRequest(request));
            if (result.TransitionKey != PasswordRecoveryCompletionFlowHandler.CompletedTransitionKey && result.TransitionKey != PasswordRecoveryCompletionFlowHandler.RejectedTransitionKey)
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

            return result.PublicResult;
        }

        public async Task<InvokeResult> VerifyEmailAsync(ConfirmEmail request, EntityHeader user)
        {
            if (_emailVerificationHandler == null)
                throw new InvalidOperationException("Email verification flow handler is not configured.");

            var result = await _emailVerificationHandler.HandleAsync(new EmailVerificationFlowRequest(request, user));
            if (result.TransitionKey != EmailVerificationFlowHandler.AcceptedTransitionKey && result.TransitionKey != EmailVerificationFlowHandler.RejectedTransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }
    }
}
