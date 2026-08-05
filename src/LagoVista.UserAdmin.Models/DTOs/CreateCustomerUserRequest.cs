using LagoVista.Core.Models;
using Newtonsoft.Json;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class CreateCustomerUserRequest
    {
        [JsonProperty("appId")]
        public string AppId { get; set; }

        [JsonProperty("appInstanceId")]
        public string AppInstanceId { get; set; }

        [JsonProperty("clientType")]
        public string ClientType { get; set; }

        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("endUserAppOrg")]
        public EntityHeader EndUserAppOrg { get; set; }

        [JsonProperty("customer")]
        public EntityHeader Customer { get; set; }

        [JsonProperty("customerContact")]
        public EntityHeader CustomerContact { get; set; }

        [JsonProperty("isCustomerAdmin")]
        public bool IsCustomerAdmin { get; set; }

        [JsonProperty("autoLogin")]
        public bool AutoLogin { get; set; }
    }
}
