using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
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


        public String UserId { get; set; }

        public String UsersName { get; set; }

        public String TeamId { get; set; }

        public String TeamName { get; set; }

        public string ProfileImageUrl { get; set; }

        public String CreatedBy { get; set; }
        public String CreatedById { get; set; }
        public String CreationDate { get; set; }
        public String LastUpdatedBy { get; set; }
        public String LastUpdatedById { get; set; }
        public String LastUpdatedDate { get; set; }

        public static String CreateRowKey(EntityHeader team, EntityHeader user)
        {
            return $"{team.Id}.{user.Id}";
        }

        public static String CreateRowKey(string teamId, string userId)
        {
            return $"{teamId}.{userId}";
        }

        public TeamUserSummary CreateSummary()
        {
            return new TeamUserSummary()
            {
                Id = UserId,
                Name = UsersName,
                ProfileImageUrl = ProfileImageUrl,
            };
        }
    }

    public class TeamUserSummary : SummaryData
    {
        public string ProfileImageUrl { get; set; }
    }
}
