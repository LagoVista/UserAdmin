﻿using Newtonsoft.Json;
using System;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class RegisterUserDTO
    {
        [JsonProperty("appId")]
        public String AppId { get; set; }
        [JsonProperty("appInstanceId")]
        public String AppInstanceId { get; set; }
        [JsonProperty("clientType")]
        public String ClientType { get; set; }
        [JsonProperty("deviceId")]
        public String DeviceId { get; set; }
        [JsonProperty("firstName")]
        public string FirstName { get; set; }
        [JsonProperty("lastName")]
        public string LastName { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
