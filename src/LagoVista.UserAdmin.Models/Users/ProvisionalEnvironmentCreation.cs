using System;

namespace LagoVista.UserAdmin.Models.Users
{
    public class CreateProvisionalEnvironmentRequest
    {
        public string CreationRequestId { get; set; }
        public string OriginActorId { get; set; }
        public string InstallationId { get; set; }
        public string BootstrapContext { get; set; }

        public bool TermsAndConditionsAccepted { get; set; }
        public string TermsAndConditionsVersion { get; set; }
        public string TermsAndConditionsAcceptedIPAddress { get; set; }

        public string ConversionJourneyId { get; set; }
        public string AcquisitionSourceKey { get; set; }
        public string CampaignKey { get; set; }
        public string EntryPointType { get; set; }
        public string EntryPointKey { get; set; }
        public string ExperimentKey { get; set; }
        public string ExperimentVariantKey { get; set; }
        public string AgentKey { get; set; }
        public string AgentVersion { get; set; }
        public string PromptVersion { get; set; }
    }

    public class CreateProvisionalEnvironmentResponse
    {
        public string ActorId { get; set; }
        public string ProvisionalEnvironmentId { get; set; }
        public string AppUserId { get; set; }
        public string OrganizationId { get; set; }
        public string SubscriptionId { get; set; }
        public string RecoveryToken { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public bool WasResumed { get; set; }
    }

    public class RestoreProvisionalEnvironmentRequest
    {
        public string RecoveryToken { get; set; }
        public string InstallationId { get; set; }
    }

    public class RestoreProvisionalEnvironmentResponse
    {
        public string ActorId { get; set; }
        public string ProvisionalEnvironmentId { get; set; }
        public string AppUserId { get; set; }
        public string OrganizationId { get; set; }
        public string SubscriptionId { get; set; }
        public string RecoveryToken { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public string BootstrapContext { get; set; }
    }
}
