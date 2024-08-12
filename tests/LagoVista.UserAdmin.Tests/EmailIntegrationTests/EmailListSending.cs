using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using Microsoft.Graph.Models;
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
    public class EmailListSending
    {
        IEmailSender _emailSenderService;
        Mock<IOrganizationManager> _orgManager = new Mock<IOrganizationManager>();

        EntityHeader _org = EntityHeader.Create("THISISNOTTHECATEGORYFORTHISORG", "softwarelogistics", "Software Logistics");
        EntityHeader _user = EntityHeader.Create("CC648B3B51164A8296EB7092F312D5CB", "Kevin Wolf");

        [TestInitialize]
        public void Init()
        {
            _emailSenderService = new SendGridEmailService(new IdentitySettings(), new Moq.Mock<IAppConfig>().Object, new Moq.Mock<IAdminLogger>().Object);
        }


        [TestMethod]
        public async Task CreateAndSendToListAsync()
        {
            var segmentId = "7fe9927f-95f8-4f6e-9d31-7fb5225b35ad";
            var senderId = "6164844";
            var designId = "d498a794-a235-4a3f-aba8-be3f74db26cb";

            var result = await _emailSenderService.SendToListAsync($"List Created At {DateTime.UtcNow}", segmentId, senderId, designId, _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);

            var ssId = result.Result;
            Console.WriteLine($"ssid list id: {ssId}");

            var deleteResult = await _emailSenderService.DeleteEmailListSendAsync(ssId, _org, _user);
            Assert.IsTrue(deleteResult.Successful, deleteResult.ErrorMessage);
        }

        [TestMethod]
        public async Task CreateAndGetListsForOrg()
        {
            var segmentId = "7fe9927f-95f8-4f6e-9d31-7fb5225b35ad";
            var senderId = "6164844";
            var designId = "d498a794-a235-4a3f-aba8-be3f74db26cb";

            var result = await _emailSenderService.SendToListAsync($"1) List Created At {DateTime.UtcNow}", segmentId, senderId, designId, _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);
            Console.WriteLine($"Created List 1 - {result.Result}");

            result = await _emailSenderService.SendToListAsync($"2) List Created At {DateTime.UtcNow}", segmentId, senderId, designId, _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);
            Console.WriteLine($"Created List 2 - {result.Result}");

            result = await _emailSenderService.SendToListAsync($"3) List Created At {DateTime.UtcNow}", segmentId, senderId, designId, _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);
            Console.WriteLine($"Created List 3 - {result.Result}");

            var otherOrg = EntityHeader.Create("abc1234", "Testing");

            var lastCreated = await _emailSenderService.SendToListAsync($"3) List Created At {DateTime.UtcNow}", segmentId, senderId, designId, otherOrg, _user);
            Assert.IsTrue(lastCreated.Successful, lastCreated.ErrorMessage);
            Console.WriteLine($"Created List For Other Org - {lastCreated.Result}");

            var lists = await _emailSenderService.GetEmailListSendsAsync(ListRequest.Create(), _org, _user);
            Assert.AreEqual(3, lists.Model.Count());

            foreach (var list in lists.Model)
            {
                Console.WriteLine(list.Name);
                var deleteResult = await _emailSenderService.DeleteEmailListSendAsync(list.Id, _org, _user);
                Assert.IsTrue(deleteResult.Successful);
            }

            var deleteResultOther = await _emailSenderService.DeleteEmailListSendAsync(lastCreated.Result, otherOrg, _user);
            Assert.IsTrue(deleteResultOther.Successful);
        }

        [TestMethod]
        public async Task CreateAndSendList()
        {
            var segmentId = "7fe9927f-95f8-4f6e-9d31-7fb5225b35ad";
            var senderId = "6164844";
            var designId = "956cf695-ef88-43ed-9ffc-26364e5a3b47";

            var result = await _emailSenderService.SendToListAsync($"1) List Created At {DateTime.UtcNow}", segmentId, senderId, designId, _org, _user);
            Assert.IsTrue(result.Successful, result.ErrorMessage);
            Console.WriteLine($"Created List 1 - {result.Result}");

            var sendResult = await _emailSenderService.SendEmailSendListNowAsync(result.Result, _org, _user);
            Assert.IsTrue(sendResult.Successful, sendResult.ErrorMessage);
        }
    }
}
