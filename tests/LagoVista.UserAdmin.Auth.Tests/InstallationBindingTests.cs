using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Utils;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class InstallationBindingTests
    {
        private const string User = "185B3AA7D5244C0D98EA60F77A13ACDC";
        private const string App = "app";
        private const string Installation = "installation";
        private const string Token = "0123456789012345678.185B3AA7D5244C0D98EA60F77A13ACDC";

        [TestCase(false)]
        [TestCase(true)]
        public void SignedJwtIncludesInstallationAcrossOrganizationOverloads(bool explicitOrg)
        {
            var key = new SymmetricSecurityKey(new byte[32]);
            var options = new Mock<ITokenAuthOptions>();
            options.SetupGet(x => x.Issuer).Returns("test-issuer");
            options.SetupGet(x => x.Audience).Returns("test-audience");
            options.SetupGet(x => x.SigningCredentials).Returns(new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            var factory = new Mock<IClaimsFactory>();
            factory.Setup(x => x.GetClaims(It.IsAny<AppUser>())).Returns(() => new List<Claim> { new Claim(ClaimsFactory.CurrentUserId, User) });
            factory.Setup(x => x.GetClaims(It.IsAny<AppUser>(), It.IsAny<EntityHeader>(), false, false)).Returns(() => new List<Claim> { new Claim(ClaimsFactory.CurrentUserId, User), new Claim(ClaimsFactory.CurrentOrgId, "other-org") });
            var helper = new TokenHelper(options.Object, Mock.Of<IUserRoleRepo>(), Mock.Of<IOrganizationManager>(), Mock.Of<IOrgUserRepo>(), factory.Object, Mock.Of<IAdminLogger>());
            var user = new AppUser { Id = User };
            var expiry = DateTime.UtcNow.AddMinutes(5);
            var jwt = explicitOrg ? helper.GetJWToken(user, new EntityHeader { Id = "other-org" }, false, false, expiry, Installation) : helper.GetJWToken(user, expiry, Installation);
            var principal = new JwtSecurityTokenHandler().ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuer = true, ValidIssuer = "test-issuer",
                ValidateAudience = true, ValidAudience = "test-audience",
                ValidateIssuerSigningKey = true, IssuerSigningKey = key,
                ValidateLifetime = true
            }, out _);
            Assert.That(principal.FindAll(ClaimsFactory.AppInstanceId).Single().Value, Is.EqualTo(Installation));
            Assert.That(principal.FindFirst(ClaimsFactory.CurrentUserId).Value, Is.EqualTo(User));
            Assert.That(ClaimsFactory.GetAuthenticatedAppInstanceId(principal), Is.EqualTo(Installation));
        }

        [TestCase("wrong-app", false)]
        [TestCase(App, true)]
        public async Task RefreshRejectsWrongOrMissingCanonicalInstallation(string requestedApp, bool missing)
        {
            var repo = new Mock<IAppInstanceRepo>(MockBehavior.Strict);
            repo.Setup(x => x.GetAppInstanceAsync(User, Installation)).ReturnsAsync(missing ? null : new AppInstance(Installation, User) { AppId = App, UserId = User });
            var manager = new AppInstanceManager(repo.Object, Mock.Of<IAdminLogger>());
            var result = await manager.UpdateLastAccessTokenRefreshAsync(User, new AuthRequest { AppId = requestedApp, AppInstanceId = Installation });
            Assert.That(result.Successful, Is.False);
            repo.Verify(x => x.UpdateAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
            repo.Verify(x => x.AddAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
        }

        [Test]
        public async Task LoginRejectsExistingInstallationForDifferentApp()
        {
            var repo = new Mock<IAppInstanceRepo>(MockBehavior.Strict);
            repo.Setup(x => x.GetAppInstanceAsync(User, Installation)).ReturnsAsync(new AppInstance(Installation, User) { AppId = App, UserId = User });
            var manager = new AppInstanceManager(repo.Object, Mock.Of<IAdminLogger>());
            var result = await manager.UpdateLastLoginAsync(User, new AuthRequest { AppId = "wrong-app", AppInstanceId = Installation });
            Assert.That(result.Successful, Is.False);
            repo.Verify(x => x.UpdateAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
        }

        [TestCase("wrong-user", App, Installation)]
        [TestCase(User, "wrong-app", Installation)]
        [TestCase(User, App, "wrong-installation")]
        public async Task RotationRejectsMismatchedPersistedBinding(string storedUser, string storedApp, string storedInstallation)
        {
            var repo = new Mock<IRefreshTokenRepo>(MockBehavior.Strict);
            repo.Setup(x => x.GetRefreshTokenAsync(Token, User)).ReturnsAsync(new RefreshToken(storedUser) { RowKey = Token, AppId = storedApp, AppInstanceId = storedInstallation });
            var manager = new RefreshTokenManager(Mock.Of<ITokenAuthOptions>(), Mock.Of<IAuthenticationLogManager>(), Mock.Of<IAuthRequestValidators>(), repo.Object, Mock.Of<IAdminLogger>());
            var result = await manager.RenewRefreshTokenAsync(Token, User, App, Installation);
            Assert.That(result.Successful, Is.False);
            repo.Verify(x => x.TryConsumeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            repo.Verify(x => x.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [TestCase("missing")]
        [TestCase("duplicate")]
        [TestCase("unauthenticated")]
        [TestCase("visitor")]
        [TestCase("provisional")]
        public void UntrustedPrincipalCannotEstablishInstallationBinding(string scenario)
        {
            var claims = new List<Claim> { new Claim(ClaimsFactory.CurrentUserId, User) };
            if (scenario != "missing") claims.Add(new Claim(ClaimsFactory.AppInstanceId, Installation));
            if (scenario == "duplicate") claims.Add(new Claim(ClaimsFactory.AppInstanceId, "another"));
            if (scenario == "visitor") claims.Add(new Claim(ClaimsFactory.IdentityStage, ClaimsFactory.VisitorIdentityStage));
            if (scenario == "provisional") claims.Add(new Claim(ClaimsFactory.IdentityStage, ClaimsFactory.ProvisionalIdentityStage));
            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, scenario == "unauthenticated" ? null : "Bearer"));
            Assert.That(ClaimsFactory.GetAuthenticatedAppInstanceId(principal), Is.Null);
        }

        [TestCase("expired")]
        [TestCase("revoked")]
        [TestCase("malformed-expiry")]
        public async Task RealRefreshValidationRejectsUnavailableOrExpiredTokenBeforeConsumption(string scenario)
        {
            var repo = new Mock<IRefreshTokenRepo>();
            var token = new RefreshToken(User) { RowKey = Token, AppId = App, AppInstanceId = Installation,
                ExpiresUtc = scenario == "malformed-expiry" ? "invalid" : DateTime.UtcNow.AddMinutes(-5).ToString("O") };
            repo.Setup(x => x.GetRefreshTokenAsync(Token, User)).ReturnsAsync(scenario == "revoked" ? null : token);
            var logger = Mock.Of<IAdminLogger>();
            var validator = new AuthRequestValidators(logger, repo.Object, Mock.Of<IUserManager>());
            var manager = new RefreshTokenManager(Mock.Of<ITokenAuthOptions>(), Mock.Of<IAuthenticationLogManager>(), validator, repo.Object, logger);
            var result = await manager.RenewRefreshTokenAsync(Token, User, App, Installation);
            Assert.That(result.Successful, Is.False);
            repo.Verify(x => x.TryConsumeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            repo.Verify(x => x.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [TestCase("password")]
        [TestCase("single-use")]
        [TestCase("refresh")]
        [TestCase("switch-org")]
        [TestCase("wrong-app")]
        [TestCase("wrong-token-binding")]
        [TestCase("revoked")]
        [TestCase("expired")]
        public async Task RealGrantIssuesSignedCanonicalInstallation(string grant)
        {
            var user = new AppUser("user@example.com", "test-user", "system") { Id = User, UserName = "user@example.com", CurrentOrganization = new OrganizationSummary { Id = User, Text = "Initial organization", Namespace = "initial" } };
            var users = new Mock<IUserManager>();
            users.Setup(x => x.FindByNameAsync(user.UserName)).ReturnsAsync(user);
            users.Setup(x => x.FindByIdAsync(User)).ReturnsAsync(user);
            var signIn = new Mock<ISignInManager>();
            signIn.Setup(x => x.PasswordSignInAsync(It.IsAny<AuthLoginRequest>())).ReturnsAsync(InvokeResult<AuthenticationResponse>.Create(new AuthenticationResponse { AuthenticationState = AuthenticationResponseState.Authenticated }));
            var singleUse = new Mock<ISingleUseTokenManager>();
            singleUse.Setup(x => x.ValidationAsync(User, "single")).ReturnsAsync(InvokeResult.Success);
            var log = Mock.Of<IAdminLogger>();
            var authLog = Mock.Of<IAuthenticationLogManager>();
            var instances = new Mock<IAppInstanceRepo>();
            var instance = new AppInstance(Installation, User) { AppId = App, UserId = User };
            instances.Setup(x => x.GetAppInstanceAsync(User, Installation)).ReturnsAsync(instance);
            var tokens = new Mock<IRefreshTokenRepo>();
            var previous = new RefreshToken(User) { RowKey = Token, AppId = App, AppInstanceId = grant == "wrong-token-binding" ? "other-installation" : Installation, ExpiresUtc = DateTime.UtcNow.AddHours(grant == "expired" ? -1 : 1).ToString("O") };
            tokens.Setup(x => x.GetRefreshTokenAsync(Token, User)).ReturnsAsync(grant == "revoked" ? null : previous);
            tokens.Setup(x => x.TryConsumeRefreshTokenAsync(Token, User)).ReturnsAsync(true);
            var key = new SymmetricSecurityKey(new byte[32]);
            var options = new Mock<ITokenAuthOptions>();
            options.SetupGet(x => x.Issuer).Returns("test-issuer");
            options.SetupGet(x => x.Audience).Returns("test-audience");
            options.SetupGet(x => x.AccessExpiration).Returns(TimeSpan.FromMinutes(15));
            options.SetupGet(x => x.RefreshExpiration).Returns(TimeSpan.FromDays(7));
            options.SetupGet(x => x.SigningCredentials).Returns(new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
            var claims = new Mock<IClaimsFactory>();
            claims.Setup(x => x.GetClaims(It.IsAny<AppUser>())).Returns(() => new List<Claim> { new Claim(ClaimsFactory.CurrentUserId, User) });
            const string otherOrg = "ACDC3AA7D5244C0D98EA60F77A131CA8";
            var orgs = new Mock<IOrganizationManager>();
            orgs.Setup(x => x.GetOrganizationsForUserAsync(User, It.IsAny<EntityHeader>(), It.IsAny<EntityHeader>())).ReturnsAsync(new List<OrgUser> { new OrgUser(otherOrg, User) });
            var roles = new Mock<IUserRoleRepo>();
            roles.Setup(x => x.GetRolesForUserAsync(User, otherOrg)).ReturnsAsync(new List<UserRole>());
            claims.Setup(x => x.GetClaims(It.IsAny<AppUser>(), It.IsAny<EntityHeader>(), false, false)).Returns(() => new List<Claim> { new Claim(ClaimsFactory.CurrentUserId, User), new Claim(ClaimsFactory.CurrentOrgId, otherOrg) });
            var helper = new TokenHelper(options.Object, roles.Object, orgs.Object, Mock.Of<IOrgUserRepo>(), claims.Object, log);
            var validators = new AuthRequestValidators(log, tokens.Object, users.Object);
            var manager = new AuthTokenManager(instances.Object, authLog, singleUse.Object, orgs.Object, new RefreshTokenManager(options.Object, authLog, validators, tokens.Object, log), validators, Mock.Of<IOrganizationRepo>(), helper, new AppInstanceManager(instances.Object, log), log, signIn.Object, users.Object);
            var request = new AuthRequest { AppId = App, AppInstanceId = Installation, UserId = User, UserName = user.UserName, Email = user.UserName, Password = "password", SingleUseToken = "single", RefreshToken = Token, ClientType = "APP", DeviceId = "device", AuthType = AuthTypes.User };
            if (grant == "switch-org") { request.OrgId = otherOrg; request.OrgName = "Other organization"; }
            if (grant == "wrong-app") request.AppId = "wrong-app";
            var response = grant == "password" ? await manager.AccessTokenGrantAsync(request) :
                grant == "single-use" ? await manager.SingleUseTokenGrantAsync(request) : await manager.RefreshTokenGrantAsync(request);
            if (grant == "wrong-app" || grant == "wrong-token-binding" || grant == "revoked" || grant == "expired")
            {
                Assert.That(response.Successful, Is.False);
                instances.Verify(x => x.UpdateAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
                instances.Verify(x => x.AddAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
                tokens.Verify(x => x.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
                tokens.Verify(x => x.TryConsumeRefreshTokenAsync(Token, User), Times.Never);
                return;
            }
            Assert.That(response.Successful, Is.True, response.ErrorMessage);
            var principal = new JwtSecurityTokenHandler().ValidateToken(response.Result.AccessToken, new TokenValidationParameters
            {
                ValidIssuer = "test-issuer", ValidAudience = "test-audience",
                IssuerSigningKey = key, ValidateIssuerSigningKey = true, ValidateLifetime = true
            }, out _);
            Assert.That(ClaimsFactory.GetAuthenticatedAppInstanceId(principal), Is.EqualTo(Installation));
            Assert.That(response.Result.AppInstanceId, Is.EqualTo(Installation));
            if (grant == "switch-org") Assert.That(principal.FindFirst(ClaimsFactory.CurrentOrgId).Value, Is.EqualTo(otherOrg));
            tokens.Verify(x => x.SaveRefreshTokenAsync(It.Is<RefreshToken>(t => t.AppId == App && t.AppInstanceId == Installation && t.PartitionKey == User)), Times.Once);
            instances.Verify(x => x.UpdateAppInstanceAsync(instance), Times.Once);
            instances.Verify(x => x.AddAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
            tokens.Verify(x => x.TryConsumeRefreshTokenAsync(Token, User), grant == "refresh" || grant == "switch-org" ? Times.Once() : Times.Never());
        }

        [Test]
        public async Task ExistingObjectRenewalOverloadRejectsCallerSubstitutedBinding()
        {
            var repo = new Mock<IRefreshTokenRepo>(MockBehavior.Strict);
            repo.Setup(x => x.GetRefreshTokenAsync(Token, User)).ReturnsAsync(new RefreshToken(User) { RowKey = Token, AppId = App, AppInstanceId = Installation });
            var manager = new RefreshTokenManager(Mock.Of<ITokenAuthOptions>(), Mock.Of<IAuthenticationLogManager>(), Mock.Of<IAuthRequestValidators>(), repo.Object, Mock.Of<IAdminLogger>());
            var result = await manager.RenewRefreshTokenAsync(new RefreshToken(User) { RowKey = Token, AppId = App, AppInstanceId = "substituted" });
            Assert.That(result.Successful, Is.False);
            repo.Verify(x => x.TryConsumeRefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            repo.Verify(x => x.SaveRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
        }
    }
}
