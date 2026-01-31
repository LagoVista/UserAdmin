// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ca43fb2dc17fbd78c25905bab6a8c7565ab38bc60b1cc20ef773371c765601e9
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
        UserAdminResources.Names.ChangePassword_Name,
        UserAdminResources.Names.ChangePassword_Help,
        UserAdminResources.Names.ChangePassword_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class ChangePassword
    {
        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("oldPassword")]
        public string OldPassword { get; set; }

        [JsonProperty("newPassword")]
        public string NewPassword { get; set; }
    }
}
