using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Globalization;

namespace LagoVista.UserAdmin.Repos.TableStorage.ContinuityConversations
{
    internal class ContinuityConversationEntity : TableStorageEntity
    {
        public string MessageId { get; set; }
        public string Role { get; set; }
        public string Text { get; set; }
        public string CreatedUtc { get; set; }

        public static ContinuityConversationEntity FromModel(string actorId, ContinuityConversationMessage message)
        {
            return new ContinuityConversationEntity
            {
                PartitionKey = actorId,
                RowKey = $"{message.CreatedUtc.ToUniversalTime().Ticks:D19}|{message.Id}",
                MessageId = message.Id,
                Role = message.Role,
                Text = message.Text,
                CreatedUtc = message.CreatedUtc.ToUniversalTime().ToString("O")
            };
        }

        public ContinuityConversationMessage ToModel()
        {
            return new ContinuityConversationMessage
            {
                Id = MessageId,
                Role = Role,
                Text = Text,
                CreatedUtc = DateTime.Parse(CreatedUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind)
            };
        }
    }
}
