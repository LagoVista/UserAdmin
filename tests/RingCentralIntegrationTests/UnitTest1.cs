using LagoVista.Core;
using Newtonsoft.Json;
using NUnit.Framework;
using RingCentral;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace RingCentralIntegrationTests
{
    public class Tests
    {
        RestClient _rc;

        [SetUp]
        public async Task Setup()
        {
            _rc = new RestClient(Environment.GetEnvironmentVariable("RC_CLIENT_ID"), Environment.GetEnvironmentVariable("RC_CLIENT_SECRET"), "https://platform.devtest.ringcentral.com");
            await _rc.Authorize("RC_CLIENT_JWT");
        }

        [Test]
        public async Task Analysitcs()
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
    }
}