using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class ConfirmEmail
    {
        [JsonProperty("receivedCode")]
        public String ReceivedCode { get; set; }
    }
}
