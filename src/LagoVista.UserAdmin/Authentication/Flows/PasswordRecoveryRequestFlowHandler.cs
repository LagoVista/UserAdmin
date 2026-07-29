using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class PasswordRecoveryRequestFlowHandler : IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>
    {
        public const string TransitionKey = "auth.transition.recovery.request";

        private readonly IPasswordManager _passwordManager;

        public PasswordRecoveryRequestFlowHandler(IPasswordManager passwordManager)
        {
            _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(PasswordRecoveryRequestFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _passwordManager.SendResetPasswordLinkAsync(request.Request);
            return new AuthenticationFlowResult(TransitionKey, result);
        }
    }
}
