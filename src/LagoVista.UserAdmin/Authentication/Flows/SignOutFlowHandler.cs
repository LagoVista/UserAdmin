using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public class SignOutFlowHandler : ISignOutFlowHandler
    {
        public const string SuccessTransitionKey = "auth.transition.session.sign-out-success";

        private readonly ISignInManager _signInManager;
        private readonly IRefreshTokenManager _refreshTokenManager;
        private readonly IAuthenticationLogManager _authenticationLogManager;

        public SignOutFlowHandler(ISignInManager signInManager, IRefreshTokenManager refreshTokenManager, IAuthenticationLogManager authenticationLogManager)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _refreshTokenManager = refreshTokenManager ?? throw new ArgumentNullException(nameof(refreshTokenManager));
            _authenticationLogManager = authenticationLogManager ?? throw new ArgumentNullException(nameof(authenticationLogManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(SignOutFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.User == null || String.IsNullOrWhiteSpace(request.User.Id))
                return new AuthenticationFlowResult(SuccessTransitionKey, InvokeResult.FromError("Authenticated user is required for sign out."));

            if (!String.IsNullOrWhiteSpace(request.Request.RefreshToken))
                await _refreshTokenManager.RevokeRefreshTokenAsync(request.Request.RefreshToken, request.User.Id);

            await _signInManager.SignOutAsync();
            await _authenticationLogManager.AddAsync(
                AuthLogTypes.UserLogout,
                request.User.Id,
                request.User.Text,
                request.Organization?.Id,
                request.Organization?.Text);

            return new AuthenticationFlowResult(SuccessTransitionKey, InvokeResult.Success);
        }
    }
}
