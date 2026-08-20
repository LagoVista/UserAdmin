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
    public interface IRecoveryCodeAuthenticationFlowHandler
    {
        Task<AuthenticationFlowResult<AppUser>> HandleAsync(RecoveryCodeSignInRequest request);
    }

    [CriticalCoverage]
    public class RecoveryCodeAuthenticationFlowHandler : IRecoveryCodeAuthenticationFlowHandler
    {
        public const string SuccessTransitionKey = "auth.transition.totp-recovery-sign-in.success";
        public const string RejectedTransitionKey = "auth.transition.totp-recovery-sign-in.rejected";

        private readonly IAppUserRepo _appUserRepo;
        private readonly IAppUserMfaManager _mfaManager;
        private readonly IAppConfig _appConfig;

        public RecoveryCodeAuthenticationFlowHandler(IAppUserRepo appUserRepo, IAppUserMfaManager mfaManager, IAppConfig appConfig)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        public async Task<AuthenticationFlowResult<AppUser>> HandleAsync(RecoveryCodeSignInRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var email = request.Email?.Trim();
            if (String.IsNullOrWhiteSpace(email) || String.IsNullOrWhiteSpace(request.RecoveryCode))
                return Rejected();

            var appUser = await _appUserRepo.FindByEmailAsync(email);
            if (appUser == null)
                return Rejected();

            var consumeResult = await _mfaManager.ConsumeRecoveryCodeAsync(
                appUser.Id,
                request.RecoveryCode,
                false,
                _appConfig.SystemOwnerOrg,
                appUser.ToEntityHeader());

            if (!consumeResult.Successful)
                return Rejected();

            return new AuthenticationFlowResult<AppUser>(SuccessTransitionKey, InvokeResult<AppUser>.Create(appUser));
        }

        private static AuthenticationFlowResult<AppUser> Rejected()
        {
            return new AuthenticationFlowResult<AppUser>(RejectedTransitionKey, InvokeResult<AppUser>.FromErrors(UserAdminErrorCodes.AuthInvalidCredentials.ToErrorMessage()));
        }
    }
}
