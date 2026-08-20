using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication
{
    [CriticalCoverage]
    public class MfaChallengeFlowService : IMfaChallengeFlowService
    {
        private readonly IMfaChallengeStore _mfaChallengeStore;

        public MfaChallengeFlowService(IMfaChallengeStore mfaChallengeStore)
        {
            _mfaChallengeStore = mfaChallengeStore ?? throw new ArgumentNullException(nameof(mfaChallengeStore));
        }

        public async Task<InvokeResult<MfaChallenge>> ValidateAsync(string challengeId, string provider, string email = null)
        {
            if (String.IsNullOrWhiteSpace(challengeId) || String.IsNullOrWhiteSpace(provider))
                return InvokeResult<MfaChallenge>.FromError("mfa_challenge_invalid");

            var challengeResult = await _mfaChallengeStore.GetAsync(challengeId);
            if (!challengeResult.Successful || challengeResult.Result == null)
                return InvokeResult<MfaChallenge>.FromError("mfa_challenge_invalid");

            var challenge = challengeResult.Result;
            if (challenge.AvailableProviders?.Any(available => String.Equals(available, provider, StringComparison.OrdinalIgnoreCase)) != true)
                return InvokeResult<MfaChallenge>.FromError("mfa_provider_not_available");

            if (!String.IsNullOrWhiteSpace(email) && !String.Equals(email.Trim(), challenge.Email, StringComparison.OrdinalIgnoreCase))
                return InvokeResult<MfaChallenge>.FromError("mfa_challenge_identity_mismatch");

            if (String.IsNullOrWhiteSpace(challenge.UserId))
                return InvokeResult<MfaChallenge>.FromError("mfa_challenge_identity_missing");

            return InvokeResult<MfaChallenge>.Create(challenge);
        }

        public async Task<InvokeResult<MfaChallenge>> ConsumeAsync(string challengeId, string provider, string email = null)
        {
            var validationResult = await ValidateAsync(challengeId, provider, email);
            if (!validationResult.Successful)
                return validationResult;

            var consumeResult = await _mfaChallengeStore.ConsumeAsync(challengeId);
            if (!consumeResult.Successful || consumeResult.Result == null)
                return InvokeResult<MfaChallenge>.FromError("mfa_challenge_invalid");

            return consumeResult;
        }
    }
}
