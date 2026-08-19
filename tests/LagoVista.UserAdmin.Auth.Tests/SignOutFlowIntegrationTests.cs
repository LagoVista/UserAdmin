using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Security;
using Moq;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class SignOutFlowIntegrationTests
    {
        private const string Evidence = "auth|auth.test-binding.session-sign-out|auth.flow.session-sign-out|auth.transition.session.sign-out-success";
        private const string TestUserId = "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";
        private const string TestOrganizationId = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";

        [Test]
        [Property("AptixEvidence", Evidence)]
        [Property("AptixAuthEvents", "UserLogout")]
        public async Task CookieSessionSignOut_Should_EndSession_AndRecordLogout()
        {
            var harness = CreateHarness();

            var result = await harness.FlowService.SignOutAsync(new SignOutRequest(), harness.Organization, harness.User);

            Assert.That(result.Successful, Is.True);
            harness.SignInManager.Verify(manager => manager.SignOutAsync(), Times.Once);
            harness.RefreshTokenManager.Verify(manager => manager.RevokeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[] { AuthLogTypes.UserLogout }));
        }

        [Test]
        [Property("AptixEvidence", Evidence)]
        [Property("AptixAuthEvents", "UserLogout")]
        public async Task TokenSessionSignOut_Should_RevokeCurrentRefreshToken_EndSession_AndRecordLogout()
        {
            var harness = CreateHarness();
            harness.RefreshTokenManager
                .Setup(manager => manager.RevokeRefreshTokenAsync("refresh-token", TestUserId))
                .Returns(Task.CompletedTask);

            var result = await harness.FlowService.SignOutAsync(
                new SignOutRequest { RefreshToken = "refresh-token" },
                harness.Organization,
                harness.User);

            Assert.That(result.Successful, Is.True);
            harness.RefreshTokenManager.Verify(manager => manager.RevokeRefreshTokenAsync("refresh-token", TestUserId), Times.Once);
            harness.SignInManager.Verify(manager => manager.SignOutAsync(), Times.Once);
            Assert.That(harness.Log.Events.Select(evt => evt.Type), Is.EqualTo(new AuthLogTypes?[] { AuthLogTypes.UserLogout }));
        }

        private static SignOutHarness CreateHarness()
        {
            var signInManager = new Mock<ISignInManager>(MockBehavior.Strict);
            signInManager.Setup(manager => manager.SignOutAsync()).Returns(Task.CompletedTask);

            var refreshTokenManager = new Mock<IRefreshTokenManager>(MockBehavior.Strict);
            var log = new RecordingAuthenticationLogManager();
            var signOutHandler = new SignOutFlowHandler(signInManager.Object, refreshTokenManager.Object, log);

            var passwordLoginHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Strict);
            var recoveryHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Strict);
            var flowService = new AuthenticationFlowService(
                passwordLoginHandler.Object,
                recoveryHandler.Object,
                signOutHandler: signOutHandler);

            return new SignOutHarness
            {
                FlowService = flowService,
                SignInManager = signInManager,
                RefreshTokenManager = refreshTokenManager,
                Log = log,
                Organization = EntityHeader.Create(TestOrganizationId, "Organization"),
                User = EntityHeader.Create(TestUserId, "User")
            };
        }

        private sealed class SignOutHarness
        {
            public AuthenticationFlowService FlowService { get; set; }
            public Mock<ISignInManager> SignInManager { get; set; }
            public Mock<IRefreshTokenManager> RefreshTokenManager { get; set; }
            public RecordingAuthenticationLogManager Log { get; set; }
            public EntityHeader Organization { get; set; }
            public EntityHeader User { get; set; }
        }
    }
}
