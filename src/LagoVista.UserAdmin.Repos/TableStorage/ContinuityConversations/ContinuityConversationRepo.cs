using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.TableStorage.ContinuityConversations
{
    internal class ContinuityConversationRepo : TableStorageBase<ContinuityConversationEntity>, IContinuityConversationRepo
    {
        private const int MaximumStoredMessages = 40;

        public ContinuityConversationRepo(IUserAdminSettings settings, IAdminLogger logger) : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
        }

        protected override string GetTableName()
        {
            return "continuityconversation";
        }

        public async Task<IEnumerable<ContinuityConversationMessage>> GetAsync(string actorId)
        {
            if (String.IsNullOrWhiteSpace(actorId)) throw new ArgumentNullException(nameof(actorId));
            var entities = await GetByPartitionIdAsync(actorId);
            return entities.OrderBy(entity => entity.RowKey).TakeLast(MaximumStoredMessages).Select(entity => entity.ToModel()).ToList();
        }

        public async Task AppendAsync(string actorId, IEnumerable<ContinuityConversationMessage> messages)
        {
            if (String.IsNullOrWhiteSpace(actorId)) throw new ArgumentNullException(nameof(actorId));
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            foreach (var message in messages) await InsertAsync(ContinuityConversationEntity.FromModel(actorId, message));

            var entities = (await GetByPartitionIdAsync(actorId)).OrderBy(entity => entity.RowKey).ToList();
            foreach (var entity in entities.Take(Math.Max(0, entities.Count - MaximumStoredMessages))) await RemoveAsync(entity);
        }

        public Task ClearAsync(string actorId)
        {
            if (String.IsNullOrWhiteSpace(actorId)) throw new ArgumentNullException(nameof(actorId));
            return RemoveByPartitionKeyAsync(actorId);
        }
    }
}
