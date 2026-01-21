using Fido2NetLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    public class PasskeyRegistrationCompleteRequestWire
    {
        [JsonProperty("challengeId", Required = Required.Always)]
        public string ChallengeId { get; set; }

        [JsonProperty("attestation", Required = Required.Always)]
        public WebAuthnAttestationWire Attestation { get; set; }
    }

    public class PasskeyAuthenticationCompleteRequestWire
    {
        [JsonProperty("challengeId", Required = Required.Always)]
        public string ChallengeId { get; set; }

        [JsonProperty("assertion", Required = Required.Always)]
        public WebAuthnAssertionWire Assertion { get; set; }
    }

    public sealed class WebAuthnAttestationWire
    {
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }                 // base64url string from browser

        [JsonProperty("rawId", Required = Required.Always)]
        public string RawId { get; set; }              // base64url string from browser

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }               // "public-key"

        [JsonProperty("response", Required = Required.Always)]
        public WebAuthnAttestationResponseWire Response { get; set; }

        [JsonProperty("clientExtensionResults", NullValueHandling = NullValueHandling.Ignore)]
        public JObject ClientExtensionResults { get; set; }
    }

    public sealed class WebAuthnAttestationResponseWire
    {
        [JsonProperty("clientDataJSON", Required = Required.Always)]
        public string ClientDataJSON { get; set; }     // base64url string

        [JsonProperty("attestationObject", Required = Required.Always)]
        public string AttestationObject { get; set; }  // base64url string
    }

    public sealed class WebAuthnAssertionWire
    {
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }              // base64url string (client supplied)

        [JsonProperty("rawId", Required = Required.Always)]
        public string RawId { get; set; }           // base64url string

        [JsonProperty("type", Required = Required.Always)]
        public string Type { get; set; }            // "public-key"

        [JsonProperty("response", Required = Required.Always)]
        public WebAuthnAssertionResponseWire Response { get; set; }

        [JsonProperty("clientExtensionResults", NullValueHandling = NullValueHandling.Ignore)]
        public object ClientExtensionResults { get; set; }
    }

    public sealed class WebAuthnAssertionResponseWire
    {
        [JsonProperty("authenticatorData", Required = Required.Always)]
        public string AuthenticatorData { get; set; }   // base64url string

        [JsonProperty("signature", Required = Required.Always)]
        public string Signature { get; set; }           // base64url string

        [JsonProperty("clientDataJSON", Required = Required.Always)]
        public string ClientDataJSON { get; set; }      // base64url string

        [JsonProperty("userHandle", NullValueHandling = NullValueHandling.Ignore)]
        public string UserHandle { get; set; }          // base64url string or null
    }

    public class PasskeyBeginOptionsResponse
    {
        [JsonProperty("challengeId", Required = Required.Always)]
        public string ChallengeId { get; set; }

        // Keep as JToken because options types differ between registration/assertion,
        // and you don't need to deserialize on server during "begin".
        [JsonProperty("options", Required = Required.Always)]
        public JToken Options { get; set; }

        // Optional convenience for client debugging/telemetry, but not required
        [JsonProperty("rpId", NullValueHandling = NullValueHandling.Ignore)]
        public string RpId { get; set; }

        [JsonProperty("origin", NullValueHandling = NullValueHandling.Ignore)]
        public string Origin { get; set; }
    }

    public sealed class PasskeyRegistrationCompleteRequest
    {
        [JsonProperty("challengeId", Required = Required.Always)]
        public string ChallengeId { get; set; }

        // ✅ Wire shape (base64url strings), not Fido2 type
        [JsonProperty("attestation", Required = Required.Always)]
        public WebAuthnAttestationWire Attestation { get; set; }
    }


    public class PasskeyAuthenticationCompleteRequest
    {
        [JsonProperty("challengeId", Required = Required.Always)]
        public string ChallengeId { get; set; }

        [JsonProperty("assertion", Required = Required.Always)]
        public WebAuthnAssertionWire Assertion { get; set; }
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
