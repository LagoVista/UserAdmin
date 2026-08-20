using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using System;

namespace LagoVista.AspNetCore.Identity.Services
{
    public sealed class PasskeyRelyingPartyContext : IPasskeyRelyingPartyContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAppConfig _appConfig;
        private PasskeyRelyingParty _current;

        public PasskeyRelyingPartyContext(IHttpContextAccessor httpContextAccessor, IAppConfig appConfig)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        public PasskeyRelyingParty Current => _current ??= Resolve();

        private PasskeyRelyingParty Resolve()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null && request.Host.HasValue && !String.IsNullOrWhiteSpace(request.Scheme))
            {
                var requestOrigin = NormalizeOrigin(request.Scheme, request.Host.Value);
                if (requestOrigin != null)
                    return new PasskeyRelyingParty(requestOrigin.IdnHost.ToLowerInvariant(), requestOrigin.GetLeftPart(UriPartial.Authority), true);
            }

            if (String.IsNullOrWhiteSpace(_appConfig.WebAddress))
                throw new InvalidOperationException("IAppConfig.WebAddress is required when no HTTP request is available for Passkey relying-party resolution.");

            if (!Uri.TryCreate(_appConfig.WebAddress, UriKind.Absolute, out var configuredOrigin))
                throw new InvalidOperationException($"IAppConfig.WebAddress '{_appConfig.WebAddress}' is not a valid absolute URI for Passkey relying-party resolution.");

            return new PasskeyRelyingParty(
                configuredOrigin.IdnHost.ToLowerInvariant(),
                configuredOrigin.GetLeftPart(UriPartial.Authority),
                false);
        }

        private static Uri NormalizeOrigin(string scheme, string host)
        {
            if (!String.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) &&
                !String.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
                return null;

            if (!Uri.TryCreate($"{scheme}://{host}", UriKind.Absolute, out var origin))
                return null;

            return origin;
        }
    }
}
