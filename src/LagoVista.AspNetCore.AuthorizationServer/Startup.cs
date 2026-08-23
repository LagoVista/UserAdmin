using LagoVista.AspNetCore.AuthorizationServer.Persistence;
using LagoVista.AspNetCore.AuthorizationServer.Security;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using System;
using System.Linq;
using static OpenIddict.Server.OpenIddictServerEvents;

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

            if (!settings.UseDevelopmentCertificates)
            {
                services.AddSingleton<IConfigureOptions<OpenIddictServerOptions>, OpenIddictSecureStorageCredentialConfigurator>();
            }

            services.AddControllers()
                .AddApplicationPart(typeof(AuthorizationController).Assembly);

            services.AddOpenIddict()
                .AddLagoVistaPersistence()
                .AddServer(options =>
                {
                    options.DisableAuthorizationStorage();

                    options.SetAuthorizationEndpointUris(AuthorizationServerConstants.AuthorizationEndpoint)
                           .SetTokenEndpointUris(AuthorizationServerConstants.TokenEndpoint)
                           .SetUserInfoEndpointUris(AuthorizationServerConstants.UserInfoEndpoint)
                           .SetEndSessionEndpointUris(AuthorizationServerConstants.EndSessionEndpoint);

                    options.AllowAuthorizationCodeFlow();
                    options.RequireProofKeyForCodeExchange();
                    options.DisableScopeValidation();
                    options.DisableResourceValidation();
                    
                    options.Configure(options =>
                    {
                        options.CodeChallengeMethods.Remove(
                            OpenIddictConstants.CodeChallengeMethods.Plain);

                        options.ClientAuthenticationMethods.Clear();

                        options.ClientAuthenticationMethods.Add(
                            OpenIddictConstants.ClientAuthenticationMethods.None);

                        options.ClientAuthenticationMethods.Add(
                            OpenIddictConstants.ClientAuthenticationMethods.ClientSecretPost);
                    });

                    if (settings.Issuer != null)
                        options.SetIssuer(settings.Issuer);

                    if (settings.UseDevelopmentCertificates)
                    {
                        options.AddDevelopmentEncryptionCertificate()
                               .AddDevelopmentSigningCertificate();
                    }

                    if (settings.DisableAccessTokenEncryption)
                        options.DisableAccessTokenEncryption();

                    // UserAdmin remains the source of truth for client-specific policy. OpenIddict
                    // now performs its normal application/token processing, while these handlers
                    // retain the additional UserAdmin grant/scope/resource/redirect validations.
                    options.AddEventHandler<ValidateAuthorizationRequestContext>(builder =>
                        builder.UseScopedHandler<OAuthAuthorizationRequestValidationHandler>()
                               .SetOrder(Int32.MaxValue - 100_000));

                    options.AddEventHandler<ValidateTokenRequestContext>(builder =>
                        builder.UseScopedHandler<OAuthTokenRequestValidationHandler>()
                               .SetOrder(Int32.MaxValue - 100_000));

                    // IdentityModel reconstructs a one-value JSON array from the authorization code
                    // as a scalar string claim. Restore the DOKS team_role array before OpenIddict
                    // clones the principal into SecurityTokenDescriptor.Subject. Mutating the principal
                    // after AttachTokenSubject is too late because the final JWT serializes that clone.
                    options.AddEventHandler<GenerateTokenContext>(builder =>
                        builder.UseSingletonHandler<OidcTeamRoleArrayTokenHandler>()
                               .SetOrder(OpenIddictServerHandlers.Protection.AttachTokenSubject.Descriptor.Order - 100));

                    options.UseAspNetCore()
                           .EnableStatusCodePagesIntegration()
                           .EnableAuthorizationEndpointPassthrough()
                           .EnableEndSessionEndpointPassthrough()
                           .EnableUserInfoEndpointPassthrough();
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
