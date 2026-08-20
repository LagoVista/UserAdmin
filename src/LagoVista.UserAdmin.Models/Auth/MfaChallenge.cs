using System;

namespace LagoVista.UserAdmin.Models.Auth
{
    public class MfaChallenge
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string[] AvailableProviders { get; set; }
        public string CreatedUtc { get; set; }
        public string ExpiresUtc { get; set; }

        public bool IsExpired => !DateTime.TryParse(ExpiresUtc, out var expiresUtc) || expiresUtc.ToUniversalTime() <= DateTime.UtcNow;
    }
}
