using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public interface ITotpAuthenticationFlowHandler
    {
        Task<AuthenticationFlowResult<AppUser>> HandleAsync(TotpSignInRequest request);
    }

    [CriticalCoverage]
    public class TotpAuthenticationFlowHandler : ITotpAuthenticationFlowHandler
    {
        public const string SuccessTransitionKey = "auth.transition.totp-sign-in.success";
        public const string RejectedTransitionKey = "auth.transition.totp-sign-in.rejected";

        private readonly IAppUserRepo _appUserRepo;
        private readonly IAppUserMfaManager _mfaManager;
        private readonly IAppConfig _appConfig;

        public TotpAuthenticationFlowHandler(IAppUserRepo appUserRepo, IAppUserMfaManager mfaManager, IAppConfig appConfig)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        public async Task<AuthenticationFlowResult<AppUser>> HandleAsync(TotpSignInRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var email = request.Email?.Trim();
            if (String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(request.Totp))
                return Rejected();

            var appUser = await _appUserRepo.FindByEmailAsync(email);
            if (appUser == null)
                return Rejected();

            var verifyResult = await _mfaManager.VerifyTotpAsync(appUser.Id, request.Totp, false, _appConfig.SystemOwnerOrg, appUser.ToEntityHeader());
            if (!verifyResult.Successful)
                return Rejected();

            return new AuthenticationFlowResult<AppUser>(SuccessTransitionKey, InvokeResult<AppUser>.Create(appUser));
        }

        private static AuthenticationFlowResult<AppUser> Rejected()
        {
            return new AuthenticationFlowResult<AppUser>(RejectedTransitionKey, InvokeResult<AppUser>.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage()));
        }
    }
}
