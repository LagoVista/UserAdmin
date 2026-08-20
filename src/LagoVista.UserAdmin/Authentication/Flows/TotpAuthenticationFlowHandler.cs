using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Resources;
using System;
using System.Linq;
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
        private readonly IMfaChallengeStore _mfaChallengeStore;
        private readonly IAppConfig _appConfig;

        public TotpAuthenticationFlowHandler(IAppUserRepo appUserRepo, IAppUserMfaManager mfaManager, IAppConfig appConfig, IMfaChallengeStore mfaChallengeStore)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _mfaChallengeStore = mfaChallengeStore ?? throw new ArgumentNullException(nameof(mfaChallengeStore));
        }

        public async Task<AuthenticationFlowResult<AppUser>> HandleAsync(TotpSignInRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (String.IsNullOrWhiteSpace(request.MfaChallengeId) || String.IsNullOrWhiteSpace(request.Totp))
                return Rejected();

            var challengeResult = await _mfaChallengeStore.GetAsync(request.MfaChallengeId);
            if (!challengeResult.Successful || challengeResult.Result == null)
                return Rejected();

            var challenge = challengeResult.Result;
            if (challenge.AvailableProviders?.Any(provider => String.Equals(provider, "totp", StringComparison.OrdinalIgnoreCase)) != true)
                return Rejected();

            if (!String.IsNullOrWhiteSpace(request.Email) && !String.Equals(request.Email.Trim(), challenge.Email, StringComparison.OrdinalIgnoreCase))
                return Rejected();

            var appUser = await _appUserRepo.FindByIdAsync(challenge.UserId);
            if (appUser == null)
                return Rejected();

            var verifyResult = await _mfaManager.VerifyTotpAsync(appUser.Id, request.Totp, true, _appConfig.SystemOwnerOrg, appUser.ToEntityHeader());
            if (!verifyResult.Successful)
                return Rejected();

            var consumeResult = await _mfaChallengeStore.ConsumeAsync(request.MfaChallengeId);
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
