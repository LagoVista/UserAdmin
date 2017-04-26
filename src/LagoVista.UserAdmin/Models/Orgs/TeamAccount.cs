using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public class TeamAccount : TableStorageEntity, IValidateable, ITableStorageAuditableEntity
    {
        public TeamAccount(EntityHeader team, EntityHeader account)
        {
            RowKey = CreateRowKey(team, account);
            PartitionKey = $"{team.Id}";
            AccountId = account.Id;
            AccountName = account.Text;
            TeamId = team.Id;
            TeamName = team.Text;
        }

        public TeamAccount()
        {

        }


        public String AccountId { get; set; }

        public String AccountName { get; set; }

        public String TeamId { get; set; }

        public String TeamName { get; set; }

        public string ProfileImageUrl { get; set; }

        public String CreatedBy { get; set; }
        public String CreatedById { get; set; }
        public String CreationDate { get; set; }
        public String LastUpdatedBy { get; set; }
        public String LastUpdatedById { get; set; }
        public String LastUpdatedDate { get; set; }

        public static String CreateRowKey(EntityHeader team, EntityHeader account)
        {
            return $"{team.Id}.{account.Id}";
        }

        public static String CreateRowKey(string teamId, string accountId)
        {
            return $"{teamId}.{accountId}";
        }

        public TeamAccountSummary CreateSummary()
        {
            return new TeamAccountSummary()
            {
                Id = AccountId,
                Name = AccountName,
                ProfileImageUrl = ProfileImageUrl,
            };
        }
    }

    public class TeamAccountSummary : SummaryData
    {
        public string ProfileImageUrl { get; set; }
    }
}
