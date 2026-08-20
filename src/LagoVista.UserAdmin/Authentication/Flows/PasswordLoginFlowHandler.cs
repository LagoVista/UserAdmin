using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public interface IPasswordLoginFlowHandler
    {
        Task<AuthenticationFlowResult<AuthenticationResponse>> HandleAsync(AuthLoginRequest request);
    }

    [CriticalCoverage]
    public class PasswordLoginFlowHandler : IPasswordLoginFlowHandler
    {
        public const string SuccessTransitionKey = "auth.transition.password-sign-in.success";
        public const string MfaRequiredTransitionKey = "auth.transition.password-sign-in.mfa-required";
        public const string RejectedTransitionKey = "auth.transition.password-sign-in.rejected";
        public const string LockedOutTransitionKey = "auth.transition.password-sign-in.locked-out";

        private const string TotpProvider = "totp";
        private const string PasskeyProvider = "passkey";

        private readonly ISignInManager _signInManager;
        private readonly IAppUserRepo _appUserRepo;
        private readonly IAppUserPasskeyCredentialRepo _passkeyCredentialRepo;
        private readonly IMfaChallengeStore _mfaChallengeStore;
        private readonly IAppConfig _appConfig;

        // Kept for focused unit tests and compatibility with callers that only exercise transition mapping.
        public PasswordLoginFlowHandler(ISignInManager signInManager)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        public PasswordLoginFlowHandler(
            ISignInManager signInManager,
            IAppUserRepo appUserRepo,
            IAppUserPasskeyCredentialRepo passkeyCredentialRepo,
            IAppConfig appConfig,
            IMfaChallengeStore mfaChallengeStore = null)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _passkeyCredentialRepo = passkeyCredentialRepo ?? throw new ArgumentNullException(nameof(passkeyCredentialRepo));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
            _mfaChallengeStore = mfaChallengeStore;
        }

        public async Task<AuthenticationFlowResult<AuthenticationResponse>> HandleAsync(AuthLoginRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var result = await _signInManager.PasswordSignInAsync(request);
            if (result.Successful && result.Result?.AuthenticationState == AuthenticationResponseState.MfaRequired)
            {
                var appUser = await PopulateAvailableMfaProvidersAsync(request, result.Result);
                if (appUser != null && _mfaChallengeStore != null)
                {
                    var challengeResult = await _mfaChallengeStore.CreateAsync(new MfaChallenge
                    {
                        UserId = appUser.Id,
                        Email = appUser.Email,
                        AvailableProviders = result.Result.AvailableMfaProviders
                    });

                    if (!challengeResult.Successful)
                        return new AuthenticationFlowResult<AuthenticationResponse>(RejectedTransitionKey, InvokeResult<AuthenticationResponse>.FromInvokeResult(challengeResult.ToInvokeResult()));

                    result.Result.MfaChallengeId = challengeResult.Result.Id;
                }

                return new AuthenticationFlowResult<AuthenticationResponse>(MfaRequiredTransitionKey, result);
            }

            if (result.Successful)
                return new AuthenticationFlowResult<AuthenticationResponse>(SuccessTransitionKey, result);

            if (result.Errors.Any(error => error.ErrorCode == UserAdminErrorCodes.AuthUserLockedOut.Code))
                return new AuthenticationFlowResult<AuthenticationResponse>(LockedOutTransitionKey, result);

            if (result.Errors.Any(error => error.ErrorCode == UserAdminErrorCodes.AuthInvalidCredentials.Code))
                return new AuthenticationFlowResult<AuthenticationResponse>(RejectedTransitionKey, result);

            throw new InvalidOperationException("Password sign-in produced a failure that is not mapped to a canonical authentication transition.");
        }

        private async Task<Models.Users.AppUser> PopulateAvailableMfaProvidersAsync(AuthLoginRequest request, AuthenticationResponse response)
        {
            var providers = new List<string>();
            Models.Users.AppUser appUser = null;

            if (_appUserRepo != null && _passkeyCredentialRepo != null && _appConfig != null)
            {
                appUser = await _appUserRepo.FindByEmailAsync(request.Email);
                if (appUser != null)
                {
                    if (!String.IsNullOrWhiteSpace(appUser.AuthenticatorKeySecretId))
                        providers.Add(TotpProvider);

                    if (Uri.TryCreate(_appConfig.WebAddress, UriKind.Absolute, out var webAddress))
                    {
                        var passkeys = await _passkeyCredentialRepo.GetByUserAsync(appUser.Id, webAddress.Host);
                        if (passkeys?.Any() == true)
                            providers.Add(PasskeyProvider);
                    }
                }
            }

            if (providers.Count == 0 && !String.IsNullOrWhiteSpace(response.Provider))
                providers.Add(response.Provider);

            response.AvailableMfaProviders = providers.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            if (response.AvailableMfaProviders.Length == 1)
                response.Provider = response.AvailableMfaProviders[0];

            return appUser;
        }
    }
}
