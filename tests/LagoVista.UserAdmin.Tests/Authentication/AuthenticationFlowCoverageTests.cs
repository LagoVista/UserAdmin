using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace LagoVista.UserAdmin.Tests.Authentication
{
    [TestClass]
    public class AuthenticationFlowCoverageTests
    {
        [TestMethod]
        public void AuthenticationFlowService_Should_Be_Marked_For_Critical_Coverage()
        {
            var hasCriticalCoverage = typeof(AuthenticationFlowService)
                .GetCustomAttributes(typeof(CriticalCoverageAttribute), true)
                .Any();

            Assert.IsTrue(hasCriticalCoverage);
        }

        [TestMethod]
        public void PasswordRecoveryRequestFlowHandler_Should_Be_Marked_For_Critical_Coverage()
        {
            var hasCriticalCoverage = typeof(PasswordRecoveryRequestFlowHandler)
                .GetCustomAttributes(typeof(CriticalCoverageAttribute), true)
                .Any();

            Assert.IsTrue(hasCriticalCoverage);
        }

        [TestMethod]
        public void AuthenticationFlowService_Should_Require_Handler()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new AuthenticationFlowService(null, null));
        }

        [TestMethod]
        public void PasswordRecoveryRequestFlowHandler_Should_Require_Password_Manager()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new PasswordRecoveryRequestFlowHandler(null));
        }
    }
}
