using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using System;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    public enum PasskeyChallengePurpose
    {
        Register,
        Authenticate,
    }

    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.PasskeyChallenge_Name,
        UserAdminResources.Names.PasskeyChallenge_Help,
        UserAdminResources.Names.PasskeyChallenge_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class PasskeyChallenge
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        public string UserId { get; set; }
        public string RpId { get; set; }
        public string Origin { get; set; }

        // Relative URL starting with /auth. Used for InvokeResult.SuccessRedirect after completion.
        public string PasskeyUrl { get; set; }

        public PasskeyChallengePurpose Purpose { get; set; }

        // base64url
        public string Challenge { get; set; }

        public string CreatedUtc { get; set; }
        public string ExpiresUtc { get; set; }

        public string[] AllowCredentialIds { get; set; } = Array.Empty<string>();

        // Store what you sent in options so server stays authoritative
        public int? UserVerification { get; set; } // map to UserVerificationRequirement
        public int? TimeoutMs { get; set; }

        // For Register: prevent duplicates (base64url)
        public string[] ExcludeCredentialIds { get; set; } = Array.Empty<string>();

        [JsonIgnore]
        public bool IsExpired
        {
            get
            {
                if (String.IsNullOrEmpty(ExpiresUtc)) return true;
                if (!DateTime.TryParse(ExpiresUtc, out var dt)) return true;
                return DateTime.UtcNow > dt.ToUniversalTime();
            }
        }
    }

    [EntityDescription(
        Domains.OrganizationDomain,
        UserAdminResources.Names.PasskeyChallengePacket_Name,
        UserAdminResources.Names.PasskeyChallengePacket_Help,
        UserAdminResources.Names.PasskeyChallengePacket_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class PasskeyChallengePacket
    {
        public PasskeyChallenge Challenge { get; set; }
        public string OptionsJson { get; set; }
    }
}
