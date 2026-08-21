using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Auth;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.AuthorizationServer.Persistence.UserAdmin
{
    /// <summary>
    /// Read-through OpenIddict application store backed by UserAdmin's OAuthClientApplication model.
    ///
    /// OAuthClientApplication remains the authoritative client configuration. OpenIddict's internal
    /// application identifier is deliberately the ClientId, which lets protocol/token operations
    /// resolve a client without requiring organization/user context.
    ///
    /// Administrative writes must continue through IOAuthClientApplicationManager, so the mutation
    /// members of the OpenIddict store are intentionally not supported.
    /// </summary>
    public class OpenIddictOAuthClientApplicationStore : IOpenIddictApplicationStore<OAuthClientApplication>
    {
        private readonly IOAuthClientApplicationManager _manager;

        public OpenIddictOAuthClientApplicationStore(IOAuthClientApplicationManager manager)
        {
            _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        }

        public ValueTask<long> CountAsync(CancellationToken cancellationToken)
            => throw ReadOnlyQueryNotSupported();

        public ValueTask<long> CountAsync<TResult>(Func<IQueryable<OAuthClientApplication>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => throw ReadOnlyQueryNotSupported();

        public ValueTask CreateAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask DeleteAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask<OAuthClientApplication> FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => FindByClientIdentifierAsync(identifier, cancellationToken);

        public ValueTask<OAuthClientApplication> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
            => FindByClientIdentifierAsync(identifier, cancellationToken);

        public async IAsyncEnumerable<OAuthClientApplication> FindByPostLogoutRedirectUriAsync(
            string uri, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(uri))
                throw new ArgumentNullException(nameof(uri));

            cancellationToken.ThrowIfCancellationRequested();
            var applications = await _manager.GetOAuthClientApplicationsByPostLogoutRedirectUriAsync(uri);

            foreach (var application in applications)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return application;
            }
        }

        public IAsyncEnumerable<OAuthClientApplication> FindByRedirectUriAsync(string uri, CancellationToken cancellationToken)
            => throw ReadOnlyQueryNotSupported();

        public ValueTask<string> GetApplicationTypeAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application, OpenIddictConstants.ApplicationTypes.Web, cancellationToken);

        public ValueTask<TResult> GetAsync<TState, TResult>(
            Func<IQueryable<OAuthClientApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw ReadOnlyQueryNotSupported();

        public ValueTask<string> GetClientIdAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application, application.ClientId, cancellationToken);

        public ValueTask<string> GetClientSecretAsync(OAuthClientApplication application, CancellationToken cancellationToken)
        {
            Validate(application, cancellationToken);

            // Public PKCE clients are the supported production slice today. Confidential-client
            // secret resolution will be wired through ISecureStorage when that slice is enabled.
            return new ValueTask<string>((string)null);
        }

        public ValueTask<string> GetClientTypeAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application,
                application.ClientType?.Id == OAuthClientApplication.ClientType_Confidential
                    ? OpenIddictConstants.ClientTypes.Confidential
                    : OpenIddictConstants.ClientTypes.Public,
                cancellationToken);

        public ValueTask<string> GetConsentTypeAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application,
                application.RequireConsent
                    ? OpenIddictConstants.ConsentTypes.Explicit
                    : OpenIddictConstants.ConsentTypes.Implicit,
                cancellationToken);

        public ValueTask<string> GetDisplayNameAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application, application.Name, cancellationToken);

        public ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application, ImmutableDictionary<CultureInfo, string>.Empty, cancellationToken);

        public ValueTask<string> GetIdAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application, application.ClientId, cancellationToken);

        public ValueTask<JsonWebKeySet> GetJsonWebKeySetAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value<JsonWebKeySet>(application, null, cancellationToken);

        public ValueTask<ImmutableArray<string>> GetPermissionsAsync(OAuthClientApplication application, CancellationToken cancellationToken)
        {
            Validate(application, cancellationToken);

            var permissions = ImmutableArray.CreateBuilder<string>();

            // The authorization-code slice requires the interactive endpoints, token endpoint,
            // authorization-code grant and code response type. End-session is part of the same
            // browser-facing OIDC client capability and does not revoke already-issued tokens.
            if (Values(application.AllowedGrantTypes).Contains(OpenIddictConstants.GrantTypes.AuthorizationCode, StringComparer.Ordinal))
            {
                permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
                permissions.Add(OpenIddictConstants.Permissions.Endpoints.EndSession);
                permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
                permissions.Add(OpenIddictConstants.Permissions.ResponseTypes.Code);
            }

            foreach (var grantType in Values(application.AllowedGrantTypes))
            {
                if (!String.Equals(grantType, OpenIddictConstants.GrantTypes.AuthorizationCode, StringComparison.Ordinal))
                    permissions.Add(OpenIddictConstants.Permissions.Prefixes.GrantType + grantType);
            }

            foreach (var scope in Values(application.AllowedScopes))
            {
                if (!String.Equals(scope, OpenIddictConstants.Scopes.OpenId, StringComparison.Ordinal) &&
                    !String.Equals(scope, OpenIddictConstants.Scopes.OfflineAccess, StringComparison.Ordinal))
                    permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
            }

            foreach (var resource in Values(application.AllowedResources))
                permissions.Add(OpenIddictConstants.Permissions.Prefixes.Resource + resource);

            return new ValueTask<ImmutableArray<string>>(permissions.Distinct(StringComparer.Ordinal).ToImmutableArray());
        }

        public ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application, Values(application.PostLogoutRedirectUris), cancellationToken);

        public ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application, ImmutableDictionary<string, JsonElement>.Empty, cancellationToken);

        public ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application, Values(application.RedirectUris), cancellationToken);

        public ValueTask<ImmutableArray<string>> GetRequirementsAsync(OAuthClientApplication application, CancellationToken cancellationToken)
        {
            Validate(application, cancellationToken);

            return Value(application,
                application.RequirePkce
                    ? ImmutableArray.Create(OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange)
                    : ImmutableArray<string>.Empty,
                cancellationToken);
        }

        public ValueTask<ImmutableDictionary<string, string>> GetSettingsAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => Value(application, ImmutableDictionary<string, string>.Empty, cancellationToken);

        public ValueTask<OAuthClientApplication> InstantiateAsync(CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public IAsyncEnumerable<OAuthClientApplication> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => throw ReadOnlyQueryNotSupported();

        public IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
            Func<IQueryable<OAuthClientApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw ReadOnlyQueryNotSupported();

        public ValueTask SetApplicationTypeAsync(OAuthClientApplication application, string type, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetClientIdAsync(OAuthClientApplication application, string identifier, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetClientSecretAsync(OAuthClientApplication application, string secret, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetClientTypeAsync(OAuthClientApplication application, string type, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetConsentTypeAsync(OAuthClientApplication application, string type, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetDisplayNameAsync(OAuthClientApplication application, string name, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetDisplayNamesAsync(OAuthClientApplication application, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetJsonWebKeySetAsync(OAuthClientApplication application, JsonWebKeySet set, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetPermissionsAsync(OAuthClientApplication application, ImmutableArray<string> permissions, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetPostLogoutRedirectUrisAsync(OAuthClientApplication application, ImmutableArray<string> uris, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetPropertiesAsync(OAuthClientApplication application, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetRedirectUrisAsync(OAuthClientApplication application, ImmutableArray<string> uris, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetRequirementsAsync(OAuthClientApplication application, ImmutableArray<string> requirements, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask SetSettingsAsync(OAuthClientApplication application, ImmutableDictionary<string, string> settings, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        public ValueTask UpdateAsync(OAuthClientApplication application, CancellationToken cancellationToken)
            => throw ReadOnlyMutationNotSupported();

        private async ValueTask<OAuthClientApplication> FindByClientIdentifierAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrWhiteSpace(identifier))
                throw new ArgumentNullException(nameof(identifier));

            cancellationToken.ThrowIfCancellationRequested();
            var application = await _manager.GetOAuthClientApplicationByClientIdAsync(identifier);
            cancellationToken.ThrowIfCancellationRequested();
            return application;
        }

        private static ImmutableArray<string> Values(IEnumerable<OAuthClientSettingValue> values)
            => values?
                .Where(value => value != null && !String.IsNullOrWhiteSpace(value.Value))
                .Select(value => value.Value)
                .Distinct(StringComparer.Ordinal)
                .ToImmutableArray() ?? ImmutableArray<string>.Empty;

        private static void Validate(OAuthClientApplication application, CancellationToken cancellationToken)
        {
            if (application == null) throw new ArgumentNullException(nameof(application));
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static ValueTask<T> Value<T>(OAuthClientApplication application, T value, CancellationToken cancellationToken)
        {
            Validate(application, cancellationToken);
            return new ValueTask<T>(value);
        }

        private static NotSupportedException ReadOnlyMutationNotSupported()
            => new NotSupportedException("OpenIddict client configuration is read-only through this store. Use IOAuthClientApplicationManager for administrative changes.");

        private static NotSupportedException ReadOnlyQueryNotSupported()
            => new NotSupportedException("This OpenIddict application-store query is not required by the authorization-server runtime and is intentionally not exposed without UserAdmin organization context.");
    }
}
