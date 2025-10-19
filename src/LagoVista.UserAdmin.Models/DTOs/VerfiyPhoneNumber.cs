// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d1ce2cbef08d96a0e9c5ef917956126faf1935c115a1df5792ae1367f221541d
// IndexVersion: 0
// --- END CODE INDEX META ---
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class VerfiyPhoneNumber
    {
        [JsonProperty("smsCode")]
        public String SMSCode { get; set; }

        [JsonProperty("phoneNumber")]
        public String PhoneNumber { get; set; }

        [JsonProperty("skipStep")]
        public bool SkipStep { get; set; }
    }
}
