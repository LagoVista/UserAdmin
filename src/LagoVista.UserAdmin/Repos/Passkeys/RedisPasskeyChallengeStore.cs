using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Passkeys
{
    public class RedisPasskeyChallengeStore : IPasskeyChallengeStore
    {
        private const int DefaultTtlMinutes = 5;

        private readonly ICacheProvider _cache;
        private readonly IAdminLogger _logger;

        public RedisPasskeyChallengeStore(ICacheProvider cache, IAdminLogger logger)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private string GetKey(string challengeId)
        {
            return $"passkey:challenge:{challengeId}";
        }

        public async Task<InvokeResult<PasskeyChallenge>> CreateAsync(PasskeyChallenge challenge)
        {
            if (challenge == null) throw new ArgumentNullException(nameof(challenge));
            if (String.IsNullOrEmpty(challenge.Id)) challenge.Id = Guid.NewGuid().ToId();
            if (String.IsNullOrEmpty(challenge.CreatedUtc)) challenge.CreatedUtc = DateTime.UtcNow.ToJSONString();
            if (String.IsNullOrEmpty(challenge.ExpiresUtc)) challenge.ExpiresUtc = DateTime.UtcNow.AddMinutes(DefaultTtlMinutes).ToJSONString();

            await _cache.AddAsync(GetKey(challenge.Id), JsonConvert.SerializeObject(challenge));
            return InvokeResult<PasskeyChallenge>.Create(challenge);
        }

        public async Task<InvokeResult<PasskeyChallenge>> GetAsync(string challengeId)
        {
            if (String.IsNullOrEmpty(challengeId)) throw new ArgumentNullException(nameof(challengeId));

            var json = await _cache.GetAsync(GetKey(challengeId));
            if (String.IsNullOrEmpty(json)) return InvokeResult<PasskeyChallenge>.FromError("challenge_not_found");

            var challenge = JsonConvert.DeserializeObject<PasskeyChallenge>(json);
            if (challenge == null) return InvokeResult<PasskeyChallenge>.FromError("challenge_invalid");
            if (challenge.IsExpired) return InvokeResult<PasskeyChallenge>.FromError("challenge_expired");

            return InvokeResult<PasskeyChallenge>.Create(challenge);
        }

        public async Task<InvokeResult<PasskeyChallenge>> ConsumeAsync(string challengeId)
        {
            var getResult = await GetAsync(challengeId);
            if (!getResult.Successful) return getResult;

            await _cache.RemoveAsync(GetKey(challengeId));
            return getResult;
        }
    }
}
