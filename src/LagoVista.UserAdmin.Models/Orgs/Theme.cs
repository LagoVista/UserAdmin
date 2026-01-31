// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 99d825921844f42cdbd322a85f67d7bb437c70e21a31a1a860552c1653eb6304
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.BasicTheme_Name,
        UserAdminResources.Names.BasicTheme_Help,
        UserAdminResources.Names.BasicTheme_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class BasicTheme
    {
        public string PrimryBGColor { get; set; }
        public string PrimaryTextColor { get; set; }
        public string AccentColor { get; set; }
    }
}
