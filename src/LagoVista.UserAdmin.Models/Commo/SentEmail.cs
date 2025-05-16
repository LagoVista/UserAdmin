using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LagoVista.UserAdmin.Models.Commo
{
    public class SentEmail
    {
        public string ExternalMessageId { get; set; }
        public string Email { get; set; }

        public EntityHeader Org { get; set; }
        public EntityHeader Contact { get; set; }
        public EntityHeader Company { get; set; }
        public EntityHeader AppUser { get; set; }
        public EntityHeader SentByUser { get; set; }

        public EntityHeader Industry { get; set; }
        public EntityHeader IndustryNiche { get; set; }

        public EntityHeader Campaign { get; set; }
        public EntityHeader Promotion { get; set; }

        public EntityHeader Template { get; set; }
        public EntityHeader Mailer { get; set; }

        public string SenderEmail { get; set; }
        public string ReplyToEmail { get; set; }

        public string SentDate { get; set; }
        public string Status { get; set; }
        public string StatusDate { get; set; }

        public bool Processed { get; set; }
        public bool Delivered { get; set; }
        public bool Undeliverable { get; set; }
    
        public bool Opened { get; set; }
        public bool Clicked { get; set; }
        
        public int Opens { get; set; }
        public int Clicks { get; set; }
        public bool Bounced { get; set; }
      
    }
}
