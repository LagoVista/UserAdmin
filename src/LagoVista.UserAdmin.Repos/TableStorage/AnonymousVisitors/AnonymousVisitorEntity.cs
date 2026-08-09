using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Repos.TableStorage.AnonymousVisitors
{
    internal class AnonymousVisitorEntity : TableStorageEntity
    {
        public string ActorId { get; set; }
        public string State { get; set; }
        public string ContinuityTokenHash { get; set; }
        public string InstallationIdHash { get; set; }
        public string BootstrapContext { get; set; }
        public string CreatedUtc { get; set; }
        public string LastActivityUtc { get; set; }
        public string ExpiresUtc { get; set; }
        public string StateChangedUtc { get; set; }
        public string ProvisionalEnvironmentId { get; set; }
        public string PromotedUtc { get; set; }
        public string ExpiredUtc { get; set; }
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

        public static string CreatePartitionKey(string actorId)
        {
            if (String.IsNullOrEmpty(actorId)) throw new ArgumentNullException(nameof(actorId));
            return $"VIS|{actorId.Substring(0, Math.Min(2, actorId.Length)).ToLowerInvariant()}";
        }

        public static string CreateRowKey(string actorId)
        {
            if (String.IsNullOrEmpty(actorId)) throw new ArgumentNullException(nameof(actorId));
            return $"VIS|{actorId}";
        }
    }
}
