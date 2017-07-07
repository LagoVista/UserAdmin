using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class VerfiyPhoneNumberDTO
    {
        [JsonProperty("smsCode")]
        public String SMSCode { get; set; }

        [JsonProperty("phoneNumber")]
        public String PhoneNumber { get; set; }
    }
}
