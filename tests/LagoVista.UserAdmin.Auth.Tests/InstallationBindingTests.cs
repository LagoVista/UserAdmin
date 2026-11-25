using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Utils;
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
        private const string Token = "token";

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
    }
}
