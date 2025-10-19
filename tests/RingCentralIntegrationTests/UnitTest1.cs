// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a81421ba58d3d0adfb65d6383bc44fdf930af37aa6be1a2e9c192a657093f2d8
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using RingCentral;
using System;
using System.Diagnostics;
using System.Linq;
using LagoVista.UserAdmin.Managers;
using System.Threading.Tasks;
using LagoVista.UserAdmin;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;

namespace RingCentralIntegrationTests
{
    public class Tests
    {
        RestClient _rc;

        [SetUp]
        public async Task Setup()
        {
            var clientId = Environment.GetEnvironmentVariable("RC_CLIENT_ID");
            var secret = Environment.GetEnvironmentVariable("RC_CLIENT_SECRET");
            var url = Environment.GetEnvironmentVariable("RC_CLIENT_URL");

          //  var clientId = Environment.GetEnvironmentVariable("RC_CLIENT_ID_WH");
          //  var secret = Environment.GetEnvironmentVariable("RC_CLIENT_SECRET_WH");

            var jwt = Environment.GetEnvironmentVariable("RC_CLIENT_JWT");

            _rc = new RestClient(clientId, secret, url);
            await _rc.Authorize(jwt);
        }

        [Test]
        public async Task Analytics()
        {

            var bodyParams = new AggregationRequest();
            bodyParams.grouping = new Grouping();
            bodyParams.grouping.groupBy = "Users";
            bodyParams.timeSettings = new TimeSettings();
            bodyParams.timeSettings.timeZone = "America/Los_Angeles";
            bodyParams.timeSettings.timeRange = new TimeRange();
            // Change the "timeFrom" value accordingly so that it does not exceed 184 days from the current date and time
            // The specified time is UTC time. If you want the timeFrom and timeTo your local time, you have to convert
            // your local time to UTC time!
            bodyParams.timeSettings.timeRange.timeFrom = "2025-05-01T00:00:00.000Z";
            bodyParams.timeSettings.timeRange.timeTo = DateTime.UtcNow.ToJSONString();
            bodyParams.responseOptions = new AggregationResponseOptions();
            bodyParams.responseOptions.counters = new AggregationResponseOptionsCounters();
            bodyParams.responseOptions.counters.allCalls = new AggregationResponseOptionsCountersAllCalls();
            bodyParams.responseOptions.counters.allCalls.aggregationType = "Sum";

            var queryParams = new AnalyticsCallsAggregationFetchParameters();
            queryParams.perPage = 100;

            var response = await _rc.Analytics().Calls().V1().Accounts("~").Aggregation().Fetch().Post(bodyParams, queryParams);

            Console.WriteLine(JsonConvert.SerializeObject(response, Formatting.Indented));


        }

        [Test]
        public async Task Test1()
        {
            var callLogResponse = await _rc.Restapi().Account().CallLog().List(new ReadCompanyCallLogParameters
            {
                perPage = 12,
                dateFrom = DateTime.UtcNow.AddMonths(-6).ToString("o"),
                phoneNumber = "7274550530"
            });

            Console.WriteLine(JsonConvert.SerializeObject(callLogResponse, Formatting.Indented));


            Debugger.Break();

        }


        [Test]
        public async Task GetRecords()
        {
            var cm = new RingCentralManager(new Creds());
            var contacts = await cm.GetPhoneContactsAsync("7274550530", ListRequest.Create(), EntityHeader.Create("id", "name"), EntityHeader.Create("id", "name"));
            foreach(var contact in contacts.Model)
            {
                Console.WriteLine(contact);
            }
        }

        [Test]
        public async Task Subscribe()
        {
            var bodyParams = new CreateSubscriptionRequest();
            bodyParams.eventFilters = new[] { "/restapi/v1.0/account/~/telephony/sessions" };
            bodyParams.deliveryMode = new NotificationDeliveryModeRequest
            {
                transportType = "WebHook",
                address = "https://www.nuviot.com/webhooks/AA2C78499D0140A5A9CE4B7581EF9691/ringcentral"
            };
            bodyParams.expiresIn = 60*60*24*365;

            var resp = await _rc.Restapi().Subscription().Post(bodyParams);
            Console.WriteLine("Subscription Id: " + resp.id);
            Console.WriteLine("Ready to receive incoming SMS via WebHook.");
        }
    }

    public class Creds : IRingCentralCredentials
    {
        public Creds()
        {
            RingCentralClientId = Environment.GetEnvironmentVariable("RC_CLIENT_ID");
            RingCentralClientSecret = Environment.GetEnvironmentVariable("RC_CLIENT_SECRET");
            RingCentralUrl = Environment.GetEnvironmentVariable("RC_CLIENT_URL");
            RingCentralJWT = Environment.GetEnvironmentVariable("RC_CLIENT_JWT");
        }

        public string RingCentralClientId { get; }

        public string RingCentralClientSecret { get; }

        public string RingCentralJWT { get; }

        public string RingCentralUrl { get; }
    }
}