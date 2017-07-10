using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IAuthRequestValidators
    {
        InvokeResult ValidateAccessTokenGrant(AuthRequest authRequest);
        InvokeResult ValidateAuthRequest(AuthRequest authRequest);
        InvokeResult ValidateRefreshTokenGrant(AuthRequest authRequest);
        Task<InvokeResult> ValidateRefreshTokenAsync(string refreshTokenId, string userId);
        InvokeResult ValidateRefreshTokenFormat(string refreshToken);
        InvokeResult ValidatePasswordChangeRequest(ChangePassword changePassword, string userId);
        InvokeResult ValidateSendPasswordLinkRequest(SendResetPasswordLink sendRestPasswordLink);
        InvokeResult ValidateResetPasswordRequest(ResetPassword resetPassword);
    }
}
