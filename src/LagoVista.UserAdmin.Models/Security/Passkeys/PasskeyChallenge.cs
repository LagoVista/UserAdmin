using LagoVista.Core;
using Newtonsoft.Json;
using System;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    public enum PasskeyChallengePurpose
    {
        Register,
        Authenticate,
    }

    public class PasskeyChallenge
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        public string UserId { get; set; }
        public string RpId { get; set; }
        public string Origin { get; set; }

        // Relative URL starting with /auth. Used for InvokeResult.SuccessRedirect after completion.
        public string PasskeyUrl { get; set; }

        public PasskeyChallengePurpose Purpose { get; set; }

        // base64url
        public string Challenge { get; set; }

        public string CreatedUtc { get; set; }
        public string ExpiresUtc { get; set; }

        [JsonIgnore]
        public bool IsExpired
        {
            get
            {
                if (String.IsNullOrEmpty(ExpiresUtc)) return true;
                if (!DateTime.TryParse(ExpiresUtc, out var dt)) return true;
                return DateTime.UtcNow > dt.ToUniversalTime();
            }
        }
    }
}
