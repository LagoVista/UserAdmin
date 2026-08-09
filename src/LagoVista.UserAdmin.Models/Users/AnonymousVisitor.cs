using LagoVista.Core;
using System;

namespace LagoVista.UserAdmin.Models.Users
{
    public enum AnonymousVisitorState
    {
        Active,
        Promoted,
        Expired
    }

    public class AnonymousVisitor
    {
        public const int MaximumBootstrapContextLength = 4096;

        public string ActorId { get; set; } = Guid.NewGuid().ToId();
        public AnonymousVisitorState State { get; set; } = AnonymousVisitorState.Active;

        public string ContinuityTokenHash { get; set; }
        public string InstallationIdHash { get; set; }
        public string BootstrapContext { get; set; }

        public DateTime CreatedUtc { get; set; }
        public DateTime LastActivityUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public DateTime StateChangedUtc { get; set; }

        public string ProvisionalEnvironmentId { get; set; }
        public DateTime? PromotedUtc { get; set; }
        public DateTime? ExpiredUtc { get; set; }

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
