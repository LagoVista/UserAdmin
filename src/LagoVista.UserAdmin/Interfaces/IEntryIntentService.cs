using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces
{
    public static class EntryIntentConstants
    {
        public const string CookieName = "entry_intent_cid";
        public const string CacheKeyPrefix = "entry-intent:"; // final key: entry-intent:{cid}
        public static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);
    }

    public class EntryIntentRecord
    {
        public string Path { get; set; }
        public string TargetOrgId { get; set; } // optional
        public DateTime CreatedUtc { get; set; }
        public string Source { get; set; } // optional
    }

    public interface IEntryIntentService
    {
        Task StashAsync(string path, string targetOrgId = null, string source = null);
        Task<EntryIntentRecord> ConsumeAsync(); // returns null if none/missing/invalid
    }
}
