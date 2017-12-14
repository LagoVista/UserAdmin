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
