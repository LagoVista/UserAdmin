using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Contacts
{
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
}
