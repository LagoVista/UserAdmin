// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 22e3b06a5d4cb4287e6ea2f5646f568cd40405bb0d65b1f79d773f571f7704d0
// IndexVersion: 2
// --- END CODE INDEX META ---
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace LagoVista.AspNetCore.Identity.Models
{
    public sealed record AuthFailureRecord
    {
        public DateTimeOffset Timestamp { get; init; }
        public int StatusCode { get; init; }
        public string Method { get; init; } = default!;
        public string Path { get; init; } = default!;
        public string QueryString { get; init; }
        public string UserId { get; init; }
        public string UserName { get; init; }
        public string?RemoteIp { get; init; }
        public string UserAgent { get; init; }
        public string[] Schemes { get; init; }
        public string[] RequiredPolicies { get; init; }
        public string[] RequiredRoles { get; init; }
        public string[] RequiredAuthTypes { get; init; }
        public string[] FailedRequirementTypes { get; init; }
        public string[] FailedRequirementDetails { get; init; } // human-readable
        public string[] LikelyFailingPolicies { get; init; }    // pol

        public static AuthFailureRecord FromContext(HttpContext ctx)
        {
            var endpoint = ctx.GetEndpoint();
            var authorizeData = endpoint?.Metadata.GetOrderedMetadata<IAuthorizeData>() ?? [];

            // Pull hints from endpoint metadata
            var requiredPolicies = authorizeData.Select(a => a.Policy)
                                                .Where(p => !string.IsNullOrWhiteSpace(p))
                                                .Distinct()
                                                .ToArray();
            var requiredRoles = authorizeData.Select(a => a.Roles)
                                             .Where(r => !string.IsNullOrWhiteSpace(r))
                                             .SelectMany(r => r!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                                             .Distinct()
                                             .ToArray();
            var requiredAuthTypes = authorizeData.Select(a => a.AuthenticationSchemes)
                                                 .Where(s => !string.IsNullOrWhiteSpace(s))
                                                 .SelectMany(s => s!.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                                                 .Distinct()
                                                 .ToArray();

            // Try to infer scheme(s) actually used (best effort)
            var schemes = ctx.Items.TryGetValue("__AuthSchemes", out var used) && used is string[] arr ? arr : requiredAuthTypes;

            var user = ctx.User;
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? user?.FindFirst("sub")?.Value;

            ctx.Request.Headers.TryGetValue("User-Agent", out StringValues ua);

            return new AuthFailureRecord
            {
                Timestamp = DateTimeOffset.UtcNow,
                StatusCode = ctx.Response.StatusCode,
                Method = ctx.Request.Method,
                Path = ctx.Request.Path.Value ?? "/",
                QueryString = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value : null,
                UserId = string.IsNullOrWhiteSpace(userId) ? null : userId,
                UserName = user?.Identity?.IsAuthenticated == true ? user.Identity?.Name : null,
                RemoteIp = ctx.Connection.RemoteIpAddress?.ToString(),
                UserAgent = ua.ToString(),
                Schemes = schemes?.Length > 0 ? schemes : null,
                RequiredPolicies = requiredPolicies.Length > 0 ? requiredPolicies : null,
                RequiredRoles = requiredRoles.Length > 0 ? requiredRoles : null,
                RequiredAuthTypes = requiredAuthTypes.Length > 0 ? requiredAuthTypes : null
            };
        }
    }
}
