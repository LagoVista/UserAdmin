using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Models.Auth;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class OAuthClientPolicyResolver : IOAuthClientPolicyResolver
    {
        private readonly IOAuthClientApplicationManager _clientManager;

        public OAuthClientPolicyResolver(IOAuthClientApplicationManager clientManager)
        {
            _clientManager = clientManager ?? throw new ArgumentNullException(nameof(clientManager));
        }

        public async Task<OAuthClientPolicy> GetByClientIdAsync(string clientId)
        {
            if (String.IsNullOrWhiteSpace(clientId))
                return null;

            var client = await _clientManager.GetOAuthClientApplicationByClientIdAsync(clientId);
            if (client == null)
                return null;

            return new OAuthClientPolicy
            {
                Id = client.Id,
                ClientId = client.ClientId,
                Name = client.Name,
                ClientType = client.ClientType?.Id,
                Status = client.Status?.Id,
                RequirePkce = client.RequirePkce,
                RequireConsent = client.RequireConsent,
                ClientSecretId = client.ClientSecretId,
                AccessTokenLifetimeMinutes = client.AccessTokenLifetimeMinutes,
                IdentityTokenLifetimeMinutes = client.IdentityTokenLifetimeMinutes,
                RefreshTokenLifetimeDays = client.RefreshTokenLifetimeDays,
                RedirectUris = GetValues(client.RedirectUris),
                PostLogoutRedirectUris = GetValues(client.PostLogoutRedirectUris),
                AllowedGrantTypes = GetValues(client.AllowedGrantTypes),
                AllowedScopes = GetValues(client.AllowedScopes),
                AllowedResources = GetValues(client.AllowedResources),
            };
        }

        private static string[] GetValues(System.Collections.Generic.IEnumerable<OAuthClientSettingValue> values)
        {
            return values?
                .Where(value => value != null && !String.IsNullOrWhiteSpace(value.Value))
                .Select(value => value.Value)
                .Distinct(StringComparer.Ordinal)
                .ToArray() ?? Array.Empty<string>();
        }
    }
}
