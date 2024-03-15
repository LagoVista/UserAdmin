using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.TeamsIntegration
{
    [TestClass]
    public class CalendarTests
    {
        public string AppId = Environment.GetEnvironmentVariable("GRAPH_APP_ID");
        public string DirectoryId = Environment.GetEnvironmentVariable("GRAPH_DIRECTORY_ID");
        public string Secret = Environment.GetEnvironmentVariable("GRAPH_SECRET_ID");
        public string SecretValue = Environment.GetEnvironmentVariable("GRAPH_SECRET_VALUE");

        ClientSecretCredential _creds;
        private static GraphServiceClient? _appClient;

        [TestInitialize]
        public void Init()
        {
            _creds = new ClientSecretCredential(DirectoryId, AppId, SecretValue);

            _appClient = new GraphServiceClient(_creds,
              // Use the default scope, which will request the scopes
              // configured on the app registration
              new[] { "https://graph.microsoft.com/.default" });
        }

        public async Task<string> GetAppOnlyTokenAsync()
        {
            // Request token with given scopes
            var context = new TokenRequestContext(new[] { "https://graph.microsoft.com/.default" });
            var response = await _creds.GetTokenAsync(context);
            return response.Token;
        }

        [TestMethod]
        public async Task TestIt()
        {
            var groups = await _appClient.Groups.GetAsync();

            var users = await _appClient.Users.GetAsync();

            var calendars = await _appClient.Organization.GetAsync();
      
            var calendarTab = await _appClient.Teams["7e2fee98-d460-43c7-8dda-49ac7684eb0c"]
                                .Channels["19:a8e733469c644aacacdc68d09eec1405@thread.skype"]
                                .Tabs["8bd7b46e-a22d-47da-839b-7d549799b255"].GetAsync();

          var groupCalendars =  await _appClient.Groups["7e2fee98-d460-43c7-8dda-49ac7684eb0c"].Calendar.GetAsync();


            foreach (var group in groups.Value)
            {
                Console.WriteLine(group.DisplayName);

               

                if (group.Id == "7e2fee98-d460-43c7-8dda-49ac7684eb0c")
                {
                    if (group.Calendar != null)
                    {
                        Console.WriteLine("\tCalendar Events: ");
                        foreach (var evt in group.Calendar.Events)
                        {
                            Console.WriteLine($"\t\t {evt.Subject} {evt.WebLink} {evt.Start}");
                        }
                    }
                }
            }
        }
    }
}
