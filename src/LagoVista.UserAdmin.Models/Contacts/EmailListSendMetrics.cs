using System;

namespace LagoVista.UserAdmin.Models.Contacts
{
    public class EmailListSendMetrics
    {
        public DateTime TimeStampUtc { get; set; }
        public long? Requests { get; set; }
        public long? Delivered { get; set; }
        public long? Clicks { get; set; }
        public long? Opens { get; set; }
        public long? Bounces { get; set; }
        public long? Unsubscribes { get; set; }
    }
}
