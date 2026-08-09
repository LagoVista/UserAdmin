using System;

namespace LagoVista.UserAdmin.Models.Users
{
    public class AnonymousVisitorBootstrapRequest
    {
        public string InstallationId { get; set; }
        public string BootstrapContext { get; set; }

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

    public class AnonymousVisitorRestoreRequest
    {
        public string ContinuityToken { get; set; }
        public string InstallationId { get; set; }
    }

    public class AnonymousVisitorBootstrapResponse
    {
        public string ActorId { get; set; }
        public string IdentityStage { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiresUtc { get; set; }
        public string ContinuityToken { get; set; }
        public DateTime VisitorExpiresUtc { get; set; }
        public string BootstrapContext { get; set; }
        public bool WasRestored { get; set; }
    }
}
