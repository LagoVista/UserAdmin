using System;

namespace LagoVista.UserAdmin.Models.Security
{
    public class EmailVerificationCode
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string CodeHash { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public int AttemptCount { get; set; }
        public DateTime? ConsumedUtc { get; set; }
    }
}
