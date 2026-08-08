using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Users
{
    public class ProvisionalEnvironmentLifecycleSummary
    {
        public string ProvisionalEnvironmentId { get; set; }
        public ProvisionalEnvironmentState State { get; set; }
        public string AppUserId { get; set; }
        public string OrganizationId { get; set; }
        public string SubscriptionId { get; set; }
        public DateTime CreatedUtc { get; set; }
        public DateTime LastActivityUtc { get; set; }
        public DateTime ExpiresUtc { get; set; }
        public DateTime? PurgeAfterUtc { get; set; }
    }

    public class ProvisionalEnvironmentLifecycleBatchResult
    {
        public int ExaminedCount { get; set; }
        public int UpdatedCount { get; set; }
        public List<string> ProvisionalEnvironmentIds { get; set; } = new List<string>();
    }
}
