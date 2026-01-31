// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ad3818948224cbd88be94e387c13f1a6bd176f0b4dbc8bd96721a388b13231f3
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
        Domains.OrganizationDomain,
        UserAdminResources.Names.SendResetPasswordLink_Name,
        UserAdminResources.Names.SendResetPasswordLink_Help,
        UserAdminResources.Names.SendResetPasswordLink_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class SendResetPasswordLink
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
