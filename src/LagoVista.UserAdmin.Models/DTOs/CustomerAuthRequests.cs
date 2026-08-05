using Newtonsoft.Json;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class CustomerLoginRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("endUserAppOrgId")]
        public string EndUserAppOrgId { get; set; }

        [JsonProperty("rememberMe")]
        public bool RememberMe { get; set; }

        [JsonProperty("lockoutOnFailure")]
        public bool LockoutOnFailure { get; set; }
    }

    public class CustomerForgotPasswordRequest
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("endUserAppOrgId")]
        public string EndUserAppOrgId { get; set; }
    }
}
