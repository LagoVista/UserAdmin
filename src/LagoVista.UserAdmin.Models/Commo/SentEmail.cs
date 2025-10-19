// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a06032810b1c4baa3a40c3c3b89fae2e487723b608da24e4739a9c678ba95a78
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace LagoVista.UserAdmin.Models.Commo
{
    [EntityDescription(Domains.EmailServicesDomain, UserAdminResources.Names.SentEmails_Title, UserAdminResources.Names.SentEmail_Description,
                    UserAdminResources.Names.SentEmail_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources), Icon: "icon-ae-email-3",
                    ListUIUrl: "/business/email")]
    public class SentEmail
    {
        public string Id { get; set; }
        public string ExternalMessageId { get; set; }
        public string InternalMessageId { get; set; }

        public string PrimaryExternalMessageId { get; set; }

        public string Subject { get; set; }

        public string Email { get; set; }

        public string OrgNameSpace { get; set; }

        public EntityHeader Org { get; set; }
        public EntityHeader Contact { get; set; }
        public EntityHeader Company { get; set; }
        public EntityHeader AppUser { get; set; }
        public EntityHeader SentByUser { get; set; }

        public EntityHeader Industry { get; set; }
        public EntityHeader IndustryNiche { get; set; }

        public EntityHeader Campaign { get; set; }
        public EntityHeader Promotion { get; set; }
        public EntityHeader Persona { get; set; }
        public EntityHeader Template { get; set; }
        public EntityHeader Mailer { get; set; }
        public bool IndividualMessage { get; set; }

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
