using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class BrowserInstallationSignInTests
    {
        private const string UserId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string AppId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private static readonly Uri Origin = new Uri("https://app.example.com");

        private sealed class Harness
        {
            public readonly AppUser User = new AppUser { Id = new NormalizedId32(UserId), LoginType = LoginTypes.AppUser };
            public readonly Mock<IAppInstanceRepo> Repo = new Mock<IAppInstanceRepo>(MockBehavior.Strict);
            public readonly Mock<IAuthenticationService> Authentication = new Mock<IAuthenticationService>();
            public readonly BrowserInstallationCookie Cookie = new BrowserInstallationCookie(new EphemeralDataProtectionProvider());
            public readonly DefaultHttpContext Http = new DefaultHttpContext();
            public readonly BrowserInstallationSignIn Coordinator;
            private AppInstance _record;

            public Harness(bool configured = true)
            {
                Http.Request.Scheme = "https";
                Http.Request.Host = new HostString(Origin.Host);
                Http.RequestServices = new ServiceCollection().AddSingleton(Authentication.Object).BuildServiceProvider();
                Authentication.Setup(a => a.AuthenticateAsync(Http, IdentityConstants.ApplicationScheme))
                    .ReturnsAsync(AuthenticateResult.NoResult());
                Repo.Setup(r => r.GetAppInstanceAsync(UserId, It.IsAny<string>()))
                    .ReturnsAsync((string u, string id) => _record?.RowKey == id ? _record : null);
                Repo.Setup(r => r.AddAppInstanceAsync(It.IsAny<AppInstance>()))
                    .Callback<AppInstance>(r => _record = r).Returns(Task.CompletedTask);
                Coordinator = new BrowserInstallationSignIn(
                    new BrowserInstallationSession(Cookie, new BrowserInstallationManager(Repo.Object)),
                    _ => Task.FromResult(configured ? (AppId, Origin) : ((string)null, (Uri)null)));
            }

            public CookieSigningInContext Event(string scheme = null, bool restricted = false)
            {
                scheme ??= IdentityConstants.ApplicationScheme;
                var identity = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, UserId), new Claim(ClaimsFactory.CurrentUserId, UserId),
                    new Claim("amr", "pwd")
                }, IdentityConstants.ApplicationScheme);
                if (restricted) identity.AddClaim(new Claim(ClaimsFactory.IdentityStage, ClaimsFactory.ProvisionalIdentityStage));
                return new CookieSigningInContext(Http, new AuthenticationScheme(scheme, null, typeof(CookieAuthenticationHandler)),
                    new CookieAuthenticationOptions(), new ClaimsPrincipal(identity),
                    new AuthenticationProperties { IsPersistent = true }, new CookieOptions());
            }

            public Task Apply(CookieSigningInContext evt) => Coordinator.ApplyAsync(evt, id => Task.FromResult(id == UserId ? User : null));
        }

        [Test]
        public async Task ProvenApplicationSignInBindsAndPreservesExistingCookieProperties()
        {
            var h = new Harness();
            h.Cookie.Initialize(h.Http, Origin);
            var evt = h.Event();
            using (h.Coordinator.BeginProvenSignIn(h.User)) await h.Apply(evt);
            Assert.That(evt.Principal.FindAll(ClaimsFactory.AppInstanceId).Count(), Is.EqualTo(1));
            Assert.That(evt.Principal.FindFirst("amr").Value, Is.EqualTo("pwd"));
            Assert.That(evt.Properties.IsPersistent, Is.True);
            h.Repo.Verify(r => r.AddAppInstanceAsync(It.IsAny<AppInstance>()), Times.Once);
        }

        [Test]
        public async Task ReissueInSameRequestRetainsBindingWithoutCreatingNewInstance()
        {
            var h = new Harness();
            h.Cookie.Initialize(h.Http, Origin);
            var first = h.Event();
            using (h.Coordinator.BeginProvenSignIn(h.User)) await h.Apply(first);
            var next = h.Event();
            await h.Apply(next);
            Assert.That(next.Principal.FindFirst(ClaimsFactory.AppInstanceId).Value,
                Is.EqualTo(first.Principal.FindFirst(ClaimsFactory.AppInstanceId).Value));
            h.Repo.Verify(r => r.AddAppInstanceAsync(It.IsAny<AppInstance>()), Times.Once);
        }

        [Test]
        public async Task UnboundCookieAndSuppliedClaimNeverCreateBinding()
        {
            var h = new Harness();
            h.Cookie.Initialize(h.Http, Origin);
            var evt = h.Event();
            ((ClaimsIdentity)evt.Principal.Identity).AddClaim(new Claim(ClaimsFactory.AppInstanceId, AppId));
            await h.Apply(evt);
            Assert.That(evt.Principal.FindFirst(ClaimsFactory.AppInstanceId), Is.Null);
            h.Repo.VerifyNoOtherCalls();
        }

        [TestCase("missing-cookie")]
        [TestCase("missing-app")]
        [TestCase("restricted")]
        [TestCase("two-factor-scheme")]
        public async Task FullProofStillRequiresEligibleSessionAndTrustedContext(string scenario)
        {
            var h = new Harness(scenario != "missing-app");
            if (scenario != "missing-cookie") h.Cookie.Initialize(h.Http, Origin);
            var evt = h.Event(scenario == "two-factor-scheme" ? IdentityConstants.TwoFactorUserIdScheme : IdentityConstants.ApplicationScheme,
                scenario == "restricted");
            using (h.Coordinator.BeginProvenSignIn(h.User)) await h.Apply(evt);
            Assert.That(evt.Principal.FindFirst(ClaimsFactory.AppInstanceId), Is.Null);
            h.Repo.VerifyNoOtherCalls();
        }

        [Test]
        public async Task ExplicitSignoutCannotReuseOldRequestCookie()
        {
            var h = new Harness();
            h.Cookie.Initialize(h.Http, Origin);
            var first = h.Event();
            using (h.Coordinator.BeginProvenSignIn(h.User)) await h.Apply(first);
            h.Authentication.Setup(a => a.AuthenticateAsync(h.Http, IdentityConstants.ApplicationScheme))
                .ReturnsAsync(AuthenticateResult.Success(new AuthenticationTicket(first.Principal, IdentityConstants.ApplicationScheme)));
            h.Coordinator.ClearIssuedSession();
            var next = h.Event();
            await h.Apply(next);
            Assert.That(next.Principal.FindFirst(ClaimsFactory.AppInstanceId), Is.Null);
            h.Authentication.Verify(a => a.AuthenticateAsync(h.Http, IdentityConstants.ApplicationScheme), Times.Never);
        }
    }
}
