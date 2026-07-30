using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class EmailVerificationFlowHandler : IAuthenticationFlowHandler<EmailVerificationFlowRequest>
    {
        public const string TransitionKey = "auth.transition.email-verification.complete";

        private readonly IUserVerficationManager _userVerificationManager;

        public EmailVerificationFlowHandler(IUserVerficationManager userVerificationManager)
        {
            _userVerificationManager = userVerificationManager ?? throw new ArgumentNullException(nameof(userVerificationManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(EmailVerificationFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _userVerificationManager.ValidateEmailAsync(request.Request, request.User);
            return new AuthenticationFlowResult(TransitionKey, result);
        }
    }
}
