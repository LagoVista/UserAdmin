// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fe1d5ff828c20e887dfcbb6e05ab421326969b0f6c32184e4ed2ac58da1a281b
// IndexVersion: 0
// --- END CODE INDEX META ---
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
    public class FeatureAccessTests : SecurityBase
    {
        [TestMethod]
        public async Task Should_Have_No_Feature_Access_With_Non_Restricted_Page()
        {
            Assert.AreNotEqual(0, _mod1.Areas[0].Pages[0].Features.Count);

            var pg1 = _mod1.Areas[0].Pages[0];
            pg1.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            Assert.AreEqual(0, module.Areas[0].Pages[0].Features.Count);
        }

        [TestMethod]
        public async Task Should_Have_Module_Access_With_NonRestricted_Feature()
        {
            var ft1 = _mod1.Areas[0].Pages[0].Features[0];
            ft1.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Have_Area_Access_With_NonRestricted_Feature()
        {
            var ft1 = _mod1.Areas[0].Pages[0].Features[0];
            ft1.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.Areas[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Have_Area_Page_With_NonRestricted_Feature()
        {
            var ft1 = _mod1.Areas[0].Pages[0].Features[0];
            ft1.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.Areas[0].Pages[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Have_Feature_With_NonRestricted_Feature()
        {
            var ft1 = _mod1.Areas[0].Pages[0].Features[0];
            ft1.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.Areas[0].Pages[0].Features[0].UserAccess, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant);
        }

        [TestMethod]
        public async Task Should_Have_Module_Access_With_Added_Feature()
        {
            AddFeatureAccess(_role1, _mod1, _mod1.Areas[0], _mod1.Areas[0].Pages[0], _mod1.Areas[0].Pages[0].Features[0], UserAccess.Grant);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Have_Area_Access_With_Added_Feature()
        {
            AddFeatureAccess(_role1, _mod1, _mod1.Areas[0], _mod1.Areas[0].Pages[0], _mod1.Areas[0].Pages[0].Features[0], UserAccess.Grant);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.Areas[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Have_Area_Page_With_Added_Feature()
        {
            AddFeatureAccess(_role1, _mod1, _mod1.Areas[0], _mod1.Areas[0].Pages[0], _mod1.Areas[0].Pages[0].Features[0], UserAccess.Grant);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.Areas[0].Pages[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Have_Feature_With_Added_Feature()
        {
            AddFeatureAccess(_role1, _mod1, _mod1.Areas[0], _mod1.Areas[0].Pages[0], _mod1.Areas[0].Pages[0].Features[0], UserAccess.Grant);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.Areas[0].Pages[0].Features[0].UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }


        [TestMethod]
        public async Task Should_Have_Feature_With_Added_Feature_All_Access()
        {
            AddFeatureAccess(_role1, _mod1, _mod1.Areas[0], _mod1.Areas[0].Pages[0], _mod1.Areas[0].Pages[0].Features[0], UserAccess.Grant, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.Areas[0].Pages[0].Features[0].UserAccess, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant);
        }
    }
}
