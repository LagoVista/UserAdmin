using Azure.Storage.Blobs;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Exceptions;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Repos.Commo;
using LagoVista.UserAdmin.Models.Commo;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Commo
{
    public class SentEmailRepo : TableStorageBase<SentEmailDTO>, ISentEmailRepo
    {
        private readonly IAdminLogger _adminLogger;
        private readonly IUserAdminSettings _userAdminSettings;

        public SentEmailRepo(IUserAdminSettings settings, IAdminLogger logger) :
        base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {
            _adminLogger = logger ?? throw new ArgumentNullException(nameof(logger));
            _userAdminSettings = settings ?? throw new ArgumentNullException(nameof(settings));    
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

        public async Task<ListResponse<SentEmail>> GetSentEmailForMailerAsync(string orgId, string mailerId, ListRequest listRequest)
        {
            var records = await this.GetPagedResultsAsync(orgId,  listRequest, FilterOptions.Create(nameof(SentEmailDTO.MailerId), FilterOptions.Operators.Equals, mailerId));
            return ListResponse<SentEmail>.Create(records.Model.Select(rec => rec.ToSentEmail()), listRequest);
        }

        public async Task<ListResponse<SentEmail>> GetIndividualSentEmailForOrgAsync(string orgId, ListRequest listRequest)
        { 
            var records = await this.GetPagedResultsAsync(orgId, listRequest, FilterOptions.Create(nameof(SentEmailDTO.IndividualMessage), FilterOptions.Operators.Equals, true));
            return ListResponse<SentEmail>.Create(records.Model.Select(rec => rec.ToSentEmail()), listRequest);
        }


        public Task UpdateSentEmailAsync(SentEmail sentEmail)
        {
            return UpdateAsync(SentEmailDTO.FromSentEmail(sentEmail));
        }

        public async Task<SentEmail> GetSentEmailAsync(string orgId, string internalMessageId)
        {
            var sentEmail = await this.FindAsync(orgId, FilterOptions.Create(nameof(SentEmailDTO.InternalMessageId), FilterOptions.Operators.Equals, internalMessageId));
            if(sentEmail == null)
            {
                throw new RecordNotFoundException(nameof(SentEmail), $"OrgId={orgId}, internalMessgaeId={internalMessageId}");
            }

            return sentEmail.ToSentEmail();
        }

        private string GetContainerName(SentEmail sentEmail)
        {
            var dateStamp = sentEmail.SentDate.ToDateTime();
            var containerName =  $"emailarchive{sentEmail.OrgNameSpace}{dateStamp.Year:0000}{dateStamp.Month:00}".ToLower();
            return containerName;
        }

        private async Task<BlobContainerClient> GetBlobContainerClient(string containerName)
        {
            var baseuri = $"https://{_userAdminSettings.UserTableStorage.AccountId}.blob.core.windows.net";
            var uri = new Uri(baseuri);

            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={_userAdminSettings.UserTableStorage.AccountId};AccountKey={_userAdminSettings.UserTableStorage.AccessKey}";
            var blobClient = new BlobServiceClient(connectionString);
            try
            {
                var blobContainerClient = blobClient.GetBlobContainerClient(containerName);
                return blobContainerClient;
            }
            catch (Exception)
            {
                var container = await blobClient.CreateBlobContainerAsync(containerName);

                return container.Value;
            }
        }

        private string GetBlobName(SentEmail sentEmail)
        {
            return $"{sentEmail.InternalMessageId}.eml";
        }


        public async Task AddEmailBodyAsync(SentEmail email, string body)
        {
            var retryCount = 0;
            Exception ex = null;
            while (retryCount++ < 5)
            {
                try
                {
                    var containerName = GetContainerName(email);
                    var containerClient = await GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync();
                    var blobName = GetBlobName(email);
                    var buffer = System.Text.ASCIIEncoding.ASCII.GetBytes(body);
                    var blobClient = containerClient.GetBlobClient(blobName);
                    await blobClient.UploadAsync(new BinaryData(buffer));
                    return;
                }
                catch (Exception exc)
                {
                    ex = exc;
                    _adminLogger.AddException("AttachmentRepo_AddAttachmentAsync", ex);
                    Console.WriteLine("Exception deserializeing: " + ex.Message);
                    await Task.Delay(retryCount * 250);
                    if (retryCount == 4)
                        throw;
                }
            }
        }


        public async Task<string> GetEmailBodyAsync(SentEmail email)
        {
            var retryCount = 0;
            Exception ex = null;
            while (retryCount++ < 5)
            {
                try
                {
                    var containerName = GetContainerName(email);
                    var containerClient = await GetBlobContainerClient(containerName);
                    await containerClient.CreateIfNotExistsAsync();
                    var blobName = GetBlobName(email);
                    var blobClient = containerClient.GetBlobClient(blobName);
                    var content = await blobClient.DownloadContentAsync();
                    var text = System.Text.ASCIIEncoding.ASCII.GetString(content.Value.Content.ToArray());
                    return text;                    
                }
                catch (Exception exc)
                {
                    ex = exc;
                    _adminLogger.AddException("AttachmentRepo_GetAttachmentAsync", ex);
                    Console.WriteLine("Exception deserializeing: " + ex.Message);
                    await Task.Delay(retryCount * 250);
                    if (retryCount == 4)
                        throw;
                }
            }

            /* nop */
            return null;
        }
    } 

    public class SentEmailDTO : TableStorageEntity
    {
        public string Org { get; set; }
        public string InternalMessageId { get; set; }
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

        public string PersonaId { get; set; }
        public string Persona { get; set; }

        public string Subject { get; set; }

        public bool Processed { get; set; }
        public int Opens { get; set; }
        public int Clicks { get; set; }
        public bool Bounced { get; set; }
        public bool Delivered { get; set; }
        public bool Opened { get; set; }
        public bool Clicked { get; set; }
        public bool Undeliverable { get; set; }
        public string TemplateId { get; set; }
        public string Template { get; set; }
        public string MailerId { get; set; }
        public string Mailer { get; set; }

        public bool IndividualMessage { get; set; }

        public string PrimaryExternalMessageId { get; set; }

        public static SentEmailDTO FromSentEmail(SentEmail sentEmail)
        {
            return new SentEmailDTO()
            {
                Org = sentEmail.Org.Text,
                RowKey = sentEmail.ExternalMessageId,
                PrimaryExternalMessageId = sentEmail.PrimaryExternalMessageId,
                InternalMessageId = sentEmail.InternalMessageId,
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
                StatusDate = sentEmail.StatusDate,
                SenderEmail = sentEmail.SenderEmail,
                ReplyToEmail = sentEmail.ReplyToEmail,
                Processed = sentEmail.Processed,
                Opens = sentEmail.Opens,
                Clicks = sentEmail.Clicks,
                Bounced = sentEmail.Bounced,
                Opened = sentEmail.Opened,
                Delivered = sentEmail.Delivered,
                Undeliverable = sentEmail.Undeliverable,
                TemplateId = sentEmail.Template?.Id,
                Template = sentEmail.Template?.Text,
                MailerId = sentEmail.Mailer?.Id,
                Mailer = sentEmail.Mailer?.Text,
                Persona = sentEmail.Persona?.Text,
                Subject = sentEmail.Subject,
                PersonaId = sentEmail.Persona?.Id,
                IndividualMessage = sentEmail.IndividualMessage,
                
            };
        }

        public SentEmail ToSentEmail()
        {
            return new SentEmail()
            {
                ExternalMessageId = RowKey,
                InternalMessageId = InternalMessageId,
                PrimaryExternalMessageId = PrimaryExternalMessageId,
                Org = EntityHeader.Create(PartitionKey, Org),
                SentByUser = EntityHeader.Create(SentByUserId, SentByUser),
                AppUser = String.IsNullOrEmpty(AppUserId) ? null :  EntityHeader.Create(AppUserId, AppUser),
                Company = String.IsNullOrEmpty(CompanyId) ? null : EntityHeader.Create(CompanyId, Company),
                Contact = String.IsNullOrEmpty(ContactId) ? null : EntityHeader.Create(ContactId, Contact),
                Industry = String.IsNullOrEmpty(IndustryId) ? null : EntityHeader.Create(IndustryId, Industry),
                IndustryNiche = String.IsNullOrEmpty(IndustryNicheId) ? null : EntityHeader.Create(IndustryNicheId, IndustryNiche),
                Campaign = String.IsNullOrEmpty(CampaignId) ? null : EntityHeader.Create(CampaignId, Campaign),
                Promotion = String.IsNullOrEmpty(PromotionId) ? null : EntityHeader.Create(PromotionId, Promotion),
                Template = String.IsNullOrEmpty(TemplateId) ? null : EntityHeader.Create(TemplateId, Template),
                Persona = String.IsNullOrEmpty(PersonaId) ? null : EntityHeader.Create(PersonaId, Persona),
                Mailer = String.IsNullOrEmpty(MailerId) ? null : EntityHeader.Create(MailerId, Mailer),
                SentDate = SentDate,
                StatusDate = StatusDate,
                Email = Email,
                Status = Status,
                SenderEmail = SenderEmail,
                ReplyToEmail = ReplyToEmail,
                Bounced = Bounced,
                Delivered = Delivered,
                Subject = Subject,
                IndividualMessage = IndividualMessage,
                Undeliverable = Undeliverable,
                Clicks = Clicks,
                Opens = Opens,
                Opened = Opened,
                Clicked = Clicked,
                Processed = Processed,
            };
        }
    };
}
