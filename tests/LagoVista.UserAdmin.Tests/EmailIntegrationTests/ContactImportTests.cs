using LagoVista.AspNetCore.Identity.Services;
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
