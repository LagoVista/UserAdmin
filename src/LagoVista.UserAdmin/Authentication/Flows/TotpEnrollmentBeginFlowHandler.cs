using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public sealed class TotpEnrollmentBeginFlowHandler : IAuthenticationFlowHandler<TotpEnrollmentBeginFlowRequest, AppUserTotpEnrollmentInfo>
    {
        public const string SuccessTransitionKey = "auth.transition.totp-enrollment.begin";

        private readonly IAppUserMfaManager _mfaManager;

        public TotpEnrollmentBeginFlowHandler(IAppUserMfaManager mfaManager)
        {
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
        }

        public async Task<AuthenticationFlowResult<AppUserTotpEnrollmentInfo>> HandleAsync(TotpEnrollmentBeginFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _mfaManager.BeginTotpEnrollmentAsync(request.UserId, request.Organization, request.User);
            return new AuthenticationFlowResult<AppUserTotpEnrollmentInfo>(SuccessTransitionKey, result);
        }
    }
}
