using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class EmailVerificationFlowHandler : IAuthenticationFlowHandler<EmailVerificationFlowRequest>
    {
        public const string AcceptedTransitionKey = "auth.transition.email-verification.code-accepted";
        public const string RejectedTransitionKey = "auth.transition.email-verification.code-rejected";

        private readonly IUserVerficationManager _userVerificationManager;

        public EmailVerificationFlowHandler(IUserVerficationManager userVerificationManager)
        {
            _userVerificationManager = userVerificationManager ?? throw new ArgumentNullException(nameof(userVerificationManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(EmailVerificationFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _userVerificationManager.ValidateEmailAsync(request.Request, request.User);
            return new AuthenticationFlowResult(result.Successful ? AcceptedTransitionKey : RejectedTransitionKey, result);
        }
    }
}
