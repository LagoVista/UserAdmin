// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9e642a64fd7040be5101821560ecb212b004087ff031da8a14d1dab2984ca319
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(
        Domains.UserDomain, UserAdminResources.Names.TeamUser_Name, UserAdminResources.Names.TeamUser_Help, UserAdminResources.Names.TeamUser_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "access", ModelType: EntityDescriptionAttribute.ModelTypes.DomainEntity, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 10, IndexTagsCsv: "userdomain,access,domainentity,membership")]
    public class TeamUser : TableStorageEntity, IValidateable, ITableStorageAuditableEntity
    {
        public TeamUser(EntityHeader team, EntityHeader user)
        {
            RowKey = CreateRowKey(team, user);
            PartitionKey = $"{team.Id}";
            UserId = user.Id;
            UsersName = user.Text;
            TeamId = team.Id;
            TeamName = team.Text;
        }

        public TeamUser()
        {

        }

        public string UserId { get; set; }

        public string UsersName { get; set; }

        public string TeamId { get; set; }

        public string TeamName { get; set; }

        public string ProfileImageUrl { get; set; }

        public string CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public string CreationDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedById { get; set; }
        public string LastUpdatedDate { get; set; }

        public static string CreateRowKey(EntityHeader team, EntityHeader user)
        {
            return $"{team.Id}.{user.Id}";
        }

        public static string CreateRowKey(string teamId, string userId)
        {
            return $"{teamId}.{userId}";
        }

        public TeamUserSummary CreateSummary()
        {
            return new TeamUserSummary()
            {
                Id = UserId,
                Key = UserId,
                Name = UsersName,
                ProfileImageUrl = ProfileImageUrl,
            };
        }
    }

    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.TeamUserSummary_Name, UserAdminResources.Names.TeamUserSummary_Help,
        UserAdminResources.Names.TeamUserSummary_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        ClusterKey: "access", ModelType: EntityDescriptionAttribute.ModelTypes.DomainEntity, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Aux,
        IndexPriority: 35, IndexTagsCsv: "organizationdomain,access,summary,user")]
    public class TeamUserSummary : SummaryData
    {
        public string ProfileImageUrl { get; set; }
    }
}
