using LagoVista.Core.Interfaces;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    internal class OAuthChallengeStore : IOAuthChallengeStore
    {
        private readonly ICacheProvider _cache;

        public OAuthChallengeStore(ICacheProvider cache)
        {
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        private string GetKey(string codeId)
        {
            return $"mobile-oauth:{codeId}";
        }

        public async Task CreateAsync(MobileOAuthPendingAuth packet)
        {
            if (packet == null) throw new ArgumentNullException(nameof(packet));

            await _cache.AddAsync(GetKey(packet.Code), JsonConvert.SerializeObject(packet), TimeSpan.FromMinutes(15));
        }

        public async Task<InvokeResult<MobileOAuthPendingAuth>> GetAsync(string codeId)
        {
            if (String.IsNullOrEmpty(codeId)) throw new ArgumentNullException(nameof(codeId));

            var json = await _cache.GetAsync(GetKey(codeId));
            if (String.IsNullOrEmpty(json)) return InvokeResult<MobileOAuthPendingAuth>.FromError("code_not_fou");

            var challenge = JsonConvert.DeserializeObject<MobileOAuthPendingAuth>(json);
            return InvokeResult<MobileOAuthPendingAuth>.Create(challenge);
        }

        public async Task<InvokeResult<MobileOAuthPendingAuth>> ConsumeAsync(string codeId)
        {
            var getResult = await GetAsync(codeId);
            if (!getResult.Successful) return getResult;
            await _cache.RemoveAsync(GetKey(codeId));
            return getResult;
        }
    }
}
