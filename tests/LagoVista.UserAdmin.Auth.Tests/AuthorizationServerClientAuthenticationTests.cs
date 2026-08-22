using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using OpenIddict.Abstractions;
using OpenIddict.Server;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public class AuthorizationServerClientAuthenticationTests
    {
        [Test]
        public void ConfigureServices_AllowsPublicAndClientSecretPostAuthentication()
        {
            var services = new ServiceCollection();

            LagoVista.AspNetCore.AuthorizationServer.Startup.ConfigureServices(
                services,
                options => options.UseDevelopmentCertificates = true);

            using var provider = services.BuildServiceProvider();
            var options = provider.GetRequiredService<IOptions<OpenIddictServerOptions>>().Value;

            Assert.That(options.ClientAuthenticationMethods,
                Does.Contain(OpenIddictConstants.ClientAuthenticationMethods.None));
            Assert.That(options.ClientAuthenticationMethods,
                Does.Contain(OpenIddictConstants.ClientAuthenticationMethods.ClientSecretPost));
        }
    }
}
