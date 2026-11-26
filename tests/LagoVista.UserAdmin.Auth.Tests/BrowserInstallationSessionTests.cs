using LagoVista.AspNetCore.Identity.Managers;
using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class BrowserInstallationSessionTests
    {
        private const string User = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string Other = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private const string App = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
        private static readonly Uri Origin = new Uri("https://app.example.com");

        private static DefaultHttpContext Request(string cookie = null)
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = "https";
            context.Request.Host = new HostString(Origin.Host);
            if (cookie != null) context.Request.Headers.Cookie = cookie;
            return context;
        }

        private static ClaimsPrincipal Principal(string userId = User, string auth = "Identity.Application")
            => new ClaimsPrincipal(new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, userId), new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimsFactory.CurrentUserId, userId)
            }, auth));

        private static AppUser Human() => new AppUser { Id = new NormalizedId32(User), LoginType = LoginTypes.AppUser };

        private static (BrowserInstallationSession Session, BrowserInstallationCookie Cookie, Mock<IAppInstanceRepo> Repo) Harness()
        {
            var cookie = new BrowserInstallationCookie(new EphemeralDataProtectionProvider());
            var repo = new Mock<IAppInstanceRepo>(MockBehavior.Strict);
            AppInstance record = null;
            repo.Setup(r => r.GetAppInstanceAsync(User, It.IsAny<string>()))
                .ReturnsAsync((string user, string id) => record?.RowKey == id ? record : null);
            repo.Setup(r => r.AddAppInstanceAsync(It.IsAny<AppInstance>()))
                .Callback<AppInstance>(r => record = r).Returns(Task.CompletedTask);
            return (new BrowserInstallationSession(cookie, new BrowserInstallationManager(repo.Object)), cookie, repo);
        }

        [Test]
        public async Task ProvenSessionBindsOnceAndRenewalReusesWithoutMutation()
        {
            var h = Harness();
            var context = Request();
            h.Cookie.Initialize(context, Origin);
            var principal = Principal();
            var first = await h.Session.BindProvenSessionAsync(context, principal, Human(), App, Origin);
            Assert.That(principal.FindAll(ClaimsFactory.AppInstanceId).Single().Value, Is.EqualTo(first.RowKey));
            var next = Request(context.Response.Headers.SetCookie.Single().Split(';')[0]);
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimsFactory.CurrentOrgId, Other));
            var same = await h.Session.ReadBoundSessionAsync(next, principal, Human(), App, Origin);
            Assert.That(same.RowKey, Is.EqualTo(first.RowKey));
            var relogin = await h.Session.BindProvenSessionAsync(next, Principal(), Human(), App, Origin);
            Assert.That(relogin.RowKey, Is.EqualTo(first.RowKey));
            h.Repo.Verify(r => r.AddAppInstanceAsync(It.IsAny<AppInstance>()), Times.Once);
            h.Repo.Verify(r => r.UpdateAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
        }

        [TestCase("anonymous")]
        [TestCase("provisional")]
        [TestCase("visitor")]
        [TestCase("wrong-user")]
        [TestCase("duplicate-user")]
        [TestCase("conflicting-name")]
        [TestCase("unauthenticated")]
        [TestCase("preexisting-claim")]
        public void RejectedPrincipalNeverCreatesInstallation(string kind)
        {
            var h = Harness();
            var context = Request();
            h.Cookie.Initialize(context, Origin);
            var principal = kind == "wrong-user" ? Principal(Other) :
                kind == "unauthenticated" ? Principal(User, null) : Principal();
            var identity = (ClaimsIdentity)principal.Identity;
            if (kind == "anonymous") identity.AddClaim(new Claim(ClaimsFactory.Anonymous, "true"));
            if (kind == "provisional") identity.AddClaim(new Claim(ClaimsFactory.IdentityStage, ClaimsFactory.ProvisionalIdentityStage));
            if (kind == "visitor") identity.AddClaim(new Claim(ClaimsFactory.IdentityStage, ClaimsFactory.VisitorIdentityStage));
            if (kind == "duplicate-user") identity.AddClaim(new Claim(ClaimsFactory.CurrentUserId, User));
            if (kind == "conflicting-name") identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, Other));
            if (kind == "preexisting-claim") identity.AddClaim(new Claim(ClaimsFactory.AppInstanceId, Other));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await h.Session.BindProvenSessionAsync(context, principal, Human(), App, Origin));
            h.Repo.VerifyNoOtherCalls();
        }

        [Test]
        public void UnboundLegacySessionOrMissingCookieDoesNotCreateInstallation()
        {
            var h = Harness();
            Assert.ThrowsAsync<InvalidOperationException>(async () => await h.Session.BindProvenSessionAsync(Request(), Principal(), Human(), App, Origin));
            var context = Request();
            h.Cookie.Initialize(context, Origin);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await h.Session.ReadBoundSessionAsync(context, Principal(), Human(), App, Origin));
            h.Repo.VerifyNoOtherCalls();
        }

        [Test]
        public async Task RenewedSessionRejectsCookieLossAndWrongAppAndDuplicateClaim()
        {
            var h = Harness();
            var context = Request();
            h.Cookie.Initialize(context, Origin);
            var principal = Principal();
            await h.Session.BindProvenSessionAsync(context, principal, Human(), App, Origin);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await h.Session.ReadBoundSessionAsync(Request(), principal, Human(), App, Origin));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await h.Session.ReadBoundSessionAsync(context, principal, Human(), Other, Origin));
            ((ClaimsIdentity)principal.Identity).AddClaim(new Claim(ClaimsFactory.AppInstanceId, Other));
            Assert.ThrowsAsync<InvalidOperationException>(async () => await h.Session.ReadBoundSessionAsync(context, principal, Human(), App, Origin));
            h.Repo.Verify(r => r.AddAppInstanceAsync(It.IsAny<AppInstance>()), Times.Once);
            h.Repo.Verify(r => r.UpdateAppInstanceAsync(It.IsAny<AppInstance>()), Times.Never);
        }
    }
}
