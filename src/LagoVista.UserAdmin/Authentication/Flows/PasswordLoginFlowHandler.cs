using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Resources;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public interface IPasswordLoginFlowHandler
    {
        Task<AuthenticationFlowResult<AuthenticationResponse>> HandleAsync(AuthLoginRequest request);
    }

    [CriticalCoverage]
    public class PasswordLoginFlowHandler : IPasswordLoginFlowHandler
    {
        public const string SuccessTransitionKey = "auth.transition.password-sign-in.success";
        public const string MfaRequiredTransitionKey = "auth.transition.password-sign-in.mfa-required";
        public const string RejectedTransitionKey = "auth.transition.password-sign-in.rejected";
        public const string LockedOutTransitionKey = "auth.transition.password-sign-in.locked-out";

        private readonly ISignInManager _signInManager;

        public PasswordLoginFlowHandler(ISignInManager signInManager)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        public async Task<AuthenticationFlowResult<AuthenticationResponse>> HandleAsync(AuthLoginRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _signInManager.PasswordSignInAsync(request);
            if (result.Successful && result.Result?.AuthenticationState == AuthenticationResponseState.MfaRequired)
                return new AuthenticationFlowResult<AuthenticationResponse>(MfaRequiredTransitionKey, result);

            if (result.Successful)
                return new AuthenticationFlowResult<AuthenticationResponse>(SuccessTransitionKey, result);

            if (result.Errors.Any(error => error.ErrorCode == UserAdminErrorCodes.AuthUserLockedOut.Code))
                return new AuthenticationFlowResult<AuthenticationResponse>(LockedOutTransitionKey, result);

            if (result.Errors.Any(error => error.ErrorCode == UserAdminErrorCodes.AuthInvalidCredentials.Code))
                return new AuthenticationFlowResult<AuthenticationResponse>(RejectedTransitionKey, result);

            throw new InvalidOperationException("Password sign-in produced a failure that is not mapped to a canonical authentication transition.");
        }
    }
}
