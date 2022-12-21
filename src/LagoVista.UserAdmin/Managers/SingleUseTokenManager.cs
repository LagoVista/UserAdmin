using LagoVista.Core;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class SingleUseTokenManager : ISingleUseTokenManager
    {
        private readonly ISingleUseTokenRepo _tokenRepo;

        internal static readonly char[] chars =
           "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

        public static string GetUniqueKey(int size)
        {
            byte[] data = new byte[4 * size];
            
            var generator = new Random();
            generator.NextBytes(data);

            StringBuilder result = new StringBuilder(size);
            for (int i = 0; i < size; i++)
            {
                var rnd = BitConverter.ToUInt32(data, i * 4);
                var idx = rnd % chars.Length;

                result.Append(chars[idx]);
            }

            return result.ToString();
        }


        public SingleUseTokenManager(ISingleUseTokenRepo tokenRepo)
        {
            _tokenRepo = tokenRepo ?? throw new ArgumentNullException(nameof(tokenRepo));
        }

        public async Task<InvokeResult<SingleUseToken>> CreateAsync(string userId, TimeSpan? expires = null)
        {
            if (!expires.HasValue)
            {
                expires = TimeSpan.FromSeconds(60);
            }

            var suToken = new SingleUseToken()
            {
                UserId = userId,
                Expires = DateTime.UtcNow.Add(expires.Value).ToJSONString(),
                Token = GetUniqueKey(64)
            };

            await _tokenRepo.StoreAsync(suToken);

            return InvokeResult<SingleUseToken>.Create(suToken);
        }

        public async Task<InvokeResult> ValidationAsync(string userId, string tokenId)
        {
            var suToken = await _tokenRepo.RetreiveAsync(userId, tokenId);

            if (suToken == null)
                return InvokeResult.FromError("Could not find single use token");

          
            var expireTimeStamp = suToken.Expires.ToDateTime();

            var now = DateTime.UtcNow;

            var expired = now < expireTimeStamp;

            if (expired)
                return InvokeResult.FromError($"Single Use Token Expired - Expires: {expireTimeStamp} Now: {now}");

            return InvokeResult.Success;
        }
    }
}
