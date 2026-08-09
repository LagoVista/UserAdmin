using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class PasswordRecoveryCompletionFlowHandler : IAuthenticationFlowHandler<PasswordRecoveryCompletionFlowRequest>
    {
        public const string CompletedTransitionKey = "auth.transition.password-recovery.completed";
        public const string RejectedTransitionKey = "auth.transition.password-recovery.completion-rejected";

        private readonly IPasswordManager _passwordManager;

        public PasswordRecoveryCompletionFlowHandler(IPasswordManager passwordManager)
        {
            _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(PasswordRecoveryCompletionFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _passwordManager.ResetPasswordAsync(request.Request);
            var transitionKey = result.Successful ? CompletedTransitionKey : RejectedTransitionKey;
            return new AuthenticationFlowResult(transitionKey, result);
        }
    }
}
