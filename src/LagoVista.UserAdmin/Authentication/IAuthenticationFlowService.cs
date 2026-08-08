using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
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
        Task<InvokeResult> VerifyEmailAsync(ConfirmEmail request, EntityHeader user);
    }
}
