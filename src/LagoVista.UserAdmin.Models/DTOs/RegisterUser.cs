// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: eadc48d25d414caea133196f9e73fec918b657151839460cf2fa447e5d30d445
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace LagoVista.UserAdmin.Models.DTOs
{
    public class RegisterUser : IValidateable
    {

        [JsonProperty("loginType")]
        public LoginTypes LoginType { get; set; } 

        [JsonProperty("appId")]
        public String AppId { get; set; }

        [JsonProperty("orgId")]
        public string OrgId { get; set; }

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
        [JsonProperty("inviteId")]
        public string InviteId { get; set; }

        public EntityHeader EndUserAppOrg { get; set; }
        public EntityHeader Customer { get; set; }
        public EntityHeader CustomerContact { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }
        [JsonProperty("customerCity")]
        public string CustomerCity { get; set; }
        [JsonProperty("customerState")]
        public string CustomerState { get; set; }


        [CustomValidator]
        public void Validate(Core.Validation.ValidationResult result, Actions action)
        {
            if (!String.IsNullOrEmpty(Email))
            {
                var email = new EmailAddressAttribute();
                if (!email.IsValid(Email)) result.AddUserError($"Invalid Email Address {Email}.");
            }
            switch(LoginType)
            {
                case LoginTypes.AppEndUser:
                    if (String.IsNullOrEmpty(OrgId) && EntityHeader.IsNullOrEmpty(EndUserAppOrg)) result.AddUserError("When LoginType is AppEnduser, either OrgId or EndUserAppOg is required.");
                    if(String.IsNullOrEmpty(CustomerName)) result.AddUserError("When LoginType is AppEnduser, CustomerName is required.");
                    if (String.IsNullOrEmpty(CustomerCity)) result.AddUserError("When LoginType is AppEnduser, CustomerCity is required.");
                    if (String.IsNullOrEmpty(CustomerState)) result.AddUserError("When LoginType is AppEnduser, CustomerState is required.");
                    break;
            }
        }
    }
}
