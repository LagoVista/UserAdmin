using LagoVista.AspNetCore.Identity.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class BrowserInstallationCookieTests
    {
        private static readonly Uri Origin = new Uri("https://app.example.com");
        private static DefaultHttpContext Request(Uri origin, string cookie = null)
        {
            var context = new DefaultHttpContext();
            context.Request.Scheme = origin.Scheme;
            context.Request.Host = HostString.FromUriComponent(origin.Authority);
            if (cookie != null) context.Request.Headers.Cookie = cookie;
            return context;
        }

        private static string CookiePair(DefaultHttpContext context)
            => context.Response.Headers.SetCookie.Single().Split(';')[0];

        [Test]
        public void ProtectedHostCookiePersistsAcrossRequestsAndHelperInstances()
        {
            var provider = new EphemeralDataProtectionProvider();
            var helper = new BrowserInstallationCookie(provider);
            var first = Request(Origin);
            var id = helper.Initialize(first, Origin);
            Assert.That(helper.Initialize(first, Origin), Is.EqualTo(id));
            var header = first.Response.Headers.SetCookie.Single();
            Assert.That(header, Does.StartWith("__Host-nuviot-installation-"));
            Assert.That(header, Does.Contain("path=/").IgnoreCase);
            Assert.That(header, Does.Contain("secure").IgnoreCase);
            Assert.That(header, Does.Contain("httponly").IgnoreCase);
            Assert.That(header, Does.Contain("samesite=lax").IgnoreCase);
            Assert.That(header, Does.Contain("max-age=31536000").IgnoreCase);
            Assert.That(header, Does.Not.Contain("domain=").IgnoreCase);
            Assert.That(header, Does.Not.Contain(id));
            var next = Request(Origin, CookiePair(first));
            Assert.That(new BrowserInstallationCookie(provider).Read(next, Origin), Is.EqualTo(id));
            Assert.That(helper.Initialize(next, Origin), Is.EqualTo(id));
            Assert.That(next.Response.Headers.SetCookie.Count, Is.Zero);
        }

        [Test]
        public void MissingCookieDoesNotCreateIdentityOnRead()
        {
            var helper = new BrowserInstallationCookie(new EphemeralDataProtectionProvider());
            var request = Request(Origin);
            Assert.That(helper.Read(request, Origin), Is.Null);
            Assert.That(request.Response.Headers.SetCookie.Count, Is.Zero);
        }

        [Test]
        public void ProfilesAndSiteDataResetGetDistinctIdentifiers()
        {
            var helper = new BrowserInstallationCookie(new EphemeralDataProtectionProvider());
            var first = helper.Initialize(Request(Origin), Origin);
            Assert.That(helper.Initialize(Request(Origin), Origin), Is.Not.EqualTo(first));
        }

        [Test]
        public void TamperedCookieCannotEstablishContinuity()
        {
            var helper = new BrowserInstallationCookie(new EphemeralDataProtectionProvider());
            var request = Request(Origin);
            var id = helper.Initialize(request, Origin);
            var pair = CookiePair(request);
            var name = pair.Split('=')[0];
            var next = Request(Origin, name + "=invalid");
            Assert.That(helper.Read(next, Origin), Is.Null);
            Assert.That(helper.Initialize(next, Origin), Is.Not.EqualTo(id));
        }

        [Test]
        public void SeparateOriginAndPortCannotReuseProtectedCookie()
        {
            var helper = new BrowserInstallationCookie(new EphemeralDataProtectionProvider());
            var first = Request(Origin);
            var id = helper.Initialize(first, Origin);
            foreach (var other in new[] { new Uri("https://other.example.com"), new Uri("https://app.example.com:8443") })
            {
                var next = Request(other, CookiePair(first));
                Assert.That(helper.Read(next, other), Is.Null);
                Assert.That(helper.Initialize(next, other), Is.Not.EqualTo(id));
                var otherName = CookiePair(next).Split('=')[0];
                Assert.That(helper.Read(Request(other, otherName + "=" + CookiePair(first).Split('=')[1]), other), Is.Null);
            }
        }

        [Test]
        public void HostMismatchAndInsecureRequestCannotInitialize()
        {
            var helper = new BrowserInstallationCookie(new EphemeralDataProtectionProvider());
            Assert.Throws<InvalidOperationException>(() => helper.Initialize(Request(new Uri("https://other.example.com")), Origin));
            Assert.Throws<InvalidOperationException>(() => helper.Initialize(Request(new Uri("http://app.example.com")), Origin));
            Assert.Throws<InvalidOperationException>(() => helper.Initialize(Request(Origin), new Uri("https://app.example.com/path")));
        }
    }
}
