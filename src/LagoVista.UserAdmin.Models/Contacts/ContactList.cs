using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Contacts
{
    public class ContactList
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public int Count { get; set; }
        public string LastUpdated { get; set; }
    }

    public class EmailListSend
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int Bounces { get; set; }
        public int Clicks { get; set; }
        public int Opens { get; set; }
        public int Unsubscribes { get; set; }
        public string CreateDate { get; set; }
        public string StatusDate { get; set; }
    }

    public class EmailDesign
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ThumbmailImage { get; set; }
    }
}
