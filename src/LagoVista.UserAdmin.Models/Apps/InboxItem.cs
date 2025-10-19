// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6b5a28206772e49935803b7cde6113eee9cc4e42fae405bb4d98128d7f37c5fc
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class InboxItem
    {
        public string RowKey { get; set; }
        public string PartitionKey { get; set; }
        public string Type { get; set; }
        public string Scope { get; set; }
        public int? Index { get; set; }
        public bool Viewed { get; set; }

        public string CreatedBy { get; set; }
        public string CreationDate { get; set; }

        public string Title { get; set; }
        public string Icon { get; set; }
        public string Summary { get; set; }

        public string Link { get; set; }
    }
}
