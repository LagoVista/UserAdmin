﻿using System;
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
