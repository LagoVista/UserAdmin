using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class PasswordRecoveryVerificationFlowHandler : IAuthenticationFlowHandler<PasswordRecoveryVerificationFlowRequest, string>
    {
        public const string TransitionKey = "auth.transition.recovery.verify";

        private readonly IPasswordManager _passwordManager;

        public PasswordRecoveryVerificationFlowHandler(IPasswordManager passwordManager)
        {
            _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
        }

        public async Task<AuthenticationFlowResult<string>> HandleAsync(PasswordRecoveryVerificationFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _passwordManager.VerifyPasswordResetCodeAsync(request.Request);
            return new AuthenticationFlowResult<string>(TransitionKey, result);
        }
    }
}
