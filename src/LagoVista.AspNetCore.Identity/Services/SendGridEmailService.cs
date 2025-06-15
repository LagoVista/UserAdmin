using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Contacts;
using LagoVista.UserAdmin.Models.Users;
using Newtonsoft.Json;
using OpenTelemetry.Trace;
using Prometheus;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    public class SendGridEmailService : IEmailSender
    {
        protected static readonly Counter SendEmailMessage = Metrics.CreateCounter("nuviot_send_email", "Send Email Message to a user account", "entity");

        private readonly IAppUserRepo _appUserRepo;
        private readonly ILagoVistaAspNetCoreIdentityProviderSettings _settings;
        private readonly IBackgroundServiceTaskQueue _taskQueue;
        private readonly IAppConfig _appConfig;
        private readonly IAdminLogger _adminLogger;
        private readonly IOrganizationRepo _organizationRepo;
        
        public List<string> GetRequiredImportFields()
        {
            return new List<string>()
            {
                "email",
                "first_name",
                "last_namne",
                "organization",
                "organiation_id",
                "custom_list_id",
                "custom_list"
            };
        }


        public SendGridEmailService(ILagoVistaAspNetCoreIdentityProviderSettings settings, IOrganizationRepo organizationRepo, IAppUserRepo appuserRepo, IBackgroundServiceTaskQueue taskQueue, IAppConfig appConfig, IAdminLogger adminLogger)
        {
            _settings = settings;
            _appConfig = appConfig;
            _adminLogger = adminLogger;
            _taskQueue = taskQueue;
            _appUserRepo = appuserRepo;
            _organizationRepo = organizationRepo;
        }

        public bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false; // suggested by @TK-421
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

     
        private class SendGridListResponse
        {
            public string name { get; set; }
            public string id { get; set; }
        }

        public class SendGridListRequest
        {
            public string name { get; set; }
        }

        public class SendGridSegmentRequest
        {
            public string name { get; set; }
            public string query_dsl { get; set; }
        }

        public class SendGridSegmentResponse
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        public class SendGridDesignRequest
        {

            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("editor", NullValueHandling = NullValueHandling.Ignore)]
            public string Editor { get; set; }

            [JsonProperty("subject", NullValueHandling = NullValueHandling.Ignore)]
            public string Subject { get; set; }

            [JsonProperty("html_content", NullValueHandling = NullValueHandling.Ignore)]
            public string HtmlContent { get; set; }

            [JsonProperty("plain_content", NullValueHandling = NullValueHandling.Ignore)]
            public string PlainContent { get; set; }
        }

        public class SendGridDesignResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }
        }

        public class SendGridDesignListResponse
        {
            [JsonProperty("result")]
            public List<SendGridDesignListItem> Results { get; set; }
        }

        public class SendGridDesignListItem
        {
            [JsonProperty("id")]
            public string Id { get; set; }


            private string _thumbnailUrl;
            [JsonProperty("thumbnail_url")]
            public string ThumbnailUrl 
            { 
                get { return _thumbnailUrl; }
                set
                {
                    if (value != null)
                    {
                        if (!value.StartsWith("https"))
                            _thumbnailUrl = $"https:{value}";
                        else
                        {
                            _thumbnailUrl = value;
                        }
                    }
                    else
                    {
                        _thumbnailUrl = null;
                    }
                        
                }
            }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("editor")]
            public string Editor { get; set; }
        }

        public class SendGrdiSignelSendScheduleResponse
        {
            [JsonProperty("send_at")]
            public string SendAt { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }
        }

        public class SendGrdiSignelSendScheduleRequest
        {
            [JsonProperty("send_at")]
            public string SendAt { get; set; }
        }


        public class SendGridSingleSendResultAbTest
        {
            [JsonProperty("abtest")]
            public string Type { get; set; }

            [JsonProperty("winner_criteria")]
            public string WinnerCriteria { get; set; }

            [JsonProperty("test_percentage")]
            public int TestPercentage { get; set; }

            [JsonProperty("duration")]
            public string Duration { get; set; }


            [JsonProperty("winning_template_id")]
            public string WinningTemplateId{ get; set; }


            [JsonProperty("winner_selected_at")]
            public string WinnerSelectedAt{ get; set; }

            [JsonProperty("expiration_date")]
            public string ExpirationDate { get; set; }
        }

        public class SendGridSingleSendResultRequest
        {
            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Names { get; set; }

            [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> Status { get; set; }

            [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> Categories { get; set; }
        }


        public class SendGridSingleSendResultResponse
        {
            [JsonProperty("result")]
            public List<SendGridSingleSendResultItem> Results { get; set; }

        }

        public class SendGridSingleSendResultItem
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("categories")]
            public List<string> Categories { get; set; } = new List<string>();


            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("send_at")]
            public string SendAt { get; set; }

            [JsonProperty("is_abtest")]
            public bool IsABTest { get; set; }

            [JsonProperty("updated_at")]
            public string UpdatedAt { get; set; }

            [JsonProperty("created_at")]
            public string CreatedAt { get; set; }
        }

        public class SendGridSingleSendList
        {
            [JsonProperty("list_ids", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> ListIds { get; set; } = new List<string>();
            [JsonProperty("segment_ids", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> SegmentIds { get; set; } = new List<string>();
            [JsonProperty("all", NullValueHandling = NullValueHandling.Ignore)]
            public bool All { get; set; }
        }

        public class SendGridSingleSendEmailConfig
        {
            [JsonProperty("subject", NullValueHandling = NullValueHandling.Ignore)]
            public string Subject { get; set; }
            [JsonProperty("html_content", NullValueHandling = NullValueHandling.Ignore)]
            public string HtmlContent { get; set; }
            [JsonProperty("plain_content", NullValueHandling = NullValueHandling.Ignore)]
            public string PlainContent { get; set; }
            [JsonProperty("generate_plain_content", NullValueHandling = NullValueHandling.Ignore)]
            public string GeneratePlainCOntent { get; set; }
            [JsonProperty("design_id", NullValueHandling = NullValueHandling.Ignore)]
            public string DesignId { get; set; }
            [JsonProperty("editor", NullValueHandling = NullValueHandling.Ignore)]
            public string Editor { get; set; }
            
            [JsonProperty("suppression_group_id", NullValueHandling = NullValueHandling.Ignore)]
            public int SuppressionGroupId{ get; set; }


            [JsonProperty("custom_unsubscribe_url", NullValueHandling = NullValueHandling.Ignore)]
            public string CustomUnsubscribeUrl { get; set; }
            [JsonProperty("sender_id", NullValueHandling = NullValueHandling.Ignore)]
            public int SenderId { get; set; }
            [JsonProperty("ip_pool", NullValueHandling = NullValueHandling.Ignore)]
            public string IpPool { get; set; }
        }

        public class SendGridSingleSendRequest
        {
            [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
            public string Name { get; set; }

            [JsonProperty("categories", NullValueHandling = NullValueHandling.Ignore)]
            public List<string> Categories { get; set; } = new List<string>();

            [JsonProperty("send_to", NullValueHandling = NullValueHandling.Ignore)]
            public SendGridSingleSendList SendTo { get; set; } = new SendGridSingleSendList();

            [JsonProperty("email_config", NullValueHandling = NullValueHandling.Ignore)]
            public SendGridSingleSendEmailConfig EmailConfig { get; set; } 
        }

        public class SendGridSingleSendResponse
        {
            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("send_to")]
            public SendGridSingleSendList SendTo { get; set; }

            [JsonProperty("email_config")]
            public SendGridSingleSendEmailConfig EmailConfig { get; set; }
        }


        public class SendGridSenderRequestAddress
        {
            [JsonProperty("email")]
            public string Email { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
        }

        public class SendGridSenderRequest
        {
            [JsonProperty("nickname")]
            public string Name { get; set; }

            [JsonProperty("from")]
            public SendGridSenderRequestAddress From { get; set; }

            [JsonProperty("reply_to")]
            public SendGridSenderRequestAddress ReplyTo { get; set; }

            [JsonProperty("address")]
            public string Address { get; set; }

            [JsonProperty("address_2")]
            public string OrganizationId { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("state")]
            public string State { get; set; }

            [JsonProperty("zip")]
            public string Zip { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("verified")]
            public bool Verified { get; set; }

            public static SendGridSenderRequest Create(AppUser user)
            {
                return new SendGridSenderRequest()
                {
                    Name = $"AppUser {user.Name}",
                    From = new SendGridSenderRequestAddress()
                    {
                        Email = user.Email,
                        Name = user.Name,
                    },
                    ReplyTo = new SendGridSenderRequestAddress()
                    {
                        Email = user.Email,
                        Name = user.Name,
                    },
                    Address = user.Address1,
                    City = user.City,
                    State = user.State,
                    Zip = user.PostalCode,
                    Country = user.Country
                };
            }
        }

        public class SendGridSenderVerifiedResponse
        {
            [JsonProperty("status")]
            public bool Status { get; set; }

            [JsonProperty("reason")]
            public string Reason { get; set; }
        }

        public class SendGridSenderResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("verified")]
            public SendGridSenderVerifiedResponse Verified { get; set; }
        }
        public class SendGridImportJobResponseHeader
        {
            [JsonProperty("header")]
            public string Header { get; set; }
            [JsonProperty("value")]
            public string Value { get; set; }
        }

        public class SendGridGetSegmentListsResponse
        {
            [JsonProperty("results")]
            public List<SendGridGetSegmentsResponse> Results { get; set; }
        }

        public class SendGridGetSegmentsResponseStatus
        {
            [JsonProperty("query_validation")]
            public string QueryValidation { get; set; }
            [JsonProperty("error_message")]
            public string ErrorMessage { get; set; }
        }

        public class SendGridGetSegmentsResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("contacts_count")]
            public int ContactCount { get; set; }
            [JsonProperty("sample_updated_at")]
            public string SampleUpdated { get; set; }
            [JsonProperty("next_sample_at")]
            public string NextSampleUpdated { get; set; }
            [JsonProperty("status")]
            public SendGridGetSegmentsResponseStatus Status { get; set; }
        }


        public class SendGridImportJobResponse
        {
            [JsonProperty("job_id")]
            public string JobId { get; set; }

            [JsonProperty("upload_uri")]
            public string UploadUri { get; set; }

            [JsonProperty("upload_headers")]
            public List<SendGridImportJobResponseHeader> UploadHeaders { get; set; }
        }

        public class SendGridFieldDefinitionsField
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("field_type")]
            public string FieldType { get; set; }

        }

        public class SendGridFieldDefinitions
        {
            [JsonProperty("custom_fields")]
            public List<SendGridFieldDefinitionsField> CustomFields { get; set; }

            [JsonProperty("reserved_fields")]
            public List<SendGridFieldDefinitionsField> ReservedFields { get; set; }
        }


        private class SendGridContact
        {
            public SendGridContact(Contact contact, EntityHeader org)
            {
                email = contact.Email;
                first_name = contact.FirstName;
                last_name = contact.LastName;
                phone_number = contact.Phone;

                custom_fields.organization = org.Text;
                custom_fields.organization_id = org.Id;
                custom_fields.nuviot_contact_id = contact.Id;
            }

            public SendGridContact(Contact contact, Company company)
            {
                email = contact.Email;
                first_name = contact.FirstName;
                last_name = contact.LastName;
                phone_number = contact.Phone;
                city = company.City;
                state_province_region = company.State;
                postal_code = company.Zip;

                custom_fields.industry = company.Industry?.Text ?? "?";
                custom_fields.industry_id = company.Industry?.Id ?? "?";
                custom_fields.niche_id = company.IndustryNiche?.Id ?? "?";
                custom_fields.organization = company.OwnerOrganization.Text;
                custom_fields.organization_id = company.OwnerOrganization?.Id;
                custom_fields.nuviot_contact_id = contact.Id;
                custom_fields.company = company.Name;
                custom_fields.company_id = company.Id;
            }

            public string email { get; set; }

            public string id { get; set; }

            public string first_name { get; set; }
            public string last_name { get; set; }

            public string city { get; set; }
            public string state_province_region { get; set; }
            public string postal_code { get; set; }
            public string country { get; set; }
            public string phone_number { get; set; }


            public SendGridContactCustomFields custom_fields
            {
                get; set;
            } = new SendGridContactCustomFields();
        }

        private class SendGridContactSearchResuts
        {
            public List<SendGridContact> result { get; set; }
            public int contact_count { get; set; }
        }


        public class SendGridContactCustomFields
        {
            public string company { get; set; } = "?";
            public string company_id { get; set; } = "?";
            public string industry { get; set; } = "?";
            public string industry_id { get; set; } = "?";
            public string niche_id { get; set; } = "?";
            public string nuviot_contact_id { get; set; } = "";
            public string organization { get; set; } = "";
            public string organization_id { get; set; } = "";
        }


        private async Task<InvokeResult<string>> RegisterContactAsync(SendGridContact contact, EntityHeader org, EntityHeader user)
        {
            var client = new SendGrid.SendGridClient(_settings.SmtpServer.Password);

            var contacts = new List<SendGridContact>();
            contacts.Add(contact);

            var json = JsonConvert.SerializeObject(contacts);

            json = @$"{{""contacts"":{json}}}";

            var response = await client.RequestAsync(
                method: SendGridClient.Method.PUT,
                urlPath: "marketing/contacts",
                requestBody: json
                );

            var result = await response.Body.ReadAsStringAsync();

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[SendGridEmailService__RegisterContactAsync]", $"Contact: {contact.email}", response.StatusCode.ToString().ToKVP("statusCode"));

            return InvokeResult<string>.Create(result);
        }




        public async Task<InvokeResult<string>> CreateEmailListAsync(string listName, EntityHeader org, EntityHeader user)
        {
            switch (_appConfig.Environment)
            {
                case Environments.Local:
                case Environments.LocalDevelopment:
                case Environments.Development:
                    listName = $"env:Dev, {listName}";
                    break;
                case Environments.Testing:
                    listName = $"env:Test, {listName}";
                    break;
                case Environments.Beta:
                    listName = $"env:Beta, {listName}";
                    break;
            }

            var client = new SendGrid.SendGridClient(_settings.SmtpServer.Password);

            var listRequest = new SendGridListRequest()
            {
                name = listName
            };

            var response = await client.RequestAsync(
                method: SendGridClient.Method.POST,
                urlPath: "marketing/lists",
                requestBody: JsonConvert.SerializeObject(listRequest)
            );

            var result = await response.Body.ReadAsStringAsync();
            var listResponse = JsonConvert.DeserializeObject<SendGridListResponse>(result);

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[SendGridEmailService__CreateEmailListAsync]", $"List: {listName}", response.StatusCode.ToString().ToKVP("statusCode"));


            return InvokeResult<string>.Create(listResponse.id);
        }
        public async Task<InvokeResult> AddContactToListAsync(string listId, string contactId, EntityHeader org, EntityHeader user)
        {
            var client = new SendGrid.SendGridClient(_settings.SmtpServer.Password);
            var response = await client.RequestAsync(
                method: SendGridClient.Method.POST,
                urlPath: $"contactdb/lists/{listId}/recipients/{contactId}"
            );

            var result = await response.Body.ReadAsStringAsync();

            var bldr = new StringBuilder();
            foreach (var header in response.Headers)
                bldr.Append($"{header.Key} - {String.Join(',', header.Value)}");

            _adminLogger.Trace($"[SendGridEmailService__AddContactToListAsync] Status Code {response.StatusCode} - {result} - Headers: {bldr}");

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> SendInBackgroundAsync(string email, string subject, string body, EntityHeader org, EntityHeader user)
        {
            await _taskQueue.QueueBackgroundWorkItemAsync(async (cncl) =>
            {
                await SendAsync(email, subject, body, org, user);
            });

            return InvokeResult.Success;
        }
 
        public async Task<InvokeResult> SendToAppUserAsync(string appuUserId, string subject, string body)
        {
            var user = await _appUserRepo.FindByIdAsync(appuUserId);
            if (user == null)
                return InvokeResult.FromError($"User not found for id: {appuUserId}");

            return await SendAsync(user.Email, subject, body);
        }

        public Task<InvokeResult> SendAsync(string email, string subject, string body, EntityHeader org, EntityHeader user)
        {
            return SendAsync(email, subject, body);
        }


        public async Task<InvokeResult> SendAsync(string email, string subject, string body)
        {
            //IT IS POSSIBLE THAT ORG IS NULL HERE, IF THAT"S THE CASE WE NEED TO FALLBACK TO A MASTER ORG, this will be the case when the user does 
            //not have a default org set for them and we need to send them email.

            try
            {
                body = $@"
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd""><html xmlns=""http://www.w3.org/1999/xhtml""><head>
<meta name=""viewport"" content=""width=device-width, initial-scale=1, minium-scale=1, maxium-scale=1"">
	<title>NuvIoT - IoT Eneablement Platform</title>
	<style type=""text/css"">

    body
	{{
        width: 100% !important;
		-webkit-text-size-adjust: 100%;
		-ms-text-size-adjust: 100%;
		margin: 0;
		padding: 0;
	}}

.ExternalClass
	{{
        width: 100%;
	}}

#backgroundTable
		{{
            margin: 0;
			padding: 0;
			width: 100% !important;
			line-height: 100% !important;
		}}

	img
		{{
            outline: none;
			text-decoration: none;
			-ms-interpolation-mode: bicubic;
		}}

		a img
		{{
            border: none;
		}}

		.image-fix
		{{
            display: block;
		}}

			Bring inline: Yes. */
			p
			{{
                margin: 1em 0;
			}}

			/* Hotmail header color reset
			Bring inline: Yes. */
			h1, h2, h3, h4, h5
			{{
                color: #333333 !important;
				font-weight:400;
			}}

			h6
			{{
                color: #666666 !important;
			}}

				h1 a, h2 a, h3 a, h4 a, h5 a, h6 a
				{{
                    color: #2172ba !important;
				}}

					h1 a:active, h2 a:active, h3 a:active, h4 a:active, h5 a:active, h6 a:active
					{{
                        color: #2172ba !important;
					}}

					h1 a:visited, h2 a:visited, h3 a:visited, h4 a:visited, h5 a:visited, h6 a:visited
					{{
                        color: #2172ba !important;
					}}

			/* Outlook 07, 10 Padding issue fix */
			table td
			{{
                border - collapse: collapse;
			}}
            .ssrContent{{
                font - size: 12px;
            }}
			/* Remove spacing around Outlook 07, 10 tables */
			table
			{{ /*border-collapse:collapse;*/
                mso - table - lspace: 0pt;
				mso-table-rspace: 0pt;
			}}

			a
			{{
                color: #2172ba;
				text-decoration: none;
			}}

			a:hover {{
                color: #2172ba;
				text-decoration: underline;
			}}

			.ad-width{{
                width: 275px !important;
			}}
			/*.non-mobile-image, .non-mobile-email{{
                display:block !important;
			}}*/
			td[class=""mobile-email""], td[class=""mobile-image""]

            {{
                    display: none !important;
                    max - height: 0 !important;
                    width: 0 !important;
                    font - size: 0 !important;
                    line - height: 0 !important;
                    padding: 0 !important;
                    mso - hide: all; /* hide elements in Outlook 2007-2013 */
                }}
                /*div[class=""mobile-mp-card-info""] {{

                        margin:0 !important;
                        padding:90px 0 0 0 !important;                
                    }}*/
                /* Apple Mail to limit that table's size on desktop*/
                @media screen and(min - width: 600px) {{

                .container {{
                        width: 601px!important;
                    }}
                }}
                @media only screen and(max-width: 525px)
			{{
                    div[class= ""mobile - mp - card - section""] {{
                    padding:0 !important;
                    margin:-25px 0 0 -150px !important;
                }}
}}
/* More Specific Targeting */
/* MOBILE TARGETING 480px */
@media only screen and(min-width: 480px)
{{

    /*table[class=""table""], td[class=""cell""]
    {{
        width: 304px !important;
    }}*/

    td[class= ""mobile - ad - spacing""]
			{{
				padding-bottom: 15px !important;
			}}
                td[class=""mobile-image""]
			{{
				display: inline-block !important;
			}}
             
			td[class=""non-mobile-image""] /*, .non-mobile-email*/
			{{
				display: none !important;
				max-height: 0 !important;
				width: 0 !important;
				font-size: 0 !important;
				line-height: 0 !important;
				padding: 0 !important;
				/*mso-hide: all;  hide elements in Outlook 2007-2013 */
			}}
			/*.mobile-email{{
				display:block !important;
			}}*/
			tr[class=""mobile-footer""]
			{{
				background-color: #ffffff !important;
			}}

				body[class=""mobile-body""]
				{{
					background-color: #ffffff !important;
				}}
				div[class=""navigation""] {{
					width:100% !important;
				}}
				div[class=""navigation""] a {{
					display:block !important;
					text-decoration:none !important;
					text-align:center !important;
					font-size:16px !important;
				}}
				/*Controlling phone number linking for mobile. */
				a[href ^= ""tel""], a[href ^= ""sms""]
				{{
					text-decoration: none;
					color: #2172ba;
					pointer-events: none;
					cursor: default;
				}}

				.mobile_link a[href ^= ""telephone""], .mobile_link a[href ^= ""sms""]
				{{
					text-decoration: none;
					color: #2172ba !important;
					pointer-events: auto;
					cursor: default;
				}}

				a[class=""mobile-show""], a[class=""mobile-hide""]
				{{
					display: block !important;
					color: #2172ba !important;
					border-radius: 20px;
					padding: 0 8px;
					text-decoration: none;
					font-weight: bold;
					font-family: ""Helvetica Neue"", Helvetica, sans-serif;
					font-size: 11px;
					position: absolute;
					top: 25px;
					right: 10px;
					text-align: center;
					width: 40px;
				}}
				div[class=""ad-width""]{{
					width:300px !important;
				}}
            }}
			@media only screen and(min-device-width: 768px) and(max-device-width: 1024px)
{{
    /*ipad (tablets, smaller screens, etc) */
    a[href ^= ""tel""],a[href ^= ""sms""]

                {{
        text-decoration: none;
        color: #2172ba;
					pointer-events: none;
        cursor: default;
    }}

				.mobile_link a[href ^= ""tel""], .mobile_link a[href ^= ""sms""]
                {{
        text - decoration: default;
        color: #2172ba !important;
					pointer - events: auto;
        cursor: default;
    }}
}}

@media only screen and(-webkit-min-device-pixel-ratio: 2)
{{
    /*iPhone 4g styles */
    tr[class= ""non-mobile-image""],img[class= ""non - mobile - image""], table[class=""non-mobile-email""]
				{{
					display: none !important;
					max-height: 0;
					width: 0;
					font-size: 0;
					line-height: 0;
					padding: 0;
					mso-hide: all; /* hide elements in Outlook 2007-2013 */
				}}

    </style>

<body style=""padding: 0; margin: 0; -webkit-text-size-adjust: none; -ms-text-size-adjust: 100%; background-color: #e6e6e6;"" class=""mobile-body"">
		<!--[if (gte mso 9)|(IE)]>
                
		<![endif]-->
		<table width=""100%"" cellpadding=""20"" cellspacing=""0"" border=""0"" style=""max-width:600px; background-color:white""><div>
            <tr>
                <td>
                    <img src=""{_appConfig.AppLogo}"" />
                    <h1>{_appConfig.AppName}</h1>
                    <div style=""width:550px"">
                    {body}
                    </div>
                </td>
            </tr>
        </table>
    <p>Please do not reply to this email as it is an unmonitored address.</p>
    <a href=""mailto:support@software-logistics.com"">Contact Support</a>

</div>
</body>
</html>";


                var msg = new SendGridMessage();
                msg.AddTo(email);
                msg.From = new SendGrid.Helpers.Mail.EmailAddress(_settings.SmtpFrom, "NuvIoT Notifications");
                msg.Subject = subject;
                msg.AddContent(MediaTypeNames.Text.Html, body);

                if (String.IsNullOrEmpty(_settings.SmtpServer.Password))
                {
                    throw new ArgumentNullException("SMTP Server API Key (SendGrid) is null or empty");
                }

                var client = new SendGrid.SendGridClient(_settings.SmtpServer.Password);
                var response = await client.SendEmailAsync(msg);

                SendEmailMessage.Inc();

                _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Verbose, "[SendGridEmailServices_SendAsync]", $"[SendGridEmailServices_SendAsync] EmailSent - {email}: {subject}",
                    new System.Collections.Generic.KeyValuePair<string, string>("Subject", subject),
                    new System.Collections.Generic.KeyValuePair<string, string>("to", email));

                return InvokeResult.Success;
            }
            catch (Exception ex)
            {
                _adminLogger.AddException("SendGridEmailServices_SendAsync", ex,
                    new System.Collections.Generic.KeyValuePair<string, string>("Subject", subject),
                    new System.Collections.Generic.KeyValuePair<string, string>("to", email));

                return InvokeResult.FromException("SendGridEmailServices_SendAsync", ex);
            }

        }

        public async Task<InvokeResult<string>> RegisterContactAsync(Contact contact, Company company, EntityHeader org, EntityHeader user)
        {
            var sendGridContact = new SendGridContact(contact, company);

            var result = await RegisterContactAsync(sendGridContact, org, user);
            return result;
        }

        public async Task<InvokeResult> DeleteContactByEmailAsync(string email)
        {
            var query = "email = '" + email + "'";
            var client = new SendGrid.SendGridClient(_settings.SmtpServer.Password);
            var results = await client.RequestAsync(BaseClient.Method.POST, query,
                urlPath: "/v3/marketing/contacts/search");

            var result = await results.Body.ReadAsStringAsync();
            var searchResults = JsonConvert.DeserializeObject<SendGridContactSearchResuts>(result);


            var deleteResults = await client.RequestAsync(BaseClient.Method.DELETE, queryParams: $"id={searchResults.result.Single().id}", urlPath: "/v3/marketing/contacts");

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<string>> RegisterContactAsync(Contact contact, EntityHeader org, EntityHeader user)
        {
            var sendGridContact = new SendGridContact(contact, org);
            var result = await RegisterContactAsync(sendGridContact, org, user);
            return result;
        }

        public async Task<InvokeResult<string>> AddEmailDesignAsync(string name, string subject, string htmlContents, string plainTextContent, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.PostAsJsonAsync<SendGridDesignRequest>("https://api.sendgrid.com/v3/designs", new SendGridDesignRequest()
                {
                    Name = name,
                    Subject = subject,
                    Editor = "code",
                    HtmlContent = htmlContents,
                    PlainContent = plainTextContent
                });

                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {

                    return InvokeResult<string>.FromError(strResponse);
                }

                var sgResponse = JsonConvert.DeserializeObject<SendGridDesignResponse>(strResponse);

                return InvokeResult<string>.Create(sgResponse.Id);
            }
        }

        public async Task<InvokeResult> DeleteEmailDesignAsync(string id, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.DeleteAsync($"https://api.sendgrid.com/v3/designs/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return InvokeResult.FromError(response.StatusCode.ToString());
                }

                return InvokeResult.Success;
            }
        }

        public async Task<ListResponse<EmailDesign>> GetEmailDesignsAsync(EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.GetAsync($"https://api.sendgrid.com/v3/designs");
                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return ListResponse<EmailDesign>.FromError(strResponse);
                }

                Console.WriteLine(strResponse);

                var sgResponse = JsonConvert.DeserializeObject<SendGridDesignListResponse>(strResponse);

                return ListResponse<EmailDesign>.Create(sgResponse.Results.Select(dl => new EmailDesign() { Id = dl.Id, Name = dl.Name, ThumbmailImage = dl.ThumbnailUrl }));
            }
        }

        public async Task<InvokeResult> UpdateEmailDesignAsync(string id, string name, string subject, string htmlContents, string plainTextContent, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                var json = JsonConvert.SerializeObject(new SendGridDesignRequest()
                {
                    Name = name,
                    Subject = subject,
                    HtmlContent = htmlContents,
                    PlainContent = plainTextContent
                });

                var path = $"https://api.sendgrid.com/v3/designs/{id}";

                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.PatchAsync(path, new StringContent(json, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    return InvokeResult.Success;
                }

                var errorMessage = await response.Content.ReadAsStringAsync();

                return InvokeResult.FromError(errorMessage);
            }
        }

        public async Task<InvokeResult<string>> CreateEmailListAsync(string listName, string customField, string id, EntityHeader org, EntityHeader user)
        {
            var client = new SendGrid.SendGridClient(_settings.SmtpServer.Password);

            var segmentRequest = new SendGridSegmentRequest()
            {
                name = listName,
                query_dsl = $"SELECT c.contact_id, c.updated_at FROM contact_data as c where c.{customField} = '{id}' and c.organization_id = '{org.Id}' "
            };

            var json = JsonConvert.SerializeObject(segmentRequest);

            var response = await client.RequestAsync(
                method: SendGridClient.Method.POST,
                urlPath: "marketing/segments/2.0",
                requestBody: json
            );

            var result = await response.Body.ReadAsStringAsync();
            var listResponse = JsonConvert.DeserializeObject<SendGridListResponse>(result);

            _adminLogger.AddCustomEvent(Core.PlatformSupport.LogLevel.Message, "[SendGridEmailService__CreateEmailListAsync]", $"List: {listName}", listName.ToKVP("listName"), customField.ToKVP("customField"), response.StatusCode.ToString().ToKVP("statusCode"));


            return InvokeResult<string>.Create(listResponse.id);
        }

        public async Task<InvokeResult<string>> SendToListAsync(string name, string listId, string senderId, string designId, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);

                var ssRequest = new SendGridSingleSendRequest()
                {
                    Name = name,
                    EmailConfig = new SendGridSingleSendEmailConfig()
                    {
                        DesignId = designId,
                        SenderId = Convert.ToInt32(senderId),
                        SuppressionGroupId = 26579
                    },
                    SendTo = new SendGridSingleSendList()
                    {                       
                    }
                };

                ssRequest.Categories.Add(org.Id);

                ssRequest.SendTo.SegmentIds.Add(listId);

                var json = JsonConvert.SerializeObject(ssRequest);
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var path = $"https://api.sendgrid.com/v3/marketing/singlesends";
                var response = await client.PostAsync(path, content);
                var strContent = await response.Content.ReadAsStringAsync();
                if(!response.IsSuccessStatusCode)
                {
                    return InvokeResult<string>.FromError(strContent);
                }

                var ssResponse = JsonConvert.DeserializeObject < SendGridSingleSendResponse>(strContent);

                return InvokeResult<string>.Create(ssResponse.Id);
            }
        }

        public async Task<InvokeResult> DeleteEmailListSendAsync(string id, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var path = $"https://api.sendgrid.com/v3/marketing/singlesends/{id}";
                Console.WriteLine($"delete list with: {path}");
                var response = await client.DeleteAsync(path);
                var strContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return InvokeResult.FromError(strContent);
                }

                return InvokeResult.Success;
            }
        }

        public async Task<ListResponse<EmailListSend>> GetEmailListSendsAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var request = new SendGridSingleSendResultRequest();
                request.Categories = new List<string>();
                request.Categories.Add(org.Id);
                var path = $"https://api.sendgrid.com/v3/marketing/singlesends/search";
                var response = await client.PostAsJsonAsync(path, request);
                var strContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return ListResponse<EmailListSend>.FromError(strContent);
                }

                Console.WriteLine(strContent);

                var sgResponse = JsonConvert.DeserializeObject<SendGridSingleSendResultResponse>(strContent);
                return ListResponse<EmailListSend>.Create(sgResponse.Results.Select(res => new EmailListSend()
                {
                    Name = res.Name,
                    Id = res.Id,
                    CreateDate = DateTime.Parse(res.CreatedAt).ToJSONString(),
                    Status = res.Status,
                    StatusDate = DateTime.Parse(res.UpdatedAt).ToJSONString(),
                }));
            }
        }

        public async Task<InvokeResult<string>> ScheduleEmailSendListAsync(string singleSendId, string scheduleDate, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);

                var ssRequest = new SendGrdiSignelSendScheduleRequest()
                {
                    SendAt = scheduleDate
                };


                var json = JsonConvert.SerializeObject(ssRequest);
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var path = $"https://api.sendgrid.com/v3/marketing/singlesends/{singleSendId}/schedule";
                var response = await client.PostAsync(path, content);
                var strContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return InvokeResult<string>.FromError(strContent);
                }

                var ssResponse = JsonConvert.DeserializeObject<SendGridSingleSendResponse>(strContent);

                return InvokeResult<string>.Create(ssResponse.Id);
            }
        }

        public async Task<InvokeResult<string>> SendEmailSendListNowAsync(string singleSendId,  EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);

                var ssRequest = new SendGrdiSignelSendScheduleRequest()
                {
                    SendAt = "now"
                };

                var json = JsonConvert.SerializeObject(ssRequest);
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var path = $"https://api.sendgrid.com/v3/marketing/singlesends/{singleSendId}/schedule";
                var response = await client.PutAsync(path, content);
                var strContent = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return InvokeResult<string>.FromError(strContent);
                }

                var ssResponse = JsonConvert.DeserializeObject<SendGridSingleSendResponse>(strContent);

                return InvokeResult<string>.Create(ssResponse.Id);
            }
        }

        public async Task<InvokeResult> SendInBackgroundAsync(Email email, EntityHeader org, EntityHeader user)
        {
            await _taskQueue.QueueBackgroundWorkItemAsync(async (token) =>
            {
                await SendAsync(email, org, user);
            });

            return InvokeResult.Success;
        }

        public async Task<InvokeResult<string>> SendAsync(Email email, EntityHeader org, EntityHeader user)
        {
            var msg = new SendGridMessage();
            foreach (var addr in email.To)
                msg.AddTo(addr.Address, addr.Name);

            msg.From = new SendGrid.Helpers.Mail.EmailAddress(email.From.Address, email.From.Name);

            if (email.ReplyTo != null && email.ReplyTo.Address != email.From.Address)
                msg.ReplyTo = new SendGrid.Helpers.Mail.EmailAddress(email.ReplyTo.Address, email.ReplyTo.Name);

            msg.Subject = email.Subject;

            msg.AddContent(MediaTypeNames.Text.Html, email.Content);

            var client = new SendGrid.SendGridClient(_settings.SmtpServer.Password);
            var response = await client.SendEmailAsync(msg);


            if (response.IsSuccessStatusCode)
            {
                SendEmailMessage.Inc();
                
                var messageIds = response.Headers.GetValues("X-Message-Id");
                return InvokeResult<string>.Create(messageIds.FirstOrDefault());
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _adminLogger.AddError("[SendGridEmailService__CreateEmailListAsync]", body, response.StatusCode.ToString().ToKVP("statusCode"));
                return InvokeResult<string>.FromError(String.IsNullOrEmpty(body) ? response.StatusCode.ToString() : body);
            }
        }


        public async Task<InvokeResult<AppUser>> AddEmailSenderAsync(AppUser sender, string nickNameOverRide, EntityHeader org, EntityHeader user)
        {
            var sendGridSender = SendGridSenderRequest.Create(sender);
            sendGridSender.OrganizationId = org.Id;
            if(!String.IsNullOrEmpty(nickNameOverRide))
                sendGridSender.Name = nickNameOverRide;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.PostAsJsonAsync($"https://api.sendgrid.com/v3/marketing/senders", sendGridSender);

                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {

                    return InvokeResult<AppUser>.FromError(strResponse);
                }

                var sgResponse = JsonConvert.DeserializeObject<SendGridSenderResponse>(strResponse);

                sender.SendGridSenderId = sgResponse.Id;
                sender.SendGridVerified = sgResponse.Verified.Status;
                sender.SendGridVerifiedFailedReason = sgResponse.Verified.Reason;

                return InvokeResult<AppUser>.Create(sender);
            }
        }

        public async Task<InvokeResult> DeleteEmailSenderAsync(string id, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.DeleteAsync($"https://api.sendgrid.com/v3/marketing/senders/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    return InvokeResult.FromError(response.StatusCode.ToString());
                }

                return InvokeResult.Success;
            }
        }

        public async Task<InvokeResult<string>> StartImportJobAsync(string fieldMappings, Stream stream, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var fieldResponse = await client.GetAsync("https://api.sendgrid.com/v3/marketing/field_definitions");

                var strResponse = await fieldResponse.Content.ReadAsStringAsync();

                if (!fieldResponse.IsSuccessStatusCode)
                {
                    return InvokeResult<string>.FromError(strResponse);
                }

                var fields = JsonConvert.DeserializeObject<SendGridFieldDefinitions>(strResponse);

                var mappings = fieldMappings.Split(','); // the name of the field isn't what we want to pass in, we need to look it up and then get the id for the column...go figure...
                var mappedMappings = new List<string>();
                foreach (var mapping in mappings)
                {
                    var mappedFieldName = mapping.Trim();
                    var field = fields.CustomFields.FirstOrDefault(fld => fld.Name == mappedFieldName);
                    if (field == null)
                        field = fields.ReservedFields.FirstOrDefault(field => field.Name == mappedFieldName);

                    if (field == null)
                        return InvokeResult<string>.FromError($"[{mappedFieldName}] is not a valid mapping");

                    mappedMappings.Add(field.Id);
                }

                var json = JsonConvert.SerializeObject(new { field_mappings = mappedMappings, file_type = "csv" });
                var response = await client.PutAsJsonAsync($"https://api.sendgrid.com/v3/marketing/contacts/imports", new { field_mappings = mappedMappings, file_type = "csv" });
                strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return InvokeResult<string>.FromError(strResponse);
                }

                var sgResponse = JsonConvert.DeserializeObject<SendGridImportJobResponse>(strResponse);

                using (var streamCLient = new HttpClient())
                {
                    Console.WriteLine($"Post To: {sgResponse.UploadUri}");

                    foreach (var hdr in sgResponse.UploadHeaders)
                    {
                        streamCLient.DefaultRequestHeaders.Add(hdr.Header, hdr.Value);
                        Console.WriteLine($"Add Header {hdr.Header} - {hdr.Value} ");
                    }
                    stream.Seek(0, SeekOrigin.Begin);
                    var uploadStreamResponse = await streamCLient.PutAsync(sgResponse.UploadUri, new StreamContent(stream));
                    strResponse = await uploadStreamResponse.Content.ReadAsStringAsync();

                    if (!uploadStreamResponse.IsSuccessStatusCode)
                    {
                        return InvokeResult<string>.FromError($"Error Code: {uploadStreamResponse.StatusCode} - {strResponse}");
                    }


                    return InvokeResult<string>.Create(sgResponse.JobId);
                }
            }
        }

        public async Task<InvokeResult<EmailImportStatus>> GetImportJobStatusAsync(string jobId, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.GetAsync($"https://api.sendgrid.com/v3/marketing/contacts/imports/{jobId}");
                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return InvokeResult<EmailImportStatus>.FromError(String.IsNullOrEmpty(strResponse) ? response.StatusCode.ToString() : strResponse);
                }

                return InvokeResult<EmailImportStatus>.Create(JsonConvert.DeserializeObject<EmailImportStatus>(strResponse));
            }
        }

        public async Task<InvokeResult> RefreshSegementAsync(string id, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var url = $"https://api.sendgrid.com/v3/marketing/segments/2.0/refresh/{id}";


                var response = await client.PostAsJsonAsync(url, new { user_time_zone = "America/New_York" });
                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[SendGridEmailSender__RefreshSegementAsync] Could not post to: {url}, Response Code: {response.StatusCode}, {strResponse}");

                    return InvokeResult.FromError(String.IsNullOrEmpty(strResponse) ? response.StatusCode.ToString() : $"HTTP {response.StatusCode}");
                }

                return InvokeResult.Success;
            }
        }

        public async Task<InvokeResult> UpdateListAsync(string sendGridListId, string name, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var strContent = new StringContent(JsonConvert.SerializeObject(new { name = name }), Encoding.UTF8, "application/json");
                var response = await client.PatchAsync($"https://api.sendgrid.com/v3/marketing/lists/{sendGridListId}", strContent);
                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return InvokeResult.FromError(strResponse);
                }

                return InvokeResult.Success;
            }
        }

        public async Task<ListResponse<ContactList>> GetListsAsync(EntityHeader org, EntityHeader user)
        {
            var orgRecord = await _organizationRepo.GetOrganizationAsync(org.Id);

            var ns = orgRecord.Namespace;
            
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.GetAsync("https://api.sendgrid.com/v3/marketing/segments/2.0");
                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return ListResponse<ContactList>.FromError(strResponse);
                }

                var sgResponse = JsonConvert.DeserializeObject<SendGridGetSegmentListsResponse>(strResponse);

                var nsFilter = $"orgns:{ns}, ";

                var lists = sgResponse.Results; //Eventually we wan
                var items = lists.Where(cl => cl.Name.StartsWith(nsFilter)).Select(seg => new ContactList()
                {
                    Id = seg.Id,
                    Count = seg.ContactCount,
                    LastUpdated = String.IsNullOrEmpty(seg.SampleUpdated) ? null : DateTime.Parse(seg.SampleUpdated).ToJSONString(),
                    Name = seg.Name.Replace(nsFilter, String.Empty),
                }).OrderBy(lst=>lst.Name);

                return ListResponse<ContactList>.Create(items);
            }
        }

        public async Task<ListResponse<EmailSenderSummary>> GetEmailSendersAsync(EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.GetAsync("https://api.sendgrid.com/v3/marketing/senders");
                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _adminLogger.AddError("[SendGridEmailService__GetEmailSender]", strResponse);
                    throw new Exception($"[SendGridEmailService__GetEmailSender] Could not get email senders, Response Code: {response.StatusCode}, {strResponse}");
                }

                var sgSenders = JsonConvert.DeserializeObject<List<EmailSender>>(strResponse);
                var senders = sgSenders.Where(snd=>snd.OrganizationId == org.Id);
                foreach(var sender in senders)
                {
                    sender.ReplyToAddress = sender.ReplyTo.Email;
                    sender.ReplyToName = sender.ReplyTo.Name;
                    sender.FromAddress = sender.From.Email;
                    sender.FromName = sender.From.Name;
                }
                return ListResponse<EmailSenderSummary>.Create(senders.Select(sndr=>sndr.CreateSummary()));
            }
        }

        public async Task<EmailSender> GetEmailSenderAsync(string id, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.GetAsync("https://api.sendgrid.com/v3/marketing/senders");
                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _adminLogger.AddError("[SendGridEmailService__GetEmailSender]", strResponse);
                    throw new Exception($"[SendGridEmailService__GetEmailSender] Could not get email sender with id: {id}, Response Code: {response.StatusCode}, {strResponse}");
                }

                var sgSenders = JsonConvert.DeserializeObject<List<EmailSender>>(strResponse);
                var senders = sgSenders.Where(snd => snd.OrganizationId == org.Id);
               
                foreach (var sender in senders)
                {
                    sender.Name = sender.NickName;
                    sender.ReplyToAddress = sender.ReplyTo.Email;
                    sender.ReplyToName = sender.ReplyTo.Name;
                    sender.FromAddress = sender.From.Email;
                    sender.FromName = sender.From.Name;
                }
                return senders.SingleOrDefault(sndr=>sndr.Id.ToString() == id);
            }
        }

        private InvokeResult ValidateSendGridSender(EmailSender sender)
        {
            var result = InvokeResult.Success;
            if (String.IsNullOrEmpty(sender.NickName)) result.AddUserError("Name is required.");
            if (String.IsNullOrEmpty(sender.Address)) result.AddUserError("Address is required.");
            if (String.IsNullOrEmpty(sender.City)) result.AddUserError("City is required.");
            if (String.IsNullOrEmpty(sender.State)) result.AddUserError("State is required.");
            if (String.IsNullOrEmpty(sender.Country)) result.AddUserError("Country is required.");
            if (sender.From == null)
                result.AddUserError("From object is null.");
            else
            {
                if (String.IsNullOrEmpty(sender.From?.Email)) result.AddUserError("From Email is required.");
                if (String.IsNullOrEmpty(sender.From?.Name)) result.AddUserError("From Name is required.");
                if (!IsValidEmail(sender.From?.Email)) result.AddUserError("From email address does not appear to be a valid email address.");
            }

            if (sender.ReplyTo == null)
                result.AddUserError("Reply To object is null.");
            else
            {
                if (String.IsNullOrEmpty(sender.ReplyTo?.Email)) result.AddUserError("Reply To Email is required.");
                if (String.IsNullOrEmpty(sender.ReplyTo?.Name)) result.AddUserError("Reply To Name is required.");
                if (!IsValidEmail(sender.ReplyTo?.Email)) result.AddUserError("Reply to email address does not appear to be a valid email address.");
            }

            return result;
        }

        public async Task<InvokeResult> AddEmailSenderAsync(EmailSender sender, EntityHeader org, EntityHeader user)
        {
            sender.ReplyTo = new EmailSenderAddress()
            {
                Email = sender.ReplyToAddress,
                Name = sender.ReplyToName
            };

            sender.From = new EmailSenderAddress()
            {
                Email = sender.FromAddress,
                Name = sender.FromName
            };

            sender.NickName = sender.Name;
            sender.Name = null;
            sender.FromAddress = null;
            sender.FromName = null;
            sender.ReplyToAddress = null;
            sender.ReplyToName = null;

            var result = ValidateSendGridSender(sender);

            if (!result.Successful)
                return result;

            sender.OrganizationId = org.Id;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);

                var createResult = await client.PostAsJsonAsync("https://api.sendgrid.com/v3/marketing/senders", sender);
                var strResponse = await createResult.Content.ReadAsStringAsync();
                if (!createResult.IsSuccessStatusCode)
                {
                    _adminLogger.AddError("[SendGridEmailService__AddEmailSender]", strResponse, createResult.StatusCode.ToString().ToKVP("statusCode"));
                    return InvokeResult.FromError(strResponse);
                }
                
                return InvokeResult.Success;
            }
        }

        public async Task<InvokeResult> UpdateEmailSenderAsync(EmailSender sender, EntityHeader org, EntityHeader user)
        {
            sender.ReplyTo = new EmailSenderAddress()
            {
                Email = sender.ReplyToAddress,
                Name = sender.ReplyToName
            };

            sender.From = new EmailSenderAddress()
            {
                Email = sender.FromAddress,
                Name = sender.FromName
            };

            sender.NickName = sender.Name;
            sender.Name = null;
            sender.FromAddress = null;
            sender.FromName = null;
            sender.ReplyToAddress = null;
            sender.ReplyToName = null;

            var result = ValidateSendGridSender(sender);

            if (!result.Successful)
                return result;

            sender.OrganizationId = org.Id;

            //NOTE: It would appear as of 6/15/2025, send grid does not allow you to change the Reply To Name, nor Nick Name via API. 
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);

                var jsonContent = JsonContent.Create(sender, sender.GetType());

                var json = JsonConvert.SerializeObject(sender);
                Console.WriteLine(json);

                var createResult = await client.PatchAsync($"https://api.sendgrid.com/v3/marketing/senders/{sender.Id}", jsonContent);
                var strResponse = await createResult.Content.ReadAsStringAsync();
                if (!createResult.IsSuccessStatusCode)
                {
                    _adminLogger.AddError("[SendGridEmailService__AddEmailSender]", strResponse, createResult.StatusCode.ToString().ToKVP("statusCode"));
                    return InvokeResult.FromError(strResponse);
                }

                return InvokeResult.Success;
            }
        }

        public async Task<InvokeResult> SendEmailSenderVerificationAsync(string senderId)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var response = await client.PostAsync($"https://api.sendgrid.com/v3/marketing/senders/{senderId}/resend_verification", null);
                var strResponse = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return InvokeResult.FromError(strResponse);
                }
                return InvokeResult.Success;
            }
        }

        public async Task<InvokeResult> DeleteEmailListAsync(string listId, EntityHeader org, EntityHeader user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);
                var url = $"https://api.sendgrid.com/v3/marketing/segments/{listId}";
                var response = await client.DeleteAsync(url);
                var strResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return ListResponse<EntityHeader>.FromError(strResponse);
                }
            }

            return InvokeResult.Success;
        }

    }
}