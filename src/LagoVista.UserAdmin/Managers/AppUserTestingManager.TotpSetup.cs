using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Testing;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Interfaces.REpos.Account;
using LagoVista.UserAdmin.Models.Testing;
using OtpNet;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public partial class AppUserTestingManager
    {
        private IAppUserMfaManager _mfaManager;

        public AppUserTestingManager(IAppUserTestingDslRepo dslStore,
                                     IAppUserTestRunRepo testRunStore,
                                     IDependencyManager depManager,
                                     ISecurity security,
                                     IAdminLogger adminLogger,
                                     IOrganizationManager orgManager,
                                     IAppUserRepo appUserRepo,
                                     IAppUserManager appuUserManager,
                                     ISignInManager signInManager,
                                     IAdminLogger logger,
                                     IAuthViewRepo authViewRepo,
                                     IAuthenticationLogManager authLogMgr,
                                     IUserManager userManager,
                                     IMagicLinkManager magicLinkManager,
                                     IUserRegistrationManager userRegistrationManager,
                                     IPasswordResetCodeRepo passwordResetCodeRepo,
                                     IEmailVerificationCodeRepo emailVerificationCodeRepo,
                                     ITestArtifactStorage testArtifactStorage,
                                     IAppConfig appConfig,
                                     IAppUserMfaManager mfaManager)
            : this(dslStore, testRunStore, depManager, security, adminLogger, orgManager, appUserRepo, appuUserManager, signInManager, logger, authViewRepo, authLogMgr, userManager, magicLinkManager, userRegistrationManager, passwordResetCodeRepo, emailVerificationCodeRepo, testArtifactStorage, appConfig)
        {
            _mfaManager = mfaManager ?? throw new ArgumentNullException(nameof(mfaManager));
        }

        private async Task<InvokeResult> ApplyTotpSetupAsync(AppUserTestScenario scenario, TestUserCredentials credentials, EntityHeader org, EntityHeader user)
        {
            var state = scenario?.PreConditions?.HasTOTP;
            if (state == null || state.Value == SetCondition.DontCare) return InvokeResult.Success;
            if (_mfaManager == null) return InvokeResult.FromError("TotpTestSetupUnavailable", "TOTP test setup requires IAppUserMfaManager.");

            var appUser = await _appUserRepo.FindByIdAsync(TestUserSeed.User.Id);
            if (appUser == null) return InvokeResult.FromError("TotpTestUserMissing", "TOTP setup requires the canonical test user to exist.");

            if (state.Value == SetCondition.NotSet)
            {
                if (!String.IsNullOrWhiteSpace(appUser.AuthenticatorKeySecretId) || !String.IsNullOrWhiteSpace(appUser.RecoveryCodesSecretId) || appUser.TwoFactorEnabled)
                {
                    var turnOffResult = await _mfaManager.DisableMfaAsync(appUser.Id, org, user);
                    if (!turnOffResult.Successful) return turnOffResult;
                }
                return InvokeResult.Success;
            }

            var beginResult = await _mfaManager.BeginTotpEnrollmentAsync(appUser.Id, org, user);
            if (!beginResult.Successful) return beginResult.ToInvokeResult();

            var secretBytes = Base32Encoding.ToBytes(beginResult.Result.Secret);
            var totp = new Totp(secretBytes, step: 30, totpSize: 6).ComputeTotp();
            var confirmResult = await _mfaManager.ConfirmTotpEnrollmentAsync(appUser.Id, totp, org, user);
            if (!confirmResult.Successful) return confirmResult.ToInvokeResult();

            credentials.UserId = appUser.Id;
            return InvokeResult.Success;
        }
    }
}
