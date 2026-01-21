using Fido2NetLib.Objects;
using Fido2NetLib;
using System;
using LagoVista.UserAdmin.Models.Security.Passkeys;

namespace LagoVista.AspNetCore.Identity.Utils
{
    internal static class WebAuthnWireMapper
    {
        public static AuthenticatorAttestationRawResponse ToAttestationRawResponse(LagoVista.UserAdmin.Models.Security.Passkeys.WebAuthnAttestationWire wire)
        {
            if (wire == null) throw new ArgumentNullException(nameof(wire));
            if (wire.Response == null) throw new ArgumentNullException(nameof(wire.Response));

            // Decode bytes deterministically (server-owned normalization)
            var rawIdBytes = Base64UrlDecode(wire.RawId);

            // Per Fido2 v4 shape: Id is Base64UrlEncoding of RawId (string), so recompute it.
            var normalizedId = Base64UrlEncode(rawIdBytes);
           
            return new AuthenticatorAttestationRawResponse
            {
                Id = normalizedId,
                RawId = rawIdBytes,
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAttestationRawResponse.AttestationResponse
                {
                    ClientDataJson = Base64UrlDecode(wire.Response.ClientDataJSON),
                    AttestationObject = Base64UrlDecode(wire.Response.AttestationObject)
                }
            };
        }


        public static AuthenticatorAssertionRawResponse ToFido2Assertion(WebAuthnAssertionWire wire)
        {
            if (wire == null) throw new ArgumentNullException(nameof(wire));
            if (wire.Response == null) throw new ArgumentNullException(nameof(wire.Response));

            // Decode bytes deterministically (your existing helper)
            var rawIdBytes = Base64UrlDecode(wire.RawId);

            // Optional: sanity check client consistency (id should be b64url(rawId))
            // If you keep it, normalize by recomputing Id and ignore wire.Id
            var normalizedId = Base64UrlEncode(rawIdBytes);

            return new AuthenticatorAssertionRawResponse
            {
                RawId = rawIdBytes,
                Id = normalizedId, // do NOT trust client wire.Id; recompute
                Type = PublicKeyCredentialType.PublicKey,
                Response = new AuthenticatorAssertionRawResponse.AssertionResponse
                {
                    AuthenticatorData = Base64UrlDecode(wire.Response.AuthenticatorData),
                    Signature = Base64UrlDecode(wire.Response.Signature),
                    ClientDataJson = Base64UrlDecode(wire.Response.ClientDataJSON),
                    UserHandle = String.IsNullOrWhiteSpace(wire.Response.UserHandle) ? null : Base64UrlDecode(wire.Response.UserHandle),
                }
            };
        }

        private static string Base64UrlEncode(byte[] bytes)
        {
            var s = Convert.ToBase64String(bytes);
            s = s.TrimEnd('=');
            s = s.Replace('+', '-');
            s = s.Replace('/', '_');
            return s;
        }

        private static byte[] Base64UrlDecode(string base64Url)
        {
            if (String.IsNullOrEmpty(base64Url)) return Array.Empty<byte>();
            var s = base64Url.Replace('-', '+').Replace('_', '/');
            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }
            return Convert.FromBase64String(s);
        }
    }
}
