// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: bd2aff450fabc66d09d7a2b560e79b0fd0215cfa65bba1767892ee69498ffea7
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Commo;
using System;

namespace LagoVista.UserAdmin.Repos.Repos.Commo
{
    public class SentEmailDTO : TableStorageEntity
    {
        public string Org { get; set; }
        public string ExternalMessageId { get; set; }
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
                RowKey = DateTime.UtcNow.ToInverseTicksRowKey(),
                ExternalMessageId = sentEmail.ExternalMessageId,
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
                Id = RowKey,
                ExternalMessageId = ExternalMessageId ?? RowKey,
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
