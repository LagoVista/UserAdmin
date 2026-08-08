using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentStateIndexEntity : TableStorageEntity
    {
        public string EnvironmentId { get; set; }
        public string State { get; set; }
        public string ExpiresUtc { get; set; }

        public static string CreatePartitionKey(ProvisionalEnvironmentState state, int shard)
        {
            if (shard < 0 || shard >= ShardCount) throw new ArgumentOutOfRangeException(nameof(shard));
            return $"STATE|{state.ToString().ToLowerInvariant()}|{shard:x1}";
        }

        public static string CreatePartitionKey(ProvisionalEnvironmentState state, string environmentId)
        {
            return CreatePartitionKey(state, GetShard(environmentId));
        }

        public static string CreateRowKey(DateTime expiresUtc, string environmentId)
        {
            if (String.IsNullOrEmpty(environmentId)) throw new ArgumentNullException(nameof(environmentId));
            return $"{expiresUtc.ToUniversalTime().Ticks:D19}|{environmentId}";
        }

        public const int ShardCount = 16;

        private static int GetShard(string environmentId)
        {
            if (String.IsNullOrEmpty(environmentId)) throw new ArgumentNullException(nameof(environmentId));

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(environmentId));
                return hash[0] % ShardCount;
            }
        }
    }
}
