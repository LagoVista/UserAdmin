using System;

namespace LagoVista.UserAdmin.Models.Users
{
    public class ContinuitySessionResponse
    {
        public string ActorId { get; set; }
        public string IdentityStage { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresUtc { get; set; }
        public string ContinuityToken { get; set; }
        public DateTime IdentityExpiresUtc { get; set; }
        public bool WasRestored { get; set; }

        public string ProvisionalEnvironmentId { get; set; }
        public string AppUserId { get; set; }
        public string OrganizationId { get; set; }
        public string SubscriptionId { get; set; }
        public string BootstrapContext { get; set; }
    }
}
