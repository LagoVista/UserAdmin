using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal class AnonymousVisitorLookupEntity : TableStorageEntity
    {
        public string ActorId { get; set; }
        public string LookupHash { get; set; }
        public string CreatedUtc { get; set; }

        public static string CreatePartitionKey(string lookupType, string lookupHash)
        {
            if (String.IsNullOrEmpty(lookupType)) throw new ArgumentNullException(nameof(lookupType));
            if (String.IsNullOrEmpty(lookupHash)) throw new ArgumentNullException(nameof(lookupHash));
            return $"{lookupType}|{lookupHash.Substring(0, Math.Min(2, lookupHash.Length)).ToLowerInvariant()}";
        }

        public static string CreateRowKey(string lookupType, string lookupHash)
        {
            if (String.IsNullOrEmpty(lookupType)) throw new ArgumentNullException(nameof(lookupType));
            if (String.IsNullOrEmpty(lookupHash)) throw new ArgumentNullException(nameof(lookupHash));
            return $"{lookupType}|{lookupHash}";
        }
    }
}
