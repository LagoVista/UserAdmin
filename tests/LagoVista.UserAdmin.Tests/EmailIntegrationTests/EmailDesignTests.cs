// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5277f92db22946e965d5787d7aba413fa740ecbea25067459717b2b1698ba5b8
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.EmailIntegrationTests
{
    [TestClass]
    public class EmailDesignTests
    {
        IEmailSender _emailSenderService;

        public const string OrgId = "DONTCARE";
        EntityHeader _org = EntityHeader.Create("AA2C78499D0140A5A9CE4B7581EF9691", "softwarelogistics", "Software Logistics");
        EntityHeader _user = EntityHeader.Create("CC648B3B51164A8296EB7092F312D5CB", "Kevin Wolf");

        private Mock<IOrganizationManager> _orgManager = new Mock<IOrganizationManager>();


        [TestInitialize]
        public void Init()
        {
            _emailSenderService = new SendGridEmailService(new IdentitySettings(), new Mock<IOrganizationRepo>().Object, new Mock<IAppUserRepo>().Object, new Moq.Mock<IBackgroundServiceTaskQueue>().Object, new Moq.Mock<IAppConfig>().Object, new Moq.Mock<IAdminLogger>().Object);
        }

        [TestMethod]
        public async Task CreateDesignAsync()
        {
            var result = await _emailSenderService.AddEmailDesignAsync($"TESTING - {DateTime.Now}", "SEND THE EMAIL", "HTML CONTENTS", "PLAIN TEXT CONTENTS", _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);
            var deleteResult = await _emailSenderService.DeleteEmailDesignAsync(result.Result, _org, _user);
            Assert.IsTrue(deleteResult.Successful);
        }

        [TestMethod]
        public async Task GetAllDesignsAsync()
        {
            var result = await _emailSenderService.GetEmailDesignsAsync( _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);
            foreach(var design in result.Model)
            {
                Console.WriteLine($"{design.Id} - {design.Name} {design.ThumbmailImage}");
            }
        }


        [TestMethod]
        public async Task UpdateDesignAsync()
        {
            var result = await _emailSenderService.AddEmailDesignAsync($"TESTING - {DateTime.Now}", "SEND THE EMAIL", "HTML CONTENTS", "PLAIN TEXT CONTENTS", _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);

            Console.WriteLine(result.Result);

            var updateResult = await _emailSenderService.UpdateEmailDesignAsync(result.Result, "UPDATED TESTING", "UPDATE SUBJECT", "UPDATE HTML CONTENT", "UPDATE TEXT CONTENTS", _org, _user);
            Assert.IsTrue(updateResult.Successful, updateResult.ErrorMessage);

            var deleteResult = await _emailSenderService.DeleteEmailDesignAsync(result.Result, _org, _user);
            Assert.IsTrue(deleteResult.Successful);
        }
    }
}
