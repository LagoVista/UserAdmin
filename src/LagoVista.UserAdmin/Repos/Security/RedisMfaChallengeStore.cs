using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Security
{
    public class RedisMfaChallengeStore : IMfaChallengeStore
    {
        private const int DefaultTtlMinutes = 5;
        private readonly ICacheProvider _cache;

        public RedisMfaChallengeStore(ICacheProvider cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private static string GetKey(string challengeId) => $"auth:mfa:challenge:{challengeId}";

        public async Task<InvokeResult<MfaChallenge>> CreateAsync(MfaChallenge challenge)
        {
            if (challenge == null) throw new ArgumentNullException(nameof(challenge));
            if (String.IsNullOrWhiteSpace(challenge.Id)) challenge.Id = Guid.NewGuid().ToId();
            if (String.IsNullOrWhiteSpace(challenge.CreatedUtc)) challenge.CreatedUtc = DateTime.UtcNow.ToJSONString();
            if (String.IsNullOrWhiteSpace(challenge.ExpiresUtc)) challenge.ExpiresUtc = DateTime.UtcNow.AddMinutes(DefaultTtlMinutes).ToJSONString();

            await _cache.AddAsync(GetKey(challenge.Id), JsonConvert.SerializeObject(challenge));
            return InvokeResult<MfaChallenge>.Create(challenge);
        }

        public async Task<InvokeResult<MfaChallenge>> GetAsync(string challengeId)
        {
            if (String.IsNullOrWhiteSpace(challengeId))
                return InvokeResult<MfaChallenge>.FromError("mfa_challenge_required");

            var json = await _cache.GetAsync(GetKey(challengeId));
            if (String.IsNullOrWhiteSpace(json))
                return InvokeResult<MfaChallenge>.FromError("mfa_challenge_not_found");

            var challenge = JsonConvert.DeserializeObject<MfaChallenge>(json);
            if (challenge == null)
                return InvokeResult<MfaChallenge>.FromError("mfa_challenge_invalid");

            if (challenge.IsExpired)
            {
                await _cache.RemoveAsync(GetKey(challengeId));
                return InvokeResult<MfaChallenge>.FromError("mfa_challenge_expired");
            }

            return InvokeResult<MfaChallenge>.Create(challenge);
        }

        public async Task<InvokeResult<MfaChallenge>> ConsumeAsync(string challengeId)
        {
            var result = await GetAsync(challengeId);
            if (!result.Successful) return result;

            await _cache.RemoveAsync(GetKey(challengeId));
            return result;
        }
    }
}
