using LagoVista.UserAdmin.Models.Contacts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace LagoVista.UserAdmin.Tests.EmailIntegrationTests
{
    [TestClass]
    public class EmailSenderContractTests
    {
        [TestMethod]
        public void SendGridJsonDeserializesVerifiedTrue()
        {
            var sender = JsonConvert.DeserializeObject<EmailSender>(CreateSenderJson(true));

            Assert.IsNotNull(sender);
            Assert.IsTrue(sender.Verified);
        }

        [TestMethod]
        public void SendGridJsonDeserializesVerifiedFalse()
        {
            var sender = JsonConvert.DeserializeObject<EmailSender>(CreateSenderJson(false));

            Assert.IsNotNull(sender);
            Assert.IsFalse(sender.Verified);
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void CreateSummaryPreservesVerifiedAndExistingFields(bool verified)
        {
            var sender = new EmailSender
            {
                Id = 42,
                NickName = "Transactional Sender",
                From = new EmailSenderAddress
                {
                    Email = "sender@example.com",
                    Name = "Sender Name"
                },
                Verified = verified
            };

            var summary = sender.CreateSummary();

            Assert.AreEqual("42", summary.Id);
            Assert.AreEqual("Transactional Sender", summary.Name);
            Assert.AreEqual("sender@example.com", summary.Key);
            Assert.AreEqual("icon-pz-programmer", summary.Icon);
            Assert.AreEqual(verified, summary.Verified);
        }

        private static string CreateSenderJson(bool verified)
        {
            return $@"{{
  ""id"": 42,
  ""nickname"": ""Transactional Sender"",
  ""from"": {{ ""email"": ""sender@example.com"", ""name"": ""Sender Name"" }},
  ""reply_to"": {{ ""email"": ""reply@example.com"", ""name"": ""Reply Name"" }},
  ""address"": ""123 Main St"",
  ""address_2"": ""org-1"",
  ""city"": ""Palm Harbor"",
  ""state"": ""FL"",
  ""zip"": ""34685"",
  ""country"": ""United States"",
  ""verified"": {verified.ToString().ToLowerInvariant()},
  ""updated_at"": 1449872165,
  ""created_at"": 1449872165,
  ""locked"": false
}}";
        }
    }
}
