using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Authentication;
using LagoVista.UserAdmin.Authentication.Flows;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Users;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class ProvenUserSessionCompletionTests
    {
        private const string PasskeyEvidence = "auth|auth.test-binding.passkey-sign-in.session-completion|auth.behavior.passkey.sign-in|auth.transition.passkey.complete-authentication";
        private const string UserId = "R1111111111111111111111111111111";
        private const string Email = "user@example.com";

        [Test]
        [Property("AptixEvidence", PasskeyEvidence)]
        public async Task CompleteProvenUserSession_WithDurableUser_Should_EstablishBrowserSessionAndReturnResolvedDestination()
        {
            var harness = CreateHarness();
            var expected = new AuthenticationResponse
            {
                AuthenticationState = AuthenticationResponseState.Authenticated,
                CanEnterApplication = true,
                RedirectPage = "/home"
            };

            harness.SignInManager.Setup(manager => manager.SignInAsync(harness.User, true)).Returns(Task.CompletedTask);
            harness.SignInManager
                .Setup(manager => manager.CompleteSignInToAppAsync(harness.User, null, "", ""))
                .ReturnsAsync(InvokeResult<AuthenticationResponse>.Create(expected));

            var result = await harness.Service.CompleteProvenUserSessionAsync(harness.User, true);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result, Is.SameAs(expected));
            Assert.That(result.Result.RedirectPage, Is.EqualTo("/home"));
            harness.SignInManager.Verify(manager => manager.SignInAsync(harness.User, true), Times.Once);
            harness.SignInManager.Verify(manager => manager.CompleteSignInToAppAsync(harness.User, null, "", ""), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", PasskeyEvidence)]
        public async Task CompleteProvenUserToken_WithDurableUser_Should_MintSingleUseTokenBeforeIssuingMobileTokens()
        {
            var harness = CreateHarness();
            var request = new AuthRequest
            {
                AppId = "test-app",
                AppInstanceId = "test-instance",
                Email = Email,
                UserName = Email
            };

            harness.AuthTokenManager
                .Setup(manager => manager.GenerateOneTimeUseTokenAsync(UserId, null))
                .ReturnsAsync(InvokeResult<SingleUseToken>.Create(new SingleUseToken
                {
                    UserId = UserId,
                    Token = "single-use-token",
                    Expires = DateTime.UtcNow.AddMinutes(5).ToString("O")
                }));
            harness.AuthTokenManager
                .Setup(manager => manager.SingleUseTokenGrantAsync(It.Is<AuthRequest>(auth =>
                    auth == request &&
                    auth.GrantType == "single-use-token" &&
                    auth.UserId == UserId &&
                    auth.UserName == Email &&
                    auth.SingleUseToken == "single-use-token")))
                .ReturnsAsync(InvokeResult<AuthResponse>.Create(new AuthResponse
                {
                    AccessToken = "access-token",
                    RefreshToken = "refresh-token"
                }));

            var result = await harness.Service.CompleteProvenUserTokenAsync(request, harness.User);

            Assert.That(result.Successful, Is.True);
            Assert.That(result.Result.AccessToken, Is.EqualTo("access-token"));
            Assert.That(result.Result.RefreshToken, Is.EqualTo("refresh-token"));
            harness.AuthTokenManager.Verify(manager => manager.GenerateOneTimeUseTokenAsync(UserId, null), Times.Once);
            harness.AuthTokenManager.Verify(manager => manager.SingleUseTokenGrantAsync(It.Is<AuthRequest>(auth =>
                auth.GrantType == "single-use-token" &&
                auth.UserId == UserId &&
                auth.SingleUseToken == "single-use-token")), Times.Once);
        }

        [Test]
        [Property("AptixEvidence", PasskeyEvidence)]
        public void CompleteProvenUserSession_WithoutDurableUser_Should_RejectBeforeSessionMutation()
        {
            var harness = CreateHarness();

            Assert.ThrowsAsync<ArgumentException>(async () => await harness.Service.CompleteProvenUserSessionAsync(null, true));

            harness.SignInManager.Verify(manager => manager.SignInAsync(It.IsAny<AppUser>(), It.IsAny<bool>()), Times.Never);
            harness.SignInManager.Verify(manager => manager.CompleteSignInToAppAsync(It.IsAny<AppUser>(), null, "", ""), Times.Never);
        }

        [Test]
        [Property("AptixEvidence", PasskeyEvidence)]
        public void CompleteProvenUserToken_WithoutDurableUser_Should_RejectBeforeTokenMutation()
        {
            var harness = CreateHarness();

            Assert.ThrowsAsync<ArgumentException>(async () => await harness.Service.CompleteProvenUserTokenAsync(new AuthRequest(), null));

            harness.AuthTokenManager.Verify(manager => manager.GenerateOneTimeUseTokenAsync(It.IsAny<string>(), It.IsAny<TimeSpan?>()), Times.Never);
            harness.AuthTokenManager.Verify(manager => manager.SingleUseTokenGrantAsync(It.IsAny<AuthRequest>()), Times.Never);
        }

        private static Harness CreateHarness()
        {
            var passwordHandler = new Mock<IPasswordLoginFlowHandler>(MockBehavior.Loose);
            var passwordRecoveryRequestHandler = new Mock<IAuthenticationFlowHandler<PasswordRecoveryRequestFlowRequest>>(MockBehavior.Loose);
            var signInManager = new Mock<ISignInManager>(MockBehavior.Strict);
            var authTokenManager = new Mock<IAuthTokenManager>(MockBehavior.Strict);
            var user = new AppUser(Email, "test")
            {
                Id = UserId,
                Email = Email,
                UserName = Email,
                EmailConfirmed = true
            };

            return new Harness
            {
                User = user,
                SignInManager = signInManager,
                AuthTokenManager = authTokenManager,
                Service = new AuthenticationFlowService(
                    passwordHandler.Object,
                    passwordRecoveryRequestHandler.Object,
                    signInManager: signInManager.Object,
                    authTokenManager: authTokenManager.Object)
            };
        }

        private sealed class Harness
        {
            public AuthenticationFlowService Service { get; set; }
            public AppUser User { get; set; }
            public Mock<ISignInManager> SignInManager { get; set; }
            public Mock<IAuthTokenManager> AuthTokenManager { get; set; }
        }
    }
}
