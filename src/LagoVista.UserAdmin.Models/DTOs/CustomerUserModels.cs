using LagoVista.Core.Models;
using Newtonsoft.Json;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class UpdateCustomerUserRequest
    {
        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("customerContact")]
        public EntityHeader CustomerContact { get; set; }
    }

    public class CustomerUserSummary
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public EntityHeader EndUserAppOrg { get; set; }
        public EntityHeader Customer { get; set; }
        public EntityHeader CustomerContact { get; set; }
        public bool IsCustomerAdmin { get; set; }
        public bool IsAccountDisabled { get; set; }
        public bool EmailConfirmed { get; set; }
    }
}
