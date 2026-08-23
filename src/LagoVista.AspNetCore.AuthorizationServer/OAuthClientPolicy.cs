using System;
using System.Collections.Generic;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public class OAuthClientPolicy
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string Name { get; set; }
        public string ClientType { get; set; }
        public string Status { get; set; }
        public bool RequirePkce { get; set; }
        public bool RequireConsent { get; set; }
        public string ClientSecretId { get; set; }
        public int? AccessTokenLifetimeMinutes { get; set; }
        public int? IdentityTokenLifetimeMinutes { get; set; }
        public int? RefreshTokenLifetimeDays { get; set; }
        public IReadOnlyCollection<string> RedirectUris { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> PostLogoutRedirectUris { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> AllowedGrantTypes { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> AllowedScopes { get; set; } = Array.Empty<string>();
        public IReadOnlyCollection<string> AllowedResources { get; set; } = Array.Empty<string>();

        public bool IsActive => String.Equals(Status, "active", StringComparison.OrdinalIgnoreCase);
        public bool IsPublicClient => String.Equals(ClientType, "public", StringComparison.OrdinalIgnoreCase);
    }
}
