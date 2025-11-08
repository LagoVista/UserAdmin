// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 51130dde19cba3c4494dda56ee6f3badaaa48a3177ca22eec73172bd2f02cbe5
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace LagoVista.UserAdmin.Repos.RDBMS.Models
{
    public class RDBMSOrg
    {
        [Key]
        public string OrgId { get; set; }
        public string OrgName { get; set; }
        public string OrgBillingContactId { get; set; }
        public string Status { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        
    }
}
