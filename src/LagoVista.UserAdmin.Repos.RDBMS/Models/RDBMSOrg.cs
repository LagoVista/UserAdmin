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
