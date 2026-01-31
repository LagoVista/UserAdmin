// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d6b633377be5a0d3b138d9e182790115ca39c9e69f3b141ecb686ace89f1968d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.TwitterErrorResponse_Name,
        UserAdminResources.Names.TwitterErrorResponse_Help,
        UserAdminResources.Names.TwitterErrorResponse_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public sealed class TwitterErrorResponse
    {
        public List<TwitterError> Errors { get; set; }
    }

    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.TwitterError_Name,
        UserAdminResources.Names.TwitterError_Help,
        UserAdminResources.Names.TwitterError_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public sealed class TwitterError
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
