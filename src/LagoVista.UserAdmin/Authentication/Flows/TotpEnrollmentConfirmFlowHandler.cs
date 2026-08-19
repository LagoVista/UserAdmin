using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public sealed class TotpEnrollmentConfirmFlowHandler : IAuthenticationFlowHandler<TotpEnrollmentConfirmFlowRequest, List<string>>
    {
        public const string SuccessTransitionKey = "auth.transition.totp-enrollment.success";

        private readonly IAppUserMfaManager _mfaManager;

        public TotpEnrollmentConfirmFlowHandler(IAppUserMfaManager mfaManager)
        {
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
        }

        public async Task<AuthenticationFlowResult<List<string>>> HandleAsync(TotpEnrollmentConfirmFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _mfaManager.ConfirmTotpEnrollmentAsync(request.UserId, request.Totp, request.Organization, request.User);
            return new AuthenticationFlowResult<List<string>>(SuccessTransitionKey, result);
        }
    }
}
