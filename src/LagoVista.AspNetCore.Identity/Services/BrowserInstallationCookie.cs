using LagoVista.UserAdmin.Managers;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using System;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.AspNetCore.Identity.Services
{
    /// <summary>
    /// Origin-scoped browser continuity metadata. This is not a login cookie
    /// or an authentication credential. The host supplies the trusted origin
    /// from its own configuration, never directly from a request parameter.
    /// </summary>
    public sealed class BrowserInstallationCookie
    {
        private readonly IDataProtectionProvider _protection;
        public BrowserInstallationCookie(IDataProtectionProvider protection)
            => _protection = protection ?? throw new ArgumentNullException(nameof(protection));

        public string Read(HttpContext context, Uri trustedOrigin)
        {
            var origin = RequireOrigin(context, trustedOrigin);
            var name = Name(origin);
            if (context.Items.TryGetValue(name, out var value)) return value as string;
            if (!context.Request.Cookies.TryGetValue(name, out var protectedValue) ||
                String.IsNullOrWhiteSpace(protectedValue) || protectedValue.Length > 1024) return null;
            try
            {
                var decoded = Protector(origin).Unprotect(protectedValue);
                if (!Guid.TryParseExact(decoded, "N", out var id) || id == Guid.Empty) return null;
                var canonical = id.ToString("N").ToUpperInvariant();
                context.Items[name] = canonical;
                return canonical;
            }
            catch (CryptographicException) { return null; }
        }

        // Called by a trusted host during shared browser initialization, not
        // per tab or for a notification operation. Repeated reads in one
        // request use the same identifier even before the response is sent.
        public string Initialize(HttpContext context, Uri trustedOrigin)
        {
            var origin = RequireOrigin(context, trustedOrigin);
            var existing = Read(context, trustedOrigin);
            if (existing != null) return existing;
            if (context.Response.HasStarted)
                throw new InvalidOperationException("Browser initialization requires an unstarted response.");
            var id = BrowserInstallationManager.NewBrowserId();
            var name = Name(origin);
            context.Response.Cookies.Append(name, Protector(origin).Protect(id), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/",
                MaxAge = TimeSpan.FromDays(365),
                IsEssential = true
            });
            context.Items[name] = id;
            return id;
        }

        private IDataProtector Protector(string origin)
            => _protection.CreateProtector("UserAdmin.BrowserInstallation.v1", origin);

        private static string Name(string origin)
        {
            // Cookies are host-scoped rather than port-scoped. Separate names
            // and data-protection purposes avoid sharing identity across ports.
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(origin));
            return "__Host-nuviot-installation-" + Convert.ToHexString(hash.AsSpan(0, 12)).ToLowerInvariant();
        }

        private static string RequireOrigin(HttpContext context, Uri trustedOrigin)
        {
            if (context == null || trustedOrigin == null || !trustedOrigin.IsAbsoluteUri ||
                trustedOrigin.Scheme != Uri.UriSchemeHttps || trustedOrigin.AbsolutePath != "/" ||
                !String.IsNullOrEmpty(trustedOrigin.UserInfo) || !String.IsNullOrEmpty(trustedOrigin.Query) ||
                !String.IsNullOrEmpty(trustedOrigin.Fragment) || !context.Request.IsHttps ||
                !Uri.TryCreate(context.Request.Scheme + "://" + context.Request.Host.ToUriComponent(), UriKind.Absolute, out var requestOrigin) ||
                !String.Equals(trustedOrigin.GetLeftPart(UriPartial.Authority),
                    requestOrigin.GetLeftPart(UriPartial.Authority), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Browser installation origin is not trusted.");
            return trustedOrigin.GetLeftPart(UriPartial.Authority).ToLowerInvariant();
        }
    }
}
