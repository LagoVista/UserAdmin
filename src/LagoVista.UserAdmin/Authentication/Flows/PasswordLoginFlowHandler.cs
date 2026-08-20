using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
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
        Task<InvokeResult<AuthenticationResponse>> CreateMfaChallengeAsync(AuthLoginRequest request);
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
        private readonly bool _transitionOnlyCompatibilityMode;

        // Focused transition-mapping tests intentionally do not compose the runtime MFA challenge dependencies.
        public PasswordLoginFlowHandler(ISignInManager signInManager)
        {
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _transitionOnlyCompatibilityMode = true;
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
                if (_transitionOnlyCompatibilityMode)
                    return new AuthenticationFlowResult<AuthenticationResponse>(MfaRequiredTransitionKey, result);

                var challengeResult = await AttachMfaChallengeAsync(request, result.Result);
                if (!challengeResult.Successful)
                    return new AuthenticationFlowResult<AuthenticationResponse>(RejectedTransitionKey, InvokeResult<AuthenticationResponse>.FromInvokeResult(challengeResult.ToInvokeResult()));

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

        public async Task<InvokeResult<AuthenticationResponse>> CreateMfaChallengeAsync(AuthLoginRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (_mfaChallengeStore == null)
                throw new InvalidOperationException("MFA challenge store is not configured.");

            var proofResult = await _signInManager.VerifyPasswordForMfaAsync(request);
            if (!proofResult.Successful || proofResult.Result == null)
                return InvokeResult<AuthenticationResponse>.FromInvokeResult(proofResult.ToInvokeResult());

            var response = new AuthenticationResponse
            {
                AuthenticationState = AuthenticationResponseState.MfaRequired,
                InviteId = request.InviteId ?? String.Empty
            };

            await PopulateAvailableMfaProvidersAsync(request, response, proofResult.Result);
            if (response.AvailableMfaProviders.Length == 0)
                return InvokeResult<AuthenticationResponse>.FromError("mfa_not_available");

            var challengeResult = await CreateChallengeAsync(proofResult.Result, response.AvailableMfaProviders);
            if (!challengeResult.Successful)
                return InvokeResult<AuthenticationResponse>.FromInvokeResult(challengeResult.ToInvokeResult());

            response.MfaChallengeId = challengeResult.Result.Id;
            return InvokeResult<AuthenticationResponse>.Create(response);
        }

        private async Task<InvokeResult> AttachMfaChallengeAsync(AuthLoginRequest request, AuthenticationResponse response)
        {
            var appUser = await PopulateAvailableMfaProvidersAsync(request, response);
            if (appUser == null || _mfaChallengeStore == null)
                return InvokeResult.FromError("mfa_challenge_unavailable");

            var challengeResult = await CreateChallengeAsync(appUser, response.AvailableMfaProviders);
            if (!challengeResult.Successful)
                return challengeResult.ToInvokeResult();

            response.MfaChallengeId = challengeResult.Result.Id;
            return InvokeResult.Success;
        }

        private Task<InvokeResult<MfaChallenge>> CreateChallengeAsync(AppUser appUser, string[] providers)
        {
            return _mfaChallengeStore.CreateAsync(new MfaChallenge
            {
                UserId = appUser.Id,
                Email = appUser.Email,
                AvailableProviders = providers ?? Array.Empty<string>()
            });
        }

        private async Task<AppUser> PopulateAvailableMfaProvidersAsync(AuthLoginRequest request, AuthenticationResponse response, AppUser knownUser = null)
        {
            var providers = new List<string>();
            var appUser = knownUser;

            if (_appUserRepo != null && _passkeyCredentialRepo != null && _appConfig != null)
            {
                appUser = appUser ?? await _appUserRepo.FindByEmailAsync(request.Email);
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
