using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Tests.User
{
    [TestClass]
    public class AreaAccessTests : SecurityBase
    {
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

            Assert.AreEqual(2, module.Areas.Count);
        }

        [TestMethod]
        public async Task Should_Have_Added_Restricted_Areas()
        {
            _mod1.RestrictByDefault = false;

            var nr1 = _mod1.Areas[0];
            nr1.RestrictByDefault = false;

            var added2 = _mod1.Areas[2];
          
            var nr2 = _mod1.Areas[3];
            nr2.RestrictByDefault = false;

            AddAreaAccess(_role1, _mod1, added2);

            var module = await AccessManager.GetUserModuleAsync(_mod1.Id, USER_ID, ORG_ID);
            Assert.IsNotNull(module.Areas.SingleOrDefault(ar => ar.Id == nr1.Id));
            Assert.IsNotNull(module.Areas.SingleOrDefault(ar => ar.Id == nr2.Id));
            Assert.IsNotNull(module.Areas.SingleOrDefault(ar => ar.Id == added2.Id));

            Assert.AreEqual(3, module.Areas.Count);
        }
    }
}
