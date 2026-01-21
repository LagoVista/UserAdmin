// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: passkey-dtos-v1
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using Newtonsoft.Json.Linq;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    /// <summary>
    /// Payload returned by Begin*Options endpoints and echoed back by the client
    /// when completing a passkey ceremony.
    /// </summary>
    public class PasskeyBeginOptionsResponse
    {
        /// <summary>
        /// Server-issued challenge identifier (opaque to client).
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// WebAuthn options object returned by the server and passed directly
        /// to navigator.credentials.create() or navigator.credentials.get().
        /// </summary>
        public JToken Options { get; set; }
    }

    /// <summary>
    /// Completion payload for passkey registration (attestation).
    /// </summary>
    public class PasskeyRegistrationCompleteRequest
    {
        /// <summary>
        /// Challenge identifier returned from BeginRegistrationOptions.
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// Raw AuthenticatorAttestationResponse JSON produced by the browser.
        /// This is treated as an opaque JSON blob by the server.
        /// </summary>
        public string AttestationJson { get; set; }
    }

    /// <summary>
    /// Completion payload for passkey authentication (assertion).
    /// </summary>
    public class PasskeyAuthenticationCompleteRequest
    {
        /// <summary>
        /// Challenge identifier returned from BeginAuthenticationOptions.
        /// </summary>
        public string ChallengeId { get; set; }

        /// <summary>
        /// Raw AuthenticatorAssertionResponse JSON produced by the browser.
        /// This is treated as an opaque JSON blob by the server.
        /// </summary>
        public string AssertionJson { get; set; }
    }

    /// <summary>
    /// Request to rename a registered passkey credential.
    /// </summary>
    public class RenamePasskeyRequest
    {
        /// <summary>
        /// New display name for the passkey.
        /// </summary>
        public string Name { get; set; }
    }
}
