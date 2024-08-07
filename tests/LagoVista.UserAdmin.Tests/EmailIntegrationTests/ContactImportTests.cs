﻿using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core.Interfaces;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.EmailIntegrationTests
{
    [TestClass]
    public class ContactImportTests
    {
        IEmailSender _emailSenderService;

        [TestInitialize]
        public void Init()
        {
            _emailSenderService = new SendGridEmailService(new IdentitySettings(), new Moq.Mock<IAppConfig>().Object, new Moq.Mock<IAdminLogger>().Object);
        }


        [TestMethod]
        public async Task GetSegments()
        {
            var lists = await _emailSenderService.GetListsAsync("x");
            Assert.IsTrue(lists.Successful);
            foreach(var list in lists.Model)
            {
                Console.WriteLine($"{list.Name},{list.Id},{list.Count},{list.LastUpdated}");
            }
        }

        [TestMethod]
        public async Task DeleteAllSegments()
        {
            var lists = await _emailSenderService.GetListsAsync("x");
            Assert.IsTrue(lists.Successful);
            foreach (var list in lists.Model)
            {
                Console.WriteLine($"{list.Name},{list.Id},{list.Count},{list.LastUpdated}");
                var result = await _emailSenderService.DeleteEmailListAsync("X", list.Id);
                Assert.IsTrue(result.Successful, result.ErrorMessage);
            }
        }

        [TestMethod]
        public async Task RefreshSegement()
        {
            var result = await _emailSenderService.RefreshSegementAsync("acc7890f-cfc1-45fa-87d5-8e91650d680c");
            Assert.IsTrue(true);
        }

        [TestMethod]
        public async Task CreateJobTestAsyncc()
        {
            using (var memoryStream = new MemoryStream())
            using (var writer = new StreamWriter(memoryStream))
            {
                // Various for loops etc as necessary that will ultimately do this:

                await writer.WriteLineAsync($"kevinw@software-logistics.com,Kevin,Wolf,testing,testing");
                await writer.WriteLineAsync($"kevindwolf@hotmail.com,Kevin,Wolf,testing,testing");
                await writer.WriteLineAsync($"kevinw@slsys.net,Kevin,Wolf,testing,testing");
                await writer.WriteLineAsync($"winmorocks@gmail.com,Kevin,Wolf,testing,testing");

                await writer.FlushAsync();

                memoryStream.Seek(0, SeekOrigin.Begin);

                var result = await _emailSenderService.StartImportJobAsync("email,first_name,last_name,industry_id,niche_id", memoryStream);
                Assert.IsTrue(result.Successful, result.ErrorMessage);
            }
        }
    }
}
