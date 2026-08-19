using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
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
        private readonly IAuthenticationFlowHandler<EmailVerificationSendFlowRequest, EmailVerificationSendResult> _emailVerificationSendHandler;
        private readonly IAuthenticationFlowHandler<PasswordRecoveryVerificationFlowRequest, string> _passwordRecoveryVerificationHandler;
        private readonly IAuthenticationFlowHandler<PasswordChangeFlowRequest> _passwordChangeHandler;
        private readonly IAuthenticationFlowHandler<TotpEnrollmentBeginFlowRequest, AppUserTotpEnrollmentInfo> _totpEnrollmentBeginHandler;
        private readonly IAuthenticationFlowHandler<TotpEnrollmentConfirmFlowRequest, List<string>> _totpEnrollmentConfirmHandler;
        private readonly ITotpTurnOffFlowHandler _totpTurnOffHandler;
        private readonly ITotpRecoveryCodeRotationFlowHandler _totpRecoveryCodeRotationHandler;

        public AuthenticationFlowService(IPasswordLoginFlowHandler passwordLoginHandler, IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest> passwordRecoveryRequestHandler, IAuthenticationFlowHandler<PasswordRecoveryCompletionFlowRequest> passwordRecoveryCompletionHandler = null, IAuthenticationFlowHandler<InvitationAcceptanceFlowRequest, AcceptInviteResponse> invitationAcceptanceHandler = null, IAuthenticationFlowHandler<EmailVerificationFlowRequest> emailVerificationHandler = null, IAuthenticationFlowHandler<PasswordRecoveryVerificationFlowRequest, string> passwordRecoveryVerificationHandler = null, IAuthenticationFlowHandler<PasswordChangeFlowRequest> passwordChangeHandler = null, IAuthenticationFlowHandler<EmailVerificationSendFlowRequest, EmailVerificationSendResult> emailVerificationSendHandler = null, ITotpTurnOffFlowHandler totpTurnOffHandler = null, ITotpRecoveryCodeRotationFlowHandler totpRecoveryCodeRotationHandler = null, IAuthenticationFlowHandler<TotpEnrollmentBeginFlowRequest, AppUserTotpEnrollmentInfo> totpEnrollmentBeginHandler = null, IAuthenticationFlowHandler<TotpEnrollmentConfirmFlowRequest, List<string>> totpEnrollmentConfirmHandler = null)
        {
            _passwordLoginHandler = passwordLoginHandler ?? throw new ArgumentNullException(nameof(passwordLoginHandler));
            _passwordRecoveryRequestHandler = passwordRecoveryRequestHandler ?? throw new ArgumentNullException(nameof(passwordRecoveryRequestHandler));
            _passwordRecoveryCompletionHandler = passwordRecoveryCompletionHandler;
            _invitationAcceptanceHandler = invitationAcceptanceHandler;
            _emailVerificationHandler = emailVerificationHandler;
            _passwordRecoveryVerificationHandler = passwordRecoveryVerificationHandler;
            _passwordChangeHandler = passwordChangeHandler;
            _emailVerificationSendHandler = emailVerificationSendHandler;
            _totpTurnOffHandler = totpTurnOffHandler;
            _totpRecoveryCodeRotationHandler = totpRecoveryCodeRotationHandler;
            _totpEnrollmentBeginHandler = totpEnrollmentBeginHandler;
            _totpEnrollmentConfirmHandler = totpEnrollmentConfirmHandler;
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

        public async Task<InvokeResult<EmailVerificationSendResult>> SendEmailVerificationCodeAsync(EntityHeader user)
        {
            if (_emailVerificationSendHandler == null)
                throw new InvalidOperationException("Email verification send flow handler is not configured.");

            var result = await _emailVerificationSendHandler.HandleAsync(new EmailVerificationSendFlowRequest(user));
            if (!result.PublicResult.Successful)
                return result.PublicResult;

            if (result.TransitionKey != EmailVerificationSendFlowHandler.SentTransitionKey && result.TransitionKey != EmailVerificationSendFlowHandler.ResentTransitionKey && result.TransitionKey != EmailVerificationSendFlowHandler.ThrottledTransitionKey)
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

        public async Task<InvokeResult<AppUserTotpEnrollmentInfo>> BeginTotpEnrollmentAsync(string userId, EntityHeader organization, EntityHeader user)
        {
            if (_totpEnrollmentBeginHandler == null)
                throw new InvalidOperationException("TOTP enrollment begin flow handler is not configured.");

            var result = await _totpEnrollmentBeginHandler.HandleAsync(new TotpEnrollmentBeginFlowRequest(userId, organization, user));
            if (result.TransitionKey != TotpEnrollmentBeginFlowHandler.SuccessTransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }

        public async Task<InvokeResult<List<string>>> ConfirmTotpEnrollmentAsync(string userId, string totp, EntityHeader organization, EntityHeader user)
        {
            if (_totpEnrollmentConfirmHandler == null)
                throw new InvalidOperationException("TOTP enrollment confirm flow handler is not configured.");

            var result = await _totpEnrollmentConfirmHandler.HandleAsync(new TotpEnrollmentConfirmFlowRequest(userId, totp, organization, user));
            if (result.TransitionKey != TotpEnrollmentConfirmFlowHandler.SuccessTransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }

        public async Task<InvokeResult> TurnOffTotpAsync(string userId, EntityHeader organization, EntityHeader user)
        {
            if (_totpTurnOffHandler == null)
                throw new InvalidOperationException("TOTP turn-off flow handler is not configured.");

            var result = await _totpTurnOffHandler.HandleAsync(new TotpManagementFlowRequest(userId, TotpManagementOperation.TurnOff, organization, user));
            if (result.TransitionKey != TotpTurnOffFlowHandler.SuccessTransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }

        public async Task<InvokeResult<List<string>>> RotateTotpRecoveryCodesAsync(string userId, EntityHeader organization, EntityHeader user)
        {
            if (_totpRecoveryCodeRotationHandler == null)
                throw new InvalidOperationException("TOTP recovery-code rotation flow handler is not configured.");

            var result = await _totpRecoveryCodeRotationHandler.HandleAsync(new TotpManagementFlowRequest(userId, TotpManagementOperation.RotateRecoveryCodes, organization, user));
            if (result.TransitionKey != TotpRecoveryCodeRotationFlowHandler.SuccessTransitionKey)
                throw new InvalidOperationException($"Authentication flow emitted unsupported transition [{result.TransitionKey}].");

            return result.PublicResult;
        }
    }
}
