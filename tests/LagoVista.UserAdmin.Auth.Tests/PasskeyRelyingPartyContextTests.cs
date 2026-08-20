using LagoVista.AspNetCore.Identity.Services;
using LagoVista.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class PasskeyRelyingPartyContextTests
    {
        [Test]
        public void Current_WithHttpsRequest_Should_UseRequestHostInsteadOfConfiguredWebAddress()
        {
            var accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            accessor.HttpContext.Request.Scheme = "https";
            accessor.HttpContext.Request.Host = new HostString("demo.customer.example");

            var appConfig = new Mock<IAppConfig>(MockBehavior.Strict);
            appConfig.SetupGet(config => config.WebAddress).Returns("https://dev.nuviot.com");

            var context = new PasskeyRelyingPartyContext(accessor, appConfig.Object);

            Assert.That(context.Current.RpId, Is.EqualTo("demo.customer.example"));
            Assert.That(context.Current.Origin, Is.EqualTo("https://demo.customer.example"));
            Assert.That(context.Current.IsRequestScoped, Is.True);
        }

        [Test]
        public void Current_WithExplicitPort_Should_PreservePortInOriginButNotRpId()
        {
            var accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            accessor.HttpContext.Request.Scheme = "https";
            accessor.HttpContext.Request.Host = new HostString("localhost", 5001);

            var appConfig = new Mock<IAppConfig>(MockBehavior.Strict);
            appConfig.SetupGet(config => config.WebAddress).Returns("https://dev.nuviot.com");

            var context = new PasskeyRelyingPartyContext(accessor, appConfig.Object);

            Assert.That(context.Current.RpId, Is.EqualTo("localhost"));
            Assert.That(context.Current.Origin, Is.EqualTo("https://localhost:5001"));
            Assert.That(context.Current.IsRequestScoped, Is.True);
        }

        [Test]
        public void Current_WithoutHttpRequest_Should_FallBackToConfiguredWebAddress()
        {
            var accessor = new HttpContextAccessor();
            var appConfig = new Mock<IAppConfig>(MockBehavior.Strict);
            appConfig.SetupGet(config => config.WebAddress).Returns("https://dev.nuviot.com");

            var context = new PasskeyRelyingPartyContext(accessor, appConfig.Object);

            Assert.That(context.Current.RpId, Is.EqualTo("dev.nuviot.com"));
            Assert.That(context.Current.Origin, Is.EqualTo("https://dev.nuviot.com"));
            Assert.That(context.Current.IsRequestScoped, Is.False);
        }

        [Test]
        public void Current_WithUnsupportedRequestScheme_Should_FallBackToConfiguredWebAddress()
        {
            var accessor = new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
            accessor.HttpContext.Request.Scheme = "ftp";
            accessor.HttpContext.Request.Host = new HostString("ignored.example.com");

            var appConfig = new Mock<IAppConfig>(MockBehavior.Strict);
            appConfig.SetupGet(config => config.WebAddress).Returns("https://dev.nuviot.com");

            var context = new PasskeyRelyingPartyContext(accessor, appConfig.Object);

            Assert.That(context.Current.RpId, Is.EqualTo("dev.nuviot.com"));
            Assert.That(context.Current.Origin, Is.EqualTo("https://dev.nuviot.com"));
            Assert.That(context.Current.IsRequestScoped, Is.False);
        }
    }
}
