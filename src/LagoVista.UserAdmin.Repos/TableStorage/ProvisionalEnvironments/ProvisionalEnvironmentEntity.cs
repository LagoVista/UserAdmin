using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Repos.TableStorage.ProvisionalEnvironments
{
    internal class ProvisionalEnvironmentEntity : TableStorageEntity
    {
        public string Id { get; set; }
        public string State { get; set; }
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
        public string TermsAndConditionsAcceptedUtc { get; set; }

        public string CreatedUtc { get; set; }
        public string ActivatedUtc { get; set; }
        public string LastActivityUtc { get; set; }
        public string ExpiresUtc { get; set; }
        public string ClaimedUtc { get; set; }
        public string ExpiredUtc { get; set; }
        public string PurgeAfterUtc { get; set; }
        public string StateChangedUtc { get; set; }

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

        public static string CreatePartitionKey(string id)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));

            var prefixLength = Math.Min(2, id.Length);
            return $"ENV|{id.Substring(0, prefixLength).ToLowerInvariant()}";
        }

        public static string CreateRowKey(string id)
        {
            if (String.IsNullOrEmpty(id)) throw new ArgumentNullException(nameof(id));
            return $"ENV|{id}";
        }
    }
}
