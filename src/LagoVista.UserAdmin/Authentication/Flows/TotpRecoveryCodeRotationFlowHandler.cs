using LagoVista.UserAdmin.Interfaces.Managers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class TotpRecoveryCodeRotationFlowHandler : ITotpRecoveryCodeRotationFlowHandler
    {
        public const string SuccessTransitionKey = "auth.transition.totp-management.rotate-recovery-codes-success";

        private readonly IAppUserMfaManager _mfaManager;

        public TotpRecoveryCodeRotationFlowHandler(IAppUserMfaManager mfaManager)
        {
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
        }

        public async Task<AuthenticationFlowResult<List<string>>> HandleAsync(TotpManagementFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Operation != TotpManagementOperation.RotateRecoveryCodes) throw new InvalidOperationException($"Unsupported TOTP management operation [{request.Operation}].");

            var result = await _mfaManager.RotateRecoveryCodesAsync(request.UserId, request.Organization, request.User);
            return new AuthenticationFlowResult<List<string>>(SuccessTransitionKey, result);
        }
    }
}
