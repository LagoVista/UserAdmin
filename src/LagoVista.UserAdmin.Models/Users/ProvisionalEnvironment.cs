using LagoVista.Core;
using System;

namespace LagoVista.UserAdmin.Models.Users
{
    public enum ProvisionalEnvironmentState
    {
        Provisioning,
        Active,
        Claimed,
        Expired,
        PurgePending
    }

    public class ProvisionalEnvironment
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();
        public ProvisionalEnvironmentState State { get; set; } = ProvisionalEnvironmentState.Provisioning;
        public string CreationRequestId { get; set; }

        public string AppUserId { get; set; }
        public string OrganizationId { get; set; }
        public string SubscriptionId { get; set; }

        public string RecoveryTokenHash { get; set; }
        public string InstallationIdHash { get; set; }
        public string BootstrapContext { get; set; }

        public bool TermsAndConditionsAccepted { get; set; }
        public string TermsAndConditionsVersion { get; set; }
        public string TermsAndConditionsAcceptedIPAddress { get; set; }
        public DateTime? TermsAndConditionsAcceptedUtc { get; set; }

        public DateTime CreatedUtc { get; set; }
        public DateTime? ActivatedUtc { get; set; }
        public DateTime LastActivityUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public DateTime? ClaimedUtc { get; set; }
        public DateTime? ExpiredUtc { get; set; }
        public DateTime? PurgeAfterUtc { get; set; }
        public DateTime StateChangedUtc { get; set; }

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

        public string ETag { get; set; }
    }
}
