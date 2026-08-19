using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication
{
    public interface IAuthenticationFlowService
    {
        Task<InvokeResult<AuthenticationResponse>> LoginWithPasswordAsync(AuthLoginRequest request);
        Task<InvokeResult> ChangePasswordAsync(ChangePassword request, EntityHeader organization, EntityHeader user);
        Task<InvokeResult> RequestPasswordRecoveryAsync(SendResetPasswordLink request);
        Task<InvokeResult<string>> VerifyPasswordRecoveryAsync(VerifyPasswordResetCode request);
        Task<InvokeResult> CompletePasswordRecoveryAsync(ResetPassword request);
        Task<InvokeResult<AcceptInviteResponse>> AcceptInvitationAsync(string inviteId, string userId);
        Task<InvokeResult<EmailVerificationSendResult>> SendEmailVerificationCodeAsync(EntityHeader user);
        Task<InvokeResult> VerifyEmailAsync(ConfirmEmail request, EntityHeader user);
        Task<InvokeResult<AppUserTotpEnrollmentInfo>> BeginTotpEnrollmentAsync(string userId, EntityHeader organization, EntityHeader user);
        Task<InvokeResult<List<string>>> ConfirmTotpEnrollmentAsync(string userId, string totp, EntityHeader organization, EntityHeader user);
        Task<InvokeResult> TurnOffTotpAsync(string userId, EntityHeader organization, EntityHeader user);
        Task<InvokeResult<List<string>>> RotateTotpRecoveryCodesAsync(string userId, EntityHeader organization, EntityHeader user);
    }
}
