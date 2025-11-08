// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ad3818948224cbd88be94e387c13f1a6bd176f0b4dbc8bd96721a388b13231f3
// IndexVersion: 2
// --- END CODE INDEX META ---
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class SendResetPasswordLink
    {
        [JsonProperty("email")]
        public String Email { get; set; }
    }
}
