// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2ec908f9460f4419f0857d75c4713e636ead102fae837402f766bc7127427651
// IndexVersion: 2
// --- END CODE INDEX META ---
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
