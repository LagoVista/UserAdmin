using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.EmailIntegrationTests
{
    [TestClass]
    public class ContactIntegration
    {
        IEmailSender _emailSenderService;

        [TestInitialize]
        public void Init()
        {
            _emailSenderService = new SendGridEmailService(new IdentitySettings(), new Moq.Mock<IAppConfig>().Object, new Moq.Mock<IAdminLogger>().Object);
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

            var result = await _emailSenderService.RegisterContactAsync(contact, company);

            Console.WriteLine(result.Result);

            if (!result.Successful)
                throw new Exception(result.Result);
        }

        [TestMethod]
        public async Task AddList()
        {
            var result = await _emailSenderService.CreateEmailListAsync("Test List2");

            Console.WriteLine(result.Result);

            if (!result.Successful)
                throw new Exception(result.Result);

        }

        [TestMethod]
        public async Task AddContactToList()
        {
            var result = await _emailSenderService.AddContactToListAsync("da786a80-5f53-4077-9ee6-fdd415146e24", "eee130d5-dbb7-4f3a-9943-88ec38641bc8");
        }
    }
}
