using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal class AnonymousVisitorStateIndexEntity : TableStorageEntity
    {
        public const int ShardCount = 16;

        public string ActorId { get; set; }
        public string State { get; set; }
        public string DueUtc { get; set; }

        public static string CreatePartitionKey(AnonymousVisitorState state, int shard)
        {
            if (shard < 0 || shard >= ShardCount) throw new ArgumentOutOfRangeException(nameof(shard));
            return $"STATE|{state.ToString().ToLowerInvariant()}|{shard:x1}";
        }

        public static string CreatePartitionKey(AnonymousVisitorState state, string actorId)
        {
            if (String.IsNullOrEmpty(actorId)) throw new ArgumentNullException(nameof(actorId));
            using (var sha256 = SHA256.Create()) return CreatePartitionKey(state, sha256.ComputeHash(Encoding.UTF8.GetBytes(actorId))[0] % ShardCount);
        }

        public static string CreateRowKey(DateTime dueUtc, string actorId)
        {
            if (String.IsNullOrEmpty(actorId)) throw new ArgumentNullException(nameof(actorId));
            return $"{dueUtc.ToUniversalTime().Ticks:D19}|{actorId}";
        }
    }
}
