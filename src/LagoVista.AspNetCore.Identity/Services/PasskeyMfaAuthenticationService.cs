using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    public class PasskeyMfaAuthenticationService : IPasskeyMfaAuthenticationService
    {
        private readonly IMfaChallengeFlowService _mfaChallengeFlowService;
        private readonly IAppUserPasskeyManager _passkeyManager;
        private readonly IAppUserRepo _appUserRepo;

        public PasskeyMfaAuthenticationService(IMfaChallengeFlowService mfaChallengeFlowService, IAppUserPasskeyManager passkeyManager, IAppUserRepo appUserRepo)
        {
            _mfaChallengeFlowService = mfaChallengeFlowService ?? throw new ArgumentNullException(nameof(mfaChallengeFlowService));
            _passkeyManager = passkeyManager ?? throw new ArgumentNullException(nameof(passkeyManager));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
        }

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginAsync(string mfaChallengeId, string passkeyUrl, EntityHeader organization, EntityHeader user)
        {
            var challenge = await _mfaChallengeFlowService.ValidateAsync(mfaChallengeId, "passkey");
            if (!challenge.Successful || challenge.Result == null)
                return InvokeResult<PasskeyBeginOptionsResponse>.FromError("passkey_mfa_challenge_invalid");

            return await _passkeyManager.BeginAuthenticationOptionsAsync(challenge.Result.UserId, true, passkeyUrl, organization, user);
        }

        public async Task<InvokeResult<AppUser>> CompleteAsync(string mfaChallengeId, PasskeyAuthenticationCompleteRequest request, EntityHeader organization, EntityHeader user)
        {
            if (request == null)
                return InvokeResult<AppUser>.FromError("passkey_request_required");

            var challenge = await _mfaChallengeFlowService.ValidateAsync(mfaChallengeId, "passkey");
            if (!challenge.Successful || challenge.Result == null)
                return InvokeResult<AppUser>.FromError("passkey_mfa_authentication_failed");

            var proof = await _passkeyManager.CompleteAuthenticationAsync(challenge.Result.UserId, request, true, organization, user);
            if (!proof.Successful)
                return InvokeResult<AppUser>.FromError("passkey_mfa_authentication_failed");

            var consumed = await _mfaChallengeFlowService.ConsumeAsync(mfaChallengeId, "passkey");
            if (!consumed.Successful || consumed.Result == null)
                return InvokeResult<AppUser>.FromError("passkey_mfa_authentication_failed");

            if (!String.Equals(consumed.Result.UserId, challenge.Result.UserId, StringComparison.Ordinal))
                return InvokeResult<AppUser>.FromError("passkey_mfa_authentication_failed");

            var appUser = await _appUserRepo.FindByIdAsync(challenge.Result.UserId);
            if (appUser == null)
                return InvokeResult<AppUser>.FromError("passkey_mfa_authentication_failed");

            return InvokeResult<AppUser>.Create(appUser);
        }
    }
}
