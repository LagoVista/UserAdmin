// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4ac6d271f689ba3a45bd3e96904939079566b252f91c5c1b22cfb78dbbdcd2ed
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{

    class SingleUseTokenEntity : TableStorageEntity
    {
        public string Expires { get; set; }

    
        public static SingleUseTokenEntity FromToken(SingleUseToken token)
        {
            return new SingleUseTokenEntity()
            {
                RowKey = token.Token,
                PartitionKey = token.UserId,
                Expires = token.Expires
            };
        }

        public SingleUseToken ToToken()
        {
            return new SingleUseToken()
            {
                Expires = Expires,
                Token = RowKey,
                UserId = PartitionKey
            };
        }
    }

    internal class SingleUseTokenRepo : TableStorageBase<SingleUseTokenEntity>, ISingleUseTokenRepo
    {
        public SingleUseTokenRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "singleusetoken";
        }

        public async Task<SingleUseToken> RetreiveAsync(string userId, string tokenId)
        {
            var entity = await GetAsync(userId, tokenId,  false);
            if (entity == null)
                return null;

            await RemoveAsync(userId, tokenId);

            return entity.ToToken();
        }

        public Task StoreAsync(SingleUseToken token)
        {
            return InsertAsync(SingleUseTokenEntity.FromToken(token));
        }
    }
}
