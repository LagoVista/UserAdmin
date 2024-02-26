using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class InboxItem
    {
        public string RowKey { get; set; }
        public string ParitionKey { get; set; }
        public string Type { get; set; }
        public int? Index { get; set; }
        public bool Viewed { get; set; }

        public string Title { get; set; }
        public string Icon { get; set; }
        public string Summary { get; set; }

        public string Link { get; set; }
    }
}
