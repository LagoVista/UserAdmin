using LagoVista.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class InviteUser
    {
        [JsonProperty("inviteFromuserId")]
        public String InviteFromUserId { get; set; }

        [JsonProperty("inviteToOrgId")]
        public String InviteToOrgId { get; set; }

        [JsonProperty("email")]
        public String Email { get; set; }

        [JsonProperty("name")]
        public String Name { get; set; }

        [JsonProperty("message")]
        public String Message { get; set; }

        [JsonProperty("endUserOrgApp")]
        public EntityHeader EndUserAppOrg { get; set; }
        
        [JsonProperty("customer")]
        public EntityHeader Customer { get; set; }

        [JsonProperty("customerContact")]
        public EntityHeader CustomerContact { get; set; }
    }
}
