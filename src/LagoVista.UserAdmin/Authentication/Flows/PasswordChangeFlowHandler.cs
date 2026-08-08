using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class PasswordChangeFlowHandler : IAuthenticationFlowHandler<PasswordChangeFlowRequest>
    {
        public const string SuccessTransitionKey = "auth.transition.password-management.change-success";
        public const string FailedTransitionKey = "auth.transition.password-management.change-failed";

        private readonly IPasswordManager _passwordManager;

        public PasswordChangeFlowHandler(IPasswordManager passwordManager)
        {
            _passwordManager = passwordManager ?? throw new ArgumentNullException(nameof(passwordManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(PasswordChangeFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _passwordManager.ChangePasswordAsync(request.Request, request.Organization, request.User);
            return new AuthenticationFlowResult(result.Successful ? SuccessTransitionKey : FailedTransitionKey, result);
        }
    }
}
