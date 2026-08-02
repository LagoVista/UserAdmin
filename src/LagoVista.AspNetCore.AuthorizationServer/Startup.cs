using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services, Action<AuthorizationServerOptions> configure)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var settings = new AuthorizationServerOptions();
            configure?.Invoke(settings);

            services.AddSingleton(settings);
            services.AddScoped<IOAuthClientPolicyResolver, OAuthClientPolicyResolver>();
            services.AddScoped<IOAuthClientPolicyValidator, OAuthClientPolicyValidator>();

            services.AddControllers()
                .AddApplicationPart(typeof(AuthorizationController).Assembly);

            services.AddOpenIddict()
                .AddServer(options =>
                {
                    options.EnableDegradedMode();
                    options.AcceptAnonymousClients();

                    options.SetAuthorizationEndpointUris(AuthorizationServerConstants.AuthorizationEndpoint)
                           .SetTokenEndpointUris(AuthorizationServerConstants.TokenEndpoint);

                    options.AllowAuthorizationCodeFlow();
                    options.RequireProofKeyForCodeExchange();

                    var scopes = settings.Scopes
                        .Where(scope => !String.IsNullOrWhiteSpace(scope))
                        .Distinct(StringComparer.Ordinal)
                        .ToArray();

                    if (scopes.Length > 0)
                        options.RegisterScopes(scopes);

                    if (settings.Issuer != null)
                        options.SetIssuer(settings.Issuer);

                    if (settings.UseDevelopmentCertificates)
                    {
                        options.AddDevelopmentEncryptionCertificate()
                               .AddDevelopmentSigningCertificate();
                    }

                    if (settings.DisableAccessTokenEncryption)
                        options.DisableAccessTokenEncryption();

                    options.UseAspNetCore()
                           .EnableStatusCodePagesIntegration()
                           .EnableAuthorizationEndpointPassthrough();
                })
                .AddValidation(options =>
                {
                    options.UseLocalServer();
                    options.UseAspNetCore();
                });
        }
    }
}

namespace LagoVista.DependencyInjection
{
    public static class LagoVistaAuthorizationServerModule
    {
        public static void AddLagoVistaAuthorizationServer(this IServiceCollection services, Action<LagoVista.AspNetCore.AuthorizationServer.AuthorizationServerOptions> configure)
        {
            LagoVista.AspNetCore.AuthorizationServer.Startup.ConfigureServices(services, configure);
        }
    }
}
