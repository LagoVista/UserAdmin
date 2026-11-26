using LagoVista.AspNetCore.Identity.Services;
using LagoVista.UserAdmin.Managers;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class BrowserInstallationStartupTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void OwningModulesRegisterInstallationServicesWithoutHostFeatureRegistration(bool enabled)
        {
            var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                { ["BrowserInstallation:Enabled"] = enabled.ToString() }).Build();
            var services = new ServiceCollection();
            LagoVista.UserAdmin.Startup.ConfigureServices(services, config);
            LagoVista.AspNetCore.Identity.Startup.ConfigureServices(services, config);
            Assert.That(services.Count(s => s.ServiceType == typeof(BrowserInstallationManager)), Is.EqualTo(1));
            foreach (var type in new[] { typeof(BrowserInstallationCookie), typeof(BrowserInstallationSession), typeof(BrowserInstallationSignIn) })
                Assert.That(services.Count(s => s.ServiceType == type), Is.EqualTo(enabled ? 1 : 0));
            Assert.That(services.Any(s => s.ServiceType == typeof(IPostConfigureOptions<CookieAuthenticationOptions>)), Is.EqualTo(enabled));
        }
    }
}
