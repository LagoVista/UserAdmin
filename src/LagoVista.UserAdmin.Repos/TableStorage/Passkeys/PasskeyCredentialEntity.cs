using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using Newtonsoft.Json;
using System;

namespace LagoVista.UserAdmin.Repos.TableStorage.Passkeys
{
    public class PasskeyCredentialEntity : TableStorageEntity
    {
        public string UserId { get; set; }
        public string RpId { get; set; }

        // base64url
        public string CredentialId { get; set; }

        // base64url (COSE key bytes)
        public string PublicKey { get; set; }

        public uint SignCount { get; set; }

        public string Name { get; set; }

        public string CreatedUtc { get; set; }
        public string LastUsedUtc { get; set; }

        public static string CreatePartitionKey(string userId, string rpId)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));
            if (String.IsNullOrEmpty(rpId)) throw new ArgumentNullException(nameof(rpId));
            return $"USER|{userId}|RP|{rpId}";
        }

        public static string CreateRowKey(string credentialId)
        {
            if (String.IsNullOrEmpty(credentialId)) throw new ArgumentNullException(nameof(credentialId));
            return $"CRED|{credentialId}";
        }
    }
}
