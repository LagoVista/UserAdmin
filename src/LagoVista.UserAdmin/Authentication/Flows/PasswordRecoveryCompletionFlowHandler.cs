using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class PasswordRecoveryCompletionFlowHandler : IAuthenticationFlowHandler<PasswordRecoveryCompletionFlowRequest>
    {
        public const string TransitionKey = "auth.transition.recovery.complete";

        private readonly IPasswordManager _passwordManager;

        public PasswordRecoveryCompletionFlowHandler(IPasswordManager passwordManager)
        {
            _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(PasswordRecoveryCompletionFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _passwordManager.ResetPasswordAsync(request.Request);
            return new AuthenticationFlowResult(TransitionKey, result);
        }
    }
}
