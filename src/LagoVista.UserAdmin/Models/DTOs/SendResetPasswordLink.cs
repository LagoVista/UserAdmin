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
