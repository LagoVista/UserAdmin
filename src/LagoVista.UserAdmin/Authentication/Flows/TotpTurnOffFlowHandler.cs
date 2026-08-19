using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    [CriticalCoverage]
    public class TotpTurnOffFlowHandler : ITotpTurnOffFlowHandler
    {
        public const string SuccessTransitionKey = "auth.transition.totp-management.disable-success";

        private readonly IAppUserMfaManager _mfaManager;
        private readonly IAppUserRepo _appUserRepo;

        public TotpTurnOffFlowHandler(IAppUserMfaManager mfaManager, IAppUserRepo appUserRepo)
        {
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
        }

        public async Task<AuthenticationFlowResult> HandleAsync(TotpManagementFlowRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (request.Operation != TotpManagementOperation.TurnOff) throw new InvalidOperationException($"Unsupported TOTP management operation [{request.Operation}].");

            var appUser = await _appUserRepo.FindByIdAsync(request.UserId);
            if (appUser == null)
                return new AuthenticationFlowResult(SuccessTransitionKey, InvokeResult.FromError("user_not_found"));

            if (!appUser.TwoFactorEnabled || String.IsNullOrWhiteSpace(appUser.AuthenticatorKeySecretId))
                return new AuthenticationFlowResult(SuccessTransitionKey, InvokeResult.FromError("mfa_not_enabled"));

            var result = await _mfaManager.DisableMfaAsync(request.UserId, request.Organization, request.User);
            return new AuthenticationFlowResult(SuccessTransitionKey, result);
        }
    }
}
