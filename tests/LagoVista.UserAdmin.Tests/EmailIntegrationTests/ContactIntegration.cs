using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.EmailIntegrationTests
{
    [TestClass]
    public class ContactIntegration
    {
        IEmailSender _emailSenderService;
        Mock<IOrganizationManager> _orgManager = new Mock<IOrganizationManager>();

        EntityHeader _org = EntityHeader.Create("AA2C78499D0140A5A9CE4B7581EF9691", "softwarelogistics", "Software Logistics");
        EntityHeader _user = EntityHeader.Create("CC648B3B51164A8296EB7092F312D5CB", "Kevin Wolf");

        [TestInitialize]
        public void Init()
        {
            _emailSenderService = new SendGridEmailService(new IdentitySettings(), new Mock<IOrganizationRepo>().Object, new Mock<IAppUserRepo>().Object, new Moq.Mock<IBackgroundServiceTaskQueue>().Object, new Moq.Mock<IAppConfig>().Object, new Moq.Mock<IAdminLogger>().Object);
        }

        
        [TestMethod]
        public async Task AddContact()
        {
            var contact = new Contact()
            {
                FirstName = "Kevin",
                LastName = "Wolf",
                 Email = "kevindwolf@hotmail.com",
                 Phone = "727 415 1773",
            };

            var company = new Company()
            {
                Name = "SL",
                Id = "DEF456",
                Industry = EntityHeader.Create("tech", "Technology"),
                OwnerOrganization = EntityHeader.Create("ABC","SL"),
                City = "Tampa",
                State = "FL"
            };

            var result = await _emailSenderService.RegisterContactAsync(contact, company, _org, _user);

            Console.WriteLine(result.Result);

            if (!result.Successful)
                throw new Exception(result.Result);
        }

        [TestMethod]
        public async Task AddList()
        {
            var customField = "industry_id";
            var id = "abc123";

            var result = await _emailSenderService.CreateEmailListAsync("Test Industry", customField, id, _org, _user);

            Console.WriteLine(result.Result);

            if (!result.Successful)
                throw new Exception(result.Result);

        }


        [TestMethod]
        public async Task AddContactToList()
        {
            var result = await _emailSenderService.AddContactToListAsync("da786a80-5f53-4077-9ee6-fdd415146e24", "eee130d5-dbb7-4f3a-9943-88ec38641bc8", _org, _user);
        }

        [TestMethod]
        public async Task SendEmail()
        {
            var result = await _emailSenderService.SendAsync(new Email()
            {
                To = new System.Collections.Generic.List<EmailAddress>() { new EmailAddress() { Address = "kevinw@slsys.net", Name = "Kevin" } },
                Subject = "Test Message",
                From = new EmailAddress() {  Address = "alerts@nuviot.com", Name= "ALerts"},
                Content = "Hi Kevin",
            }, _org, _user);


            Assert.IsTrue(result.Successful);
            Console.WriteLine(result.Result); 
        }

    }
}
