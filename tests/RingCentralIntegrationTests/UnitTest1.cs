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
            bodyParams.timeSettings.timeRange.timeFrom = "2024-01-01T00:00:00.000Z";
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