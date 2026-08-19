using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class TotpTurnOffFlowHandler : ITotpTurnOffFlowHandler
    {
        public const string SuccessTransitionKey = "auth.transition.totp-management.disable-success";

        private readonly IAppUserMfaManager _mfaManager;

        public TotpTurnOffFlowHandler(IAppUserMfaManager mfaManager)
        {
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(TotpManagementFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Operation != TotpManagementOperation.TurnOff) throw new InvalidOperationException($"Unsupported TOTP management operation [{request.Operation}].");

            var result = await _mfaManager.DisableMfaAsync(request.UserId, request.Organization, request.User);
            return new AuthenticationFlowResult(SuccessTransitionKey, result);
        }
    }
}
