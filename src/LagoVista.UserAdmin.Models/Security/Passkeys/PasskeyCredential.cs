using LagoVista.Core;
using LagoVista.Core.Models;
using Newtonsoft.Json;
using System;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    public class PasskeyCredential
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

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

        [JsonIgnore]
        public EntityHeader User { get; set; }
    }
}
