﻿using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Team_Title, UserAdminResources.Names.Team_Help, UserAdminResources.Names.Team_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Team : UserAdminModelBase, INamedEntity, IKeyedEntity, IValidateable, IOwnedEntity, IDescriptionEntity
    {

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }
        public TeamSummary CreateSummary()
        {
            return new TeamSummary()
            {
                Id = Id,
                Name = Name,
                Key = Key,
                IsPublic = false,
                Description = Description
            };
        }
    }

    public class TeamSummary : SummaryData
    {

    }
}
