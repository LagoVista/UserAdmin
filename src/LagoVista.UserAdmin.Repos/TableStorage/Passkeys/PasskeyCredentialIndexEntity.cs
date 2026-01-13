using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Repos.TableStorage.Passkeys
{
    public class PasskeyCredentialIndexEntity : TableStorageEntity
    {
        public string UserId { get; set; }
        public string RpId { get; set; }

        // base64url
        public string CredentialId { get; set; }

        public string CreatedUtc { get; set; }

        public static string CreatePartitionKey(string rpId, string credentialId)
        {
            if (String.IsNullOrEmpty(rpId)) throw new ArgumentNullException(nameof(rpId));
            if (String.IsNullOrEmpty(credentialId)) throw new ArgumentNullException(nameof(credentialId));
            var prefix = credentialId.Length >= 2 ? credentialId.Substring(0, 2) : credentialId;
            return $"RP|{rpId}|CRED|{prefix}";
        }

        public static string CreateRowKey(string credentialId)
        {
            if (String.IsNullOrEmpty(credentialId)) throw new ArgumentNullException(nameof(credentialId));
            return credentialId;
        }
    }
}
