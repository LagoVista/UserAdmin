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
