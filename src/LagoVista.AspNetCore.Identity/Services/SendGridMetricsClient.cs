using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Contacts;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    public class SendGridMetricsClient : ISendGridMetricsClient
    {
        private sealed class SendGridStats
        {
            [JsonProperty("requests")]
            public long? Requests { get; set; }

            [JsonProperty("delivered")]
            public long? Delivered { get; set; }

            [JsonProperty("clicks")]
            public long? Clicks { get; set; }

            [JsonProperty("opens")]
            public long? Opens { get; set; }

            [JsonProperty("bounces")]
            public long? Bounces { get; set; }

            [JsonProperty("unsubscribes")]
            public long? Unsubscribes { get; set; }
        }

        private sealed class SendGridStatsRow
        {
            [JsonProperty("aggregation")]
            public string Aggregation { get; set; }

            [JsonProperty("stats")]
            public SendGridStats Stats { get; set; }
        }

        private sealed class SendGridStatsResponse
        {
            [JsonProperty("results")]
            public List<SendGridStatsRow> Results { get; set; }
        }

        private readonly ILagoVistaAspNetCoreIdentityProviderSettings _settings;

        public SendGridMetricsClient(ILagoVistaAspNetCoreIdentityProviderSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public async Task<InvokeResult<IReadOnlyList<EmailListSendMetrics>>> GetSingleSendMetricsAsync(
            string singleSendId,
            DateTime startUtc,
            DateTime endUtc)
        {
            if (String.IsNullOrWhiteSpace(singleSendId))
                return InvokeResult<IReadOnlyList<EmailListSendMetrics>>.FromError("SendGrid Single Send id is required.");

            if (startUtc.Kind != DateTimeKind.Utc || endUtc.Kind != DateTimeKind.Utc || endUtc <= startUtc)
                return InvokeResult<IReadOnlyList<EmailListSendMetrics>>.FromError("SendGrid metrics require a valid UTC window.");

            if (startUtc.TimeOfDay != TimeSpan.Zero || endUtc.TimeOfDay != TimeSpan.Zero)
                return InvokeResult<IReadOnlyList<EmailListSendMetrics>>.FromError("SendGrid metrics require UTC day-aligned windows.");

            var inclusiveEnd = endUtc.AddDays(-1);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.SmtpServer.Password);

                var path =
                    $"https://api.sendgrid.com/v3/marketing/stats/singlesends/{Uri.EscapeDataString(singleSendId)}" +
                    $"?aggregated_by=day&timezone=UTC&start_date={startUtc:yyyy-MM-dd}&end_date={inclusiveEnd:yyyy-MM-dd}";

                var response = await client.GetAsync(path).ConfigureAwait(false);
                var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                    return InvokeResult<IReadOnlyList<EmailListSendMetrics>>.FromError(
                        $"SendGrid metrics request failed with HTTP {(int)response.StatusCode}.");

                var provider = JsonConvert.DeserializeObject<SendGridStatsResponse>(content);
                var rows = new List<EmailListSendMetrics>();

                foreach (var row in provider?.Results ?? new List<SendGridStatsRow>())
                {
                    if (!DateTime.TryParseExact(
                            row.Aggregation,
                            "yyyy-MM-dd",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out var timestamp))
                        continue;

                    rows.Add(new EmailListSendMetrics
                    {
                        TimeStampUtc = DateTime.SpecifyKind(timestamp.Date, DateTimeKind.Utc),
                        Requests = row.Stats?.Requests,
                        Delivered = row.Stats?.Delivered,
                        Clicks = row.Stats?.Clicks,
                        Opens = row.Stats?.Opens,
                        Bounces = row.Stats?.Bounces,
                        Unsubscribes = row.Stats?.Unsubscribes
                    });
                }

                return InvokeResult<IReadOnlyList<EmailListSendMetrics>>.Create(rows);
            }
        }
    }
}
