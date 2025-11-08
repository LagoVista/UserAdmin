// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ca43fb2dc17fbd78c25905bab6a8c7565ab38bc60b1cc20ef773371c765601e9
// IndexVersion: 2
// --- END CODE INDEX META ---
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class ChangePassword
    {
        [JsonProperty("userId")]
        public String UserId { get; set; }
        [JsonProperty("oldPassword")]
        public String OldPassword { get; set; }
        [JsonProperty("newPassword")]
        public String NewPassword { get; set; }
    }
}
