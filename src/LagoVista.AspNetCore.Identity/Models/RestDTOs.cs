using Fido2NetLib;
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LagoVista.UserAdmin.Models.Auth.Passkeys
{
    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.PasskeyRegistrationCompleteRequestWire_Name,
        UserAdminResources.Names.PasskeyRegistrationCompleteRequestWire_Help,
        UserAdminResources.Names.PasskeyRegistrationCompleteRequestWire_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class PasskeyRegistrationCompleteRequestWire
    {
        [JsonProperty("challengeId", Required = Required.Always)]
        public string ChallengeId { get; set; }

        [JsonProperty("attestation", Required = Required.Always)]
        public WebAuthnAttestationWire Attestation { get; set; }
    }

    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.PasskeyAuthenticationCompleteRequestWire_Name,
        UserAdminResources.Names.PasskeyAuthenticationCompleteRequestWire_Help,
        UserAdminResources.Names.PasskeyAuthenticationCompleteRequestWire_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class PasskeyAuthenticationCompleteRequestWire
    {
        [JsonProperty("challengeId", Required = Required.Always)]
        public string ChallengeId { get; set; }

        [JsonProperty("assertion", Required = Required.Always)]
        public WebAuthnAssertionWire Assertion { get; set; }
    }

    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.WebAuthnAttestationWire_Name,
        UserAdminResources.Names.WebAuthnAttestationWire_Help,
        UserAdminResources.Names.WebAuthnAttestationWire_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
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

    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.WebAuthnAttestationResponseWire_Name,
        UserAdminResources.Names.WebAuthnAttestationResponseWire_Help,
        UserAdminResources.Names.WebAuthnAttestationResponseWire_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public sealed class WebAuthnAttestationResponseWire
    {
        [JsonProperty("clientDataJSON", Required = Required.Always)]
        public string ClientDataJSON { get; set; }     // base64url string

        [JsonProperty("attestationObject", Required = Required.Always)]
        public string AttestationObject { get; set; }  // base64url string
    }

    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.WebAuthnAssertionWire_Name,
        UserAdminResources.Names.WebAuthnAssertionWire_Help,
        UserAdminResources.Names.WebAuthnAssertionWire_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
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

    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.WebAuthnAssertionResponseWire_Name,
        UserAdminResources.Names.WebAuthnAssertionResponseWire_Help,
        UserAdminResources.Names.WebAuthnAssertionResponseWire_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
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

    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.PasskeyBeginOptionsResponse_Name,
        UserAdminResources.Names.PasskeyBeginOptionsResponse_Help,
        UserAdminResources.Names.PasskeyBeginOptionsResponse_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
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

    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.PasskeyRegistrationCompleteRequest_Name,
        UserAdminResources.Names.PasskeyRegistrationCompleteRequest_Help,
        UserAdminResources.Names.PasskeyRegistrationCompleteRequest_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public sealed class PasskeyRegistrationCompleteRequest
    {
        [JsonProperty("challengeId", Required = Required.Always)]
        public string ChallengeId { get; set; }

        // ✅ Wire shape (base64url strings), not Fido2 type
        [JsonProperty("attestation", Required = Required.Always)]
        public WebAuthnAttestationWire Attestation { get; set; }
    }

    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.PasskeyAuthenticationCompleteRequest_Name,
        UserAdminResources.Names.PasskeyAuthenticationCompleteRequest_Help,
        UserAdminResources.Names.PasskeyAuthenticationCompleteRequest_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
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
    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.RenamePasskeyRequest_Name,
        UserAdminResources.Names.RenamePasskeyRequest_Help,
        UserAdminResources.Names.RenamePasskeyRequest_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class RenamePasskeyRequest
    {
        /// <summary>
        /// New display name for the passkey.
        /// </summary>
        public string Name { get; set; }
    }
}
