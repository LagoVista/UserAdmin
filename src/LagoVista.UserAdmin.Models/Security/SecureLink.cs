using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    public class SecureLink : TableStorageEntity
    {
        public string OrgId { get; set; } 
        public string OrgName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DestinationLink { get; set; }
        public string Generator { get; set; }
        public string Expires { get; set; }
    }
}
