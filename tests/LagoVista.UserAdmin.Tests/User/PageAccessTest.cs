using LagoVista.UserAdmin.Models.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.User
{
    [TestClass]
    public class PageAccessTest : SecurityBase
    {
        [TestMethod]
        public async Task Should_Have_No_Page_Access()
        {
            var nr1 = _mod1.Areas[0];
            nr1.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            Assert.IsNotNull(module.Areas.SingleOrDefault(ar => ar.Id == nr1.Id));
            Assert.AreEqual(0, module.Areas.SingleOrDefault(ar => ar.Id == nr1.Id).Pages.Count);
        }

        [TestMethod]
        public async Task Should_Have_Module_Level_Access_When_Page_Not_Restricted()
        {
            var pg1 = _mod1.Areas[0].Pages[0];
            pg1.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Have_Area_Level_Access_When_Page_Not_Restricted()
        {
            var pg1 = _mod1.Areas[0].Pages[0];
            pg1.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.Areas[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Get_Non_Restricted_Page()
        {
            var pg1 = _mod1.Areas[0].Pages[2];
            var pg2 = _mod1.Areas[3].Pages[1];
            var pg3 = _mod1.Areas[4].Pages[0];

            pg1.RestrictByDefault = false;
            pg2.RestrictByDefault = false;
            pg3.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);

            ValidateUserAccess(module.Areas.Single(ar => ar.Id == _mod1.Areas[0].Id).UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas.Single(ar => ar.Id == _mod1.Areas[3].Id).UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas.Single(ar => ar.Id == _mod1.Areas[4].Id).UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);

            Assert.IsFalse(module.Areas.Any(ar => ar.Id == _mod1.Areas[1].Id));
            Assert.IsFalse(module.Areas.Any(ar => ar.Id == _mod1.Areas[2].Id));

            Assert.AreEqual(3, module.Areas.Count);
        }

        [TestMethod]
        public async Task Should_Add_Page_To_All_Restricted_ReadOnly()
        {
            var pg1 = _mod1.Areas[0].Pages[2];
            AddPageAccess(_role1, _mod1, _mod1.Areas[0], pg1, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke, UserAccess.Revoke);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);

            Assert.AreEqual(1, module.Areas.Count);
            Assert.AreEqual(1, module.Areas[0].Pages.Count);
            Assert.AreEqual(pg1.Id, module.Areas[0].Pages[0].Id);

            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas[0].Pages[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Add_Pages_To_All_Restricted_ReadOnly()
        {
            var pg1 = _mod1.Areas[0].Pages[2];
            AddPageAccess(_role1, _mod1, _mod1.Areas[0], pg1, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke, UserAccess.Revoke);

            var pg2 = _mod1.Areas[1].Pages[2];
            AddPageAccess(_role1, _mod1, _mod1.Areas[1], pg2, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke, UserAccess.Revoke);

            var pg3 = _mod1.Areas[4].Pages[0];
            AddPageAccess(_role1, _mod1, _mod1.Areas[4], pg3, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke, UserAccess.Revoke);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);

            Assert.AreEqual(3, module.Areas.Count);
            Assert.AreEqual(1, module.Areas[0].Pages.Count);
            Assert.AreEqual(1, module.Areas[1].Pages.Count);
            Assert.AreEqual(1, module.Areas[2].Pages.Count);
           
            Assert.AreEqual(pg1.Id, module.Areas[0].Pages[0].Id);
            Assert.AreEqual(pg2.Id, module.Areas[1].Pages[0].Id);
            Assert.AreEqual(pg3.Id, module.Areas[2].Pages[0].Id);

            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas[1].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas[2].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            
            ValidateUserAccess(module.Areas[0].Pages[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas[1].Pages[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas[2].Pages[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Add_Page_To_All_Restricted_AllAccess()
        {
            var pg1 = _mod1.Areas[0].Pages[2];
            AddPageAccess(_role1, _mod1, _mod1.Areas[0], pg1, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);

            Assert.AreEqual(1, module.Areas.Count);
            Assert.AreEqual(1, module.Areas[0].Pages.Count);
            Assert.AreEqual(pg1.Id, module.Areas[0].Pages[0].Id);

            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(module.Areas[0].Pages[0].UserAccess, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant);
        }
    }
}