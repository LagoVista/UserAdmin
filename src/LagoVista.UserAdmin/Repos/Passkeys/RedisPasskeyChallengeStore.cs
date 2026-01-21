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
        
        private string GetOptionsKey(string challengeId)
        {
            return $"passkey:challenge:options:{challengeId}";
        }

        public async Task<InvokeResult<PasskeyChallengePacket>> CreateAsync(PasskeyChallengePacket packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));
            if (String.IsNullOrEmpty(packet.Challenge.Id)) packet.Challenge.Id = Guid.NewGuid().ToId();
            if (String.IsNullOrEmpty(packet.Challenge.CreatedUtc)) packet.Challenge.CreatedUtc = DateTime.UtcNow.ToJSONString();
            if (String.IsNullOrEmpty(packet.Challenge.ExpiresUtc)) packet.Challenge.ExpiresUtc = DateTime.UtcNow.AddMinutes(DefaultTtlMinutes).ToJSONString();

            await _cache.AddAsync(GetKey(packet.Challenge.Id), JsonConvert.SerializeObject(packet));
            return InvokeResult<PasskeyChallengePacket>.Create(packet);
        }

        public async Task<InvokeResult<PasskeyChallengePacket>> GetAsync(string challengeId)
        {
            if (String.IsNullOrEmpty(challengeId)) throw new ArgumentNullException(nameof(challengeId));

            var json = await _cache.GetAsync(GetKey(challengeId));
            if (String.IsNullOrEmpty(json)) return InvokeResult<PasskeyChallengePacket>.FromError("challenge_not_found");

            var challenge = JsonConvert.DeserializeObject<PasskeyChallengePacket>(json);
            if (challenge == null) return InvokeResult<PasskeyChallengePacket>.FromError("challenge_invalid");
            if (challenge.Challenge.IsExpired) return InvokeResult<PasskeyChallengePacket>.FromError("challenge_expired");

            return InvokeResult<PasskeyChallengePacket>.Create(challenge);
        }

        public async Task<InvokeResult<PasskeyChallengePacket>> ConsumeAsync(string challengeId)
        {
            var getResult = await GetAsync(challengeId);
            if (!getResult.Successful) return getResult;

            await _cache.RemoveAsync(GetKey(challengeId));
            return getResult;
        }

    }
}
