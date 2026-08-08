using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Users
{
    public class ProvisionalEnvironmentBillingEventArchive
    {
        public string Id { get; set; }
        public string SubscriptionId { get; set; }
        public string ProductId { get; set; }
        public string ModelUsageRateId { get; set; }
        public DateTime StartTimestamp { get; set; }
        public string StartedByAppUserId { get; set; }
        public DateTime? EndTimestamp { get; set; }
        public string EndedByAppUserId { get; set; }
        public string BillingDate { get; set; }
        public DateTime? RolloverAt { get; set; }
        public string IdempotencyKey { get; set; }
        public int BillingTimeZoneId { get; set; }
        public string Status { get; set; }
        public decimal? HoursBilled { get; set; }
        public long? Tokens { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? ActualCost { get; set; }
        public int UnitTypeId { get; set; }
        public decimal? DiscountPercent { get; set; }
        public decimal? Extended { get; set; }
        public string VendorUsageKey { get; set; }
        public decimal? Quantity { get; set; }
        public string ResourceId { get; set; }
        public string ResourceName { get; set; }
        public string Notes { get; set; }
        public string RollupType { get; set; }
    }

    public class ProvisionalEnvironmentArchiveManifest
    {
        public int SchemaVersion { get; set; } = 1;
        public string ProvisionalEnvironmentId { get; set; }
        public string AppUserId { get; set; }
        public string OrganizationId { get; set; }
        public string SubscriptionId { get; set; }
        public DateTime EstablishedUtc { get; set; }
        public DateTime LastActivityUtc { get; set; }
        public DateTime? ExpiredUtc { get; set; }
        public DateTime ArchivedUtc { get; set; }
        public string ArchiveReason { get; set; }
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
        public int BillingEventCount { get; set; }
        public decimal TotalActualCost { get; set; }
        public decimal TotalExtended { get; set; }
        public long TotalTokens { get; set; }
        public decimal TotalQuantity { get; set; }
        public DateTime? EarliestBillingEventUtc { get; set; }
        public DateTime? LatestBillingEventUtc { get; set; }
        public string BillingEventsFileName { get; set; } = "billing-events.csv";
        public string BillingEventsSha256 { get; set; }
    }

    public class ProvisionalEnvironmentArchiveWriteRequest
    {
        public ProvisionalEnvironmentArchiveManifest Manifest { get; set; }
        public IReadOnlyCollection<ProvisionalEnvironmentBillingEventArchive> BillingEvents { get; set; }
    }

    public class ProvisionalEnvironmentArchiveWriteResult
    {
        public string ArchivePath { get; set; }
        public string ManifestBlobName { get; set; }
        public string BillingEventsBlobName { get; set; }
        public string BillingEventsSha256 { get; set; }
        public int BillingEventCount { get; set; }
        public DateTime ArchivedUtc { get; set; }
        public bool AlreadyExisted { get; set; }
    }
}
