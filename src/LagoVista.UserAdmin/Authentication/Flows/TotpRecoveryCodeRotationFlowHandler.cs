using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
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
        private readonly IAppUserRepo _appUserRepo;

        public TotpRecoveryCodeRotationFlowHandler(IAppUserMfaManager mfaManager, IAppUserRepo appUserRepo)
        {
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
        }

        public async Task<AuthenticationFlowResult<List<string>>> HandleAsync(TotpManagementFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Operation != TotpManagementOperation.RotateRecoveryCodes) throw new InvalidOperationException($"Unsupported TOTP management operation [{request.Operation}].");

            var appUser = await _appUserRepo.FindByIdAsync(request.UserId);
            if (appUser == null)
                return new AuthenticationFlowResult<List<string>>(SuccessTransitionKey, InvokeResult<List<string>>.FromError("user_not_found"));

            if (!appUser.TwoFactorEnabled || String.IsNullOrWhiteSpace(appUser.AuthenticatorKeySecretId))
                return new AuthenticationFlowResult<List<string>>(SuccessTransitionKey, InvokeResult<List<string>>.FromError("mfa_not_enabled"));

            var result = await _mfaManager.RotateRecoveryCodesAsync(request.UserId, request.Organization, request.User);
            return new AuthenticationFlowResult<List<string>>(SuccessTransitionKey, result);
        }
    }
}
