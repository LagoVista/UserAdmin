// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 463930f8a4a40a639aeb73fd4a301f9589402c4efa49bf42fb235138761da7e6
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Contacts
{
    [EntityDescription(Domains.EmailServicesDomain, TitleResource: UserAdminResources.Names.ContactList_Title, UserHelpResource: UserAdminResources.Names.ContactList_Description, DescriptionResource: UserAdminResources.Names.ContactList_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources), Icon: "icon-pz-programming")]
    public class ContactList
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public int Count { get; set; }
        public string LastUpdated { get; set; }
    }

    public class EmailListSend
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int Bounces { get; set; }
        public int Clicks { get; set; }
        public int Opens { get; set; }
        public int Unsubscribes { get; set; }
        public string CreateDate { get; set; }
        public string StatusDate { get; set; }
    }

    public class EmailDesign
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ThumbmailImage { get; set; }
    }

    public class EmailImportStatusDetail
    {
        [JsonProperty("requested_count")]
        public int RequestedCount { get; set; }

        [JsonProperty("created_count")]
        public int CreatedCount { get; set; }

        [JsonProperty("updated_count")]
        public int UpdatedCount { get; set; }

        [JsonProperty("deleted_count")]
        public int DeletedCount { get; set; }

        [JsonProperty("errored_count")]
        public int ErrorCount { get; set; }

        [JsonProperty("errors_url")]
        public string ErrorUrl { get; set; }
    }

    public class EmailImportStatus
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("created")]
        public string Created { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("job_type")]
        public string JobType { get; set; }


        [JsonProperty("started_at")]
        public string StartedAt { get; set; }

        [JsonProperty("finished_at")]
        public string FinishedAt { get; set; }

        [JsonProperty("results")]
        public EmailImportStatusDetail Details { get; set; }
    }

    public class EmailSenderAddress : IValidateable, IFormDescriptor
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Email)
            };
        }
    }

    [EntityDescription(Domains.EmailServicesDomain, UserAdminResources.Names.EmailSender_Title, UserAdminResources.Names.EmailSender_Description, UserAdminResources.Names.EmailSender_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),
        Icon: "icon-pz-programmer", EditUIUrl: "/marketing/emailcampaigns/sender/{id|", CreateUIUrl: "/marketing/emailcampaigns/sender/add", ListUIUrl: "/marketing/emailcampaigns/senders",
        SaveUrl: "/api/email/sender", GetUrl: "/api/email/sender/{id}", GetListUrl: "/api/email/senders", DeleteUrl: "/api/email/sender/{id}", FactoryUrl: "/api/email/sender/factory")]
    public class EmailSender : IValidateable, IFormDescriptor
    {
        /*
{
"id": 1,
"nickname": "Example Orders",
"from": {
"email": "orders@example.com",
"name": "Example Orders"
},
"reply_to": {
"email": "support@example.com",
"name": "Example Support"
},
"address": "1234 Fake St.",
"address_2": "",
"city": "San Francisco",
"state": "CA",
"zip": "94105",
"country": "United States",
"verified": true,
"updated_at": 1449872165,
"created_at": 1449872165,
"locked": false
}             
         */

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("nickname")]
        public string NickName { get; set; }

        [FormField(UserAdminResources.Names.Common_Name, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }


        [JsonProperty("from")]
        public EmailSenderAddress From { get; set; }

        [JsonProperty("reply_to")]
        public EmailSenderAddress ReplyTo { get; set; }

        [FormField(UserAdminResources.Names.EmailSender_FromName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string FromName { get; set; }

        [FormField(UserAdminResources.Names.EmailSender_FromAddress, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string FromAddress { get; set; }



        [FormField(UserAdminResources.Names.EmailSender_ReplyToName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string ReplyToName { get; set; }

        [FormField(UserAdminResources.Names.EmailSender_ReplyToAddress, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string ReplyToAddress { get; set; }


        [JsonProperty("address")]
        [FormField(UserAdminResources.Names.Common_Address1, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Address { get; set; }

        [JsonProperty("address_2")]
        public string OrganizationId { get; set; }
        [JsonProperty("city")]
        [FormField(UserAdminResources.Names.Common_City, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string City { get; set; }
        [JsonProperty("state")]
        [FormField(UserAdminResources.Names.Common_State, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string State { get; set; }
        [JsonProperty("zip")]
        [FormField(UserAdminResources.Names.Common_PostalCode, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Zip { get; set; }
        [JsonProperty("country")]
        [FormField(UserAdminResources.Names.Common_Country, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Country { get; set; }
        [JsonProperty("updated_at")]

        public ulong UpdatedAt { get; set; }
        [JsonProperty("created_at")]
        public ulong CreatedAt { get; set; }
        [JsonProperty("locked")]
        public bool Locked { get; set; }

        public EmailSenderSummary CreateSummary()
        {
            return new EmailSenderSummary()
            {
                Id = Id.ToString(),
                Name = NickName,
                Key = From.Email,
                Icon = "icon-pz-programmer",
            };
        }

        public List<string> GetFormFields()
        {

            return new List<string>()
            {
                nameof(Name),
                nameof(FromName),
                nameof(FromAddress),
                nameof(ReplyToName),
                nameof(ReplyToAddress),
                nameof(Address),
                nameof(City),
                nameof(State),
                nameof(Zip),
                nameof(Country)
            };
        }
    }

    [EntityDescription(Domains.EmailServicesDomain, UserAdminResources.Names.EmailSenders_Title, UserAdminResources.Names.EmailSender_Description, UserAdminResources.Names.EmailSender_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),
      Icon: "icon-pz-programmer", EditUIUrl: "/marketing/emailcampaigns/sender/{id|", CreateUIUrl: "/marketing/emailcampaigns/sender/add", ListUIUrl: "/marketing/emailcampaigns/senders",
      SaveUrl: "/api/email/sender", GetUrl: "/api/email/sender/{id}", GetListUrl: "/api/email/senders", DeleteUrl: "/api/email/sender/{id}", FactoryUrl: "/api/email/sender/factory")]
    public class EmailSenderSummary : SummaryData
    {


    }
}