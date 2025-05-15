using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Commo;
using LagoVista.UserAdmin.Models.Commo;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Commo
{
    public class SentEmailRepo : TableStorageBase<SentEmailDTO>, ISentEmailRepo
    {
        private readonly IAdminLogger _adminLogger;

        public SentEmailRepo(IUserAdminSettings settings, IAdminLogger logger) :
        base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task AddSentEmailAsync(SentEmail sentEmail)
        {
            return InsertAsync(SentEmailDTO.FromSentEmail(sentEmail)); 
        }

        public async Task<SentEmail> GetSentEmailAsync(string externalMessageId)
        {
            var record = await GetAsync(externalMessageId, false);
            if (record == null)
                return null;

            return record.ToSentEmail();
        }

        public async Task<ListResponse<SentEmail>> GetSentEmailForOrgAsync(string orgId, ListRequest listRequest)
        {
            var records = await this.GetPagedResultsAsync(orgId, listRequest);
            return ListResponse<SentEmail>.Create(records.Model.Select(rec => rec.ToSentEmail()), listRequest);
        }

        public Task UpdateSentEmailAsync(SentEmail sentEmail)
        {
            return UpdateAsync(SentEmailDTO.FromSentEmail(sentEmail));
        }
    } 

    public class SentEmailDTO : TableStorageEntity
    {
        public string Org { get; set; }
        public string Email { get; set; }
        public string ContactId { get; set; }
        public string CompanyId { get; set; }
        public string Company { get; set; }
        public string Contact { get; set; }

        public string AppUserId { get; set; }
        public string AppUser { get; set; }
        public string SentByUserId { get; set; }
        public string SentByUser { get; set; }
        public string SenderEmail { get; set; }
        public string ReplyToEmail { get; set;}
        public string SentDate { get; set; }
        public string Status { get; set; }
        public string StatusDate { get; set; }

        public string IndustryId { get; set; }
        public string Industry { get; set; }

        public string IndustryNicheId { get; set; }
        public string IndustryNiche { get; set; }

        public string CampaignId { get; set; }
        public string Campaign { get; set; }

        public string PromotionId { get; set; }
        public string Promotion { get; set; }
        public bool Processed { get; set; }
        public int Opens { get; set; }
        public int Clicks { get; set; }
        public bool Bounced { get; set; }
        public bool Delivered { get; set; }
        public bool Undeliverable { get; set; }

        public static SentEmailDTO FromSentEmail(SentEmail sentEmail)
        {
            return new SentEmailDTO()
            {
                Org = sentEmail.Org.Text,
                RowKey = sentEmail.ExternalMessageId,
                Email = sentEmail.Email,
                AppUserId = sentEmail.AppUser?.Id,
                AppUser = sentEmail.AppUser?.Text,
                CompanyId = sentEmail.Company?.Id,
                Company = sentEmail.Company?.Text,
                Contact = sentEmail.Contact?.Text,
                ContactId = sentEmail.Contact?.Id,
                Industry = sentEmail.Industry?.Text,
                IndustryId = sentEmail.Industry?.Id,
                IndustryNiche = sentEmail.IndustryNiche?.Text,
                IndustryNicheId = sentEmail.IndustryNiche?.Id,
                Campaign = sentEmail.Campaign?.Text,
                CampaignId = sentEmail.Campaign?.Id,
                Promotion = sentEmail.Promotion?.Text,
                PromotionId = sentEmail.Promotion?.Id,
                PartitionKey = sentEmail.Org.Id,
                SentByUserId = sentEmail.SentByUser.Id,
                SentByUser = sentEmail.SentByUser.Text,
                SentDate = sentEmail.SentDate,
                Status = sentEmail.Status,  
                SenderEmail = sentEmail.SenderEmail,
                ReplyToEmail = sentEmail.ReplyToEmail,
                Processed = sentEmail.Processed,
                Opens = sentEmail.Opens,
                Clicks = sentEmail.Clicks,
                Bounced = sentEmail.Bounced,
                Delivered = sentEmail.Delivered,
                Undeliverable = sentEmail.Undeliverable,
            };
        }

        public SentEmail ToSentEmail()
        {
            return new SentEmail()
            {
                ExternalMessageId = RowKey,
                Org = EntityHeader.Create(PartitionKey, Org),
                SentByUser = EntityHeader.Create(SentByUserId, SentByUser),
                AppUser = String.IsNullOrEmpty(AppUserId) ? null :  EntityHeader.Create(AppUserId, AppUser),
                Company = String.IsNullOrEmpty(CompanyId) ? null : EntityHeader.Create(CompanyId, Company),
                Contact = String.IsNullOrEmpty(ContactId) ? null : EntityHeader.Create(ContactId, Contact),
                Industry = String.IsNullOrEmpty(IndustryId) ? null : EntityHeader.Create(IndustryId, Industry),
                IndustryNiche = String.IsNullOrEmpty(IndustryNicheId) ? null : EntityHeader.Create(IndustryNicheId, IndustryNiche),
                Campaign = String.IsNullOrEmpty(CampaignId) ? null : EntityHeader.Create(CampaignId, Campaign),
                Promotion = String.IsNullOrEmpty(PromotionId) ? null : EntityHeader.Create(PromotionId, Promotion),
                SentDate = SentDate,
                StatusDate = StatusDate,
                Email = Email,
                Status = Status,
                SenderEmail = SenderEmail,
                ReplyToEmail = ReplyToEmail,
                Bounced = Bounced,
                Delivered = Delivered,
                Undeliverable = Undeliverable,
                Clicks = Clicks,
                Opens = Opens,
                Processed = Processed,
            };
        }
    };
}
