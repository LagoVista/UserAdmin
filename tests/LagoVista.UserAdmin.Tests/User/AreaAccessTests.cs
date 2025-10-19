// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d4be84d4bcbbec1b8afba692ef47fcee9d4d77758f710afd31d0c3a595ee6ccb
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Models.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.User
{
    [TestClass]
    public class AreaAccessTests : SecurityBase
    {
        [TestMethod]
        public async Task Should_Have_No_Area_Access()
        {
            _mod1.RestrictByDefault = false;
            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            Assert.AreEqual(0, module.Areas.Count);
        }

        [TestMethod]
        public async Task Should_Have_Non_Restricted_Areas()
        {
            _mod1.RestrictByDefault = false;

            var nr1 = _mod1.Areas[0];
            nr1.RestrictByDefault = false;

            var nr2 = _mod1.Areas[3];
            nr2.RestrictByDefault = false;

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            Assert.IsNotNull(module.Areas.SingleOrDefault(ar => ar.Id == nr1.Id));
            Assert.IsNotNull(module.Areas.SingleOrDefault(ar => ar.Id == nr2.Id));

            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);

            Assert.AreEqual(2, module.Areas.Count);
        }

        [TestMethod]
        public async Task Should_Have_Added_Restricted_Areas_With_NonRestrictedAreas()
        {
            _mod1.RestrictByDefault = false;

            var nr1 = _mod1.Areas[0];
            nr1.RestrictByDefault = false;

          
            var nr2 = _mod1.Areas[3];
            nr2.RestrictByDefault = false;

            var added2 = _mod1.Areas[2];
            AddAreaAccess(_role1, _mod1, added2);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            Assert.IsNotNull(module.Areas.SingleOrDefault(ar => ar.Id == added2.Id));

            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);

            Assert.AreEqual(3, module.Areas.Count);
        }

        [TestMethod]
        public async Task Should_Have_Added_Restricted_Areas_Without_NonRestrictedAreas()
        {
            _mod1.RestrictByDefault = false;

            var added2 = _mod1.Areas[2];

            AddAreaAccess(_role1, _mod1, added2);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            var addedArea = module.Areas.SingleOrDefault(ar => ar.Id == added2.Id);

            Assert.AreEqual(1, module.Areas.Count);
            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(addedArea.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
        }

        [TestMethod]
        public async Task Should_Have_Added_Restricted_Areas_Without_NonRestrictedAreas_All_Perms()
        {
            _mod1.RestrictByDefault = false;

            var added2 = _mod1.Areas[2];

            AddAreaAccess(_role1, _mod1, added2, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
          
            var addedArea = module.Areas.SingleOrDefault(ar => ar.Id == added2.Id);

            Assert.AreEqual(1, module.Areas.Count);
            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(addedArea.UserAccess, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant);
        }

        [TestMethod]
        public async Task Should_Have_Added_Restricted_Areas_Without_NonRestrictedAreas_All_Perms_Module_Restrcited()
        {
            var added2 = _mod1.Areas[2];

            AddAreaAccess(_role1, _mod1, added2, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);

            var addedArea = module.Areas.SingleOrDefault(ar => ar.Id == added2.Id);

            Assert.AreEqual(1, module.Areas.Count);
            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Grant, UserAccess.Revoke, UserAccess.Revoke);
            ValidateUserAccess(addedArea.UserAccess, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant, UserAccess.Grant);
        }


        [TestMethod]
        public async Task Should_Have_No_Access_When_Adding_All_Revoked_Area()
        {
            var added2 = _mod1.Areas[2];

            AddAreaAccess(_role1, _mod1, added2, UserAccess.Revoke, UserAccess.Revoke, UserAccess.Revoke, UserAccess.Revoke);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            ValidateUserAccess(module.UserAccess, UserAccess.Revoke, UserAccess.Revoke, UserAccess.Revoke, UserAccess.Revoke);
            Assert.AreEqual(0, module.Areas.Count);
        }
    }
}
