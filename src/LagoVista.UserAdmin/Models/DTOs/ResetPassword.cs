using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class ResetPassword
    {
        [JsonProperty("useId")]
        public String UserId { get; set; }
        [JsonProperty("newPassword")]
        public String NewPassword { get; set; }
    }
}
