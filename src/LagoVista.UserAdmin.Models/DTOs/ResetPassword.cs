// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 45ef345c95dda7073fcc1744115db447d90dbcbf0f41fa2a24d609223a4ca66e
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
        Domains.SecurityDomain,
        UserAdminResources.Names.ResetPassword_Name,
        UserAdminResources.Names.ResetPassword_Help,
        UserAdminResources.Names.ResetPassword_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class ResetPassword
    {
        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
    }
}
