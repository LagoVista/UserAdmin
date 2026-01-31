// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ea345ec363b35724235dcb3600cead3090952c32fc4a5fa04b9e2faede87ecfa
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.OrganizationDetailVM_Title, UserAdminResources.Names.OrganizationDetailVM_Help, UserAdminResources.Names.OrganizationDetailsVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class OrganizationDetailsViewModel
    {
        public String OrganizationName { get; set; }
        public Models.Orgs.Organization Organization { get; set; }
        public IEnumerable<UserInfoSummary> People { get; set; }
        public IEnumerable<Models.Orgs.OrgLocation> Locations { get; set; }
    }
}
