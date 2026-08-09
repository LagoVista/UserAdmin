using System;

namespace LagoVista.UserAdmin.Models.Users
{
    public class AnonymousVisitorPromotionRequest
    {
        public bool TermsAndConditionsAccepted { get; set; }
        public string TermsAndConditionsVersion { get; set; }
        public string InstallationId { get; set; }
    }

    public class AnonymousVisitorPromotionResponse
    {
        public string ActorId { get; set; }
        public string IdentityStage { get; set; }
        public string ProvisionalEnvironmentId { get; set; }
        public string AppUserId { get; set; }
        public string OrganizationId { get; set; }
        public string SubscriptionId { get; set; }
        public string RecoveryToken { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public string BootstrapContext { get; set; }
        public bool WasResumed { get; set; }
    }
}
