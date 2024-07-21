using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.IoT.Logging.Utils;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Graph.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System;
using ZstdSharp.Unsafe;

namespace LagoVista.UserAdmin.Tests.Distribution
{

    [TestClass]
    public class UniqueDistributionTests
    {

        IDistributionManager _distorMgr;
        Mock<IDistributionListRepo> _listRepo;
        Mock<IAppUserRepo> _appuserRepo;

        private int _index = 100;

        [TestInitialize]
        public void Init()
        {
            _listRepo = new Mock<IDistributionListRepo>();
            _appuserRepo = new Mock<IAppUserRepo>();

            _distorMgr = new DistributionManager(new Mock<IEmailSender>().Object, new Mock<ISmsSender>().Object, _listRepo.Object, new Mock<ILinkShortener>().Object, _appuserRepo.Object, new AdminLogger(new ConsoleLogWriter()),
                new Mock<IAppConfig>().Object, new Mock<IDependencyManager>().Object, new Mock<ISecurity>().Object);
        }


        private AppUserContact AddUser(string userId, string firstName, string lastName, string email, string phoneNumber)
        {
            var appUser = new Models.Users.AppUser()
            {
                Id = userId,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phoneNumber
            };

            _appuserRepo.Setup(mdt => mdt.FindByIdAsync(It.Is<string>(id => id == userId))).ReturnsAsync(appUser);


            return new AppUserContact() { Id = appUser.Id, Text = appUser.Name };
        }

        private void AddDistributionList(string id, string name, System.Collections.Generic.List<AppUserContact> users, System.Collections.Generic.List<ExternalContact> contacts)
        {
            var dl = new Models.Orgs.DistroList()
            {
                Id = id,
                Name = name,
                ExternalContacts = contacts,
                AppUsers = users
            };

            _listRepo.Setup(dl => dl.GetDistroListAsync(It.Is<string>(lst => lst == id))).ReturnsAsync(dl);
        }

        private AppUserContact GenerateAppuser()
        {
            _index++;

            var appUser = new AppUser()
            {
                Id = $"USERID{_index}",
                FirstName = $"FN_{_index}",
                LastName = $"FN_{_index}",
                PhoneNumber = $"6125551{_index}",
                Email = $"email{_index}@company.com"
            };


            _appuserRepo.Setup(mdt => mdt.FindByIdAsync(It.Is<string>(id => id == appUser.Id))).ReturnsAsync(appUser);

            return new AppUserContact() { Id = appUser.Id, Text = appUser.Name };
        }

        private ExternalContact GenerateExternalContact()
        {
            _index++;

            return new ExternalContact()
            {
                SendEmail = true,
                SendSMS = true,
                FirstName = $"FN_{_index}",
                LastName = $"FN_{_index}",
                Phone = $"6125551{_index}",
                Email = $"email{_index}@company.com"
            };
        }

        [TestMethod]
        public async Task Should_Load_Distro_List()
        {
            var au1 = GenerateAppuser();
            var au2 = GenerateAppuser();
            var au3 = GenerateAppuser();
            var au4 = GenerateAppuser();

            var users = new System.Collections.Generic.List<AppUserContact>() { au1, au2, au3, au4 };

            var ec1 = GenerateExternalContact();
            var ec2 = GenerateExternalContact();
            var ec3 = GenerateExternalContact();
            var ec4 = GenerateExternalContact();
            var ec5 = GenerateExternalContact();

            var contacts = new System.Collections.Generic.List<ExternalContact>() { ec1, ec2, ec3, ec4, ec5 };

            AddDistributionList("dist1", "Distro List 1", users, contacts);

            var allContacts = await _distorMgr.GetAllContactsAsync("dist1");
            Assert.AreEqual(9, allContacts.Count);
            foreach(var contact in allContacts)
            {
                Console.WriteLine($"{contact.Name}, {contact.Email}, {contact.Phone}");
            }
        }

        [TestMethod]
        public async Task Should_Ignore_Dup_Emails_Across_Lists()
        {
            var au1 = GenerateAppuser();
            var au2 = GenerateAppuser();
            var au3 = GenerateAppuser();
            var au4 = GenerateAppuser();
            var au5 = GenerateAppuser();
            var au6 = GenerateAppuser();

            var userListOne = new System.Collections.Generic.List<AppUserContact>() { au1, au2, au3, au4 };
            var userListTwo = new System.Collections.Generic.List<AppUserContact>() { au3, au4, au5, au6 };

            var ec1 = GenerateExternalContact();
            var ec2 = GenerateExternalContact();
            var ec3 = GenerateExternalContact();
            var ec4 = GenerateExternalContact();
            var ec5 = GenerateExternalContact();

            var contacts = new System.Collections.Generic.List<ExternalContact>() { ec1, ec2, ec3, ec4, ec5 };

            AddDistributionList("dist1", "Distro List 1", userListOne, contacts);
            AddDistributionList("dist2", "Distro List 1", userListTwo, new System.Collections.Generic.List<ExternalContact>());

            var allContacts = await _distorMgr.GetAllContactsAsync("dist1");
            allContacts = await _distorMgr.GetAllContactsAsync("dist2", allContacts);
            Assert.AreEqual(11, allContacts.Count);
            foreach (var contact in allContacts)
            {
                Console.WriteLine($"{contact.Name}, {contact.Email}, {contact.Phone}");
            }
        }

    }
}
