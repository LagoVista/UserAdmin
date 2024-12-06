﻿using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    public class SecureLink : TableStorageEntity
    {
        public string OrgId { get; set; } 
        public string OrgName { get; set; }
       
        public string CreatedByUserId { get; set; }
        public string CreatedByUser { get; set; }
        public string CreationDate { get; set; }

        public string ForUserId { get; set; }
        public string ForUserName { get; set; }
        public string DestinationLink { get; set; }
        public string Generator { get; set; }
        public string Expires { get; set; }
        public bool Expired { get; set; }

        public int AccessCount { get; set; }
        public string LastAccess { get; set; }
    }
}
