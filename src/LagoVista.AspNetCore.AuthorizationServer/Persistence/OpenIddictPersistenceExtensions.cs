using LagoVista.AspNetCore.AuthorizationServer.Persistence.Cassandra;
using LagoVista.AspNetCore.AuthorizationServer.Persistence.UserAdmin;
using LagoVista.UserAdmin.Models.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace LagoVista.AspNetCore.AuthorizationServer.Persistence
{
    public static class OpenIddictPersistenceExtensions
    {
        /// <summary>
        /// Registers the LagoVista-owned OpenIddict persistence boundary:
        /// UserAdmin remains authoritative for OAuth client configuration while
        /// OpenIddict protocol tokens/codes are persisted in Cassandra with bounded retention.
        /// </summary>
        public static OpenIddictBuilder AddLagoVistaPersistence(this OpenIddictBuilder builder)
        {
            builder.AddCore(options =>
            {
                options.SetDefaultApplicationEntity<OAuthClientApplication>();
                options.ReplaceApplicationStore<OAuthClientApplication, OpenIddictOAuthClientApplicationStore>();
                options.ReplaceApplicationManager<OAuthClientApplication, LagoVistaOpenIddictApplicationManager>();

                options.SetDefaultTokenEntity<OpenIddictProtocolToken>();
                options.ReplaceTokenStore<OpenIddictProtocolToken, OpenIddictCassandraTokenStore>();
            });

            return builder;
        }
    }
}
