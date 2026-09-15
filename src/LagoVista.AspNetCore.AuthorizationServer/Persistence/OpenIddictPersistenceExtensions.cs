using LagoVista.AspNetCore.AuthorizationServer.Persistence.TableStorage;
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
        /// OpenIddict protocol tokens/codes are persisted in shared Table Storage.
        /// </summary>
        public static OpenIddictBuilder AddLagoVistaPersistence(this OpenIddictBuilder builder, bool useCassandraTokenStore = false)
        {
            builder.AddCore(options =>
            {
                options.SetDefaultApplicationEntity<OAuthClientApplication>();
                options.ReplaceApplicationStore<OAuthClientApplication, OpenIddictOAuthClientApplicationStore>();
                options.ReplaceApplicationManager<OAuthClientApplication, LagoVistaOpenIddictApplicationManager>();

                options.SetDefaultTokenEntity<OpenIddictTableToken>();
                if (useCassandraTokenStore)
                    options.ReplaceTokenStore<OpenIddictTableToken, OpenIddictCassandraTokenStore>();
                else
                    options.ReplaceTokenStore<OpenIddictTableToken, OpenIddictTableTokenStore>();
            });

            return builder;
        }
    }
}
