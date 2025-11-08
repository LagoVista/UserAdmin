// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 45ef345c95dda7073fcc1744115db447d90dbcbf0f41fa2a24d609223a4ca66e
// IndexVersion: 2
// --- END CODE INDEX META ---
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class ResetPassword
    {
        [JsonProperty("email")]
        public String Email { get; set; }
        [JsonProperty("token")]
        public String Token { get; set; }
        [JsonProperty("newPassword")]
        public String NewPassword { get; set; }
    }
}
