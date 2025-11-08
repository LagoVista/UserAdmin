// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e46acd6e94d66a53744d6061f60035a8387178321da81dbbe7289ea831fc619f
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
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
        InvokeResult ValidateSingleUseTokenGrant(AuthRequest authRequest);
    }
}
