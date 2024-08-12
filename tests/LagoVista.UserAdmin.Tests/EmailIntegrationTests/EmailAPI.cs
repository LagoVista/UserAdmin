using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.EmailIntegrationTests
{
    [TestClass]
    public class EmailAPI
    {
        IEmailSender _emailSenderService;

        public const string OrgId = "DONTCARE";
        private EntityHeader _org;
        private EntityHeader _user;

        private Mock<IOrganizationManager> _orgManager;


        [TestInitialize]
        public void Init()
        {
            _emailSenderService = new SendGridEmailService(new IdentitySettings(), _orgManager.Object, new Moq.Mock<IAppConfig>().Object,  new Moq.Mock<IAdminLogger>().Object);
        }

        [TestMethod]
        public async Task CreateDesignAsync()
        {
            var result = await _emailSenderService.AddEmailDesignAsync($"TESTING - {DateTime.Now}", "SEND THE EMAIL", "HTML CONTENTS", "PLAIN TEXT CONTENTS", _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);
            Assert.IsTrue(result.Successful);
            var deleteResult = await _emailSenderService.DeleteEmailDesignAsync(result.Result, _org, _user);
            Assert.IsTrue(deleteResult.Successful);
        }

        [TestMethod]
        public async Task GetAllSendersAsync()
        {
            var senders = await _emailSenderService.GetEmailSendersAsync(_org, _user);
            Assert.IsTrue(senders.Successful);

            foreach(var sender in senders.Model)
            {
                Console.WriteLine($"{sender.Id} - {sender.Text}");
            }
        }

        [TestMethod]
        public async Task UpdateDesignAsync()
        {
            var result = await _emailSenderService.AddEmailDesignAsync($"TESTING - {DateTime.Now}", "SEND THE EMAIL", "HTML CONTENTS", "PLAIN TEXT CONTENTS", _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);

            Console.WriteLine(result.Result);

            var updateResult = await _emailSenderService.UpdateEmailDesignAsync(result.Result, "UPDATED TESTING", "UPDATE SUBJECT", "UPDATE HTML CONTENT", "UPDATE TEXT CONTENTS",_org, _user);
            Assert.IsTrue(updateResult.Successful, updateResult.ErrorMessage);

            var deleteResult = await _emailSenderService.DeleteEmailDesignAsync(result.Result, _org, _user);
            Assert.IsTrue(deleteResult.Successful);
        }

        [TestMethod]
        public async Task AddSenderAsync()
        {
            var appUser = new AppUser();
            appUser.FirstName = "Kevin";
            appUser.LastName = "Wolf";
            appUser.Email = "kevinw@software-logistics.com";
            appUser.Address1 = "1847 Lago Vista Blvd";
            appUser.City = "Palm Harbor";
            appUser.State = "FL";
            appUser.PostalCode = "34685";
            appUser.Country = "United States";

            var result = await _emailSenderService.AddEmailSenderAsync(appUser, _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);
            Assert.IsTrue(result.Result.SendGridVerified, "Send Grid User should be Verified (email match domain name)");
            Assert.IsTrue(!String.IsNullOrEmpty(result.Result.SendGridSenderId));

            var deleteResult = await _emailSenderService.DeleteEmailSenderAsync(appUser.SendGridSenderId, _org, _user);
            Assert.IsTrue(deleteResult.Successful, deleteResult.ErrorMessage);
        }

        [TestMethod]
        public async Task UpdateSenderAsync()
        {
            var appUser = new AppUser();
            appUser.FirstName = "Kevin";
            appUser.LastName = "Wolf";
            appUser.Email = "kevinw@software-logistics.com";
            appUser.Address1 = "1847 Lago Vista Blvd";
            appUser.City = "Palm Harbor";
            appUser.State = "FL";
            appUser.PostalCode = "34685";
            appUser.Country = "United States";

            var result = await _emailSenderService.AddEmailSenderAsync(appUser, _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);
            Assert.IsTrue(result.Result.SendGridVerified, "Send Grid User should be Verified (email match domain name)");
            Assert.IsTrue(!String.IsNullOrEmpty(result.Result.SendGridSenderId));

            appUser.Email = "foo@mycompany.com";

            result = await _emailSenderService.UpdateEmailSenderAsync(appUser, _org, _user);

            Console.WriteLine(result.Result.SendGridVerifiedFailedReason);

            Assert.IsTrue(result.Successful, result.ErrorMessage);
            Assert.IsFalse(result.Result.SendGridVerified, "Send Grid User should not be Verified (we changed email to not match domain name)");
            Assert.IsTrue(!String.IsNullOrEmpty(result.Result.SendGridSenderId));


            var deleteResult = await _emailSenderService.DeleteEmailSenderAsync(appUser.SendGridSenderId, _org, _user);
            Assert.IsTrue(deleteResult.Successful, deleteResult.ErrorMessage);
        }

    }
}
