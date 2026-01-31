// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d1ce2cbef08d96a0e9c5ef917956126faf1935c115a1df5792ae1367f221541d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.DTOs
{
    [EntityDescription(
        Domains.UserDomain,
        UserAdminResources.Names.VerfiyPhoneNumber_Name,
        UserAdminResources.Names.VerfiyPhoneNumber_Help,
        UserAdminResources.Names.VerfiyPhoneNumber_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class VerfiyPhoneNumber
    {
        [JsonProperty("smsCode")]
        public string SMSCode { get; set; }

        [JsonProperty("phoneNumber")]
        public string PhoneNumber { get; set; }

        [JsonProperty("skipStep")]
        public bool SkipStep { get; set; }
    }
}
