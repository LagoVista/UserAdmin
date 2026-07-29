using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class PasswordLoginFlowHandler : IAuthenticationFlowHandler<PasswordLoginFlowRequest>
    {
        public const string TransitionKey = "auth.transition.login.password";

        private readonly ISignInManager _signInManager;

        public PasswordLoginFlowHandler(ISignInManager signInManager)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(PasswordLoginFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _signInManager.PasswordSignInAsync(request.Request);
            return new AuthenticationFlowResult(TransitionKey, result);
        }
    }
}
