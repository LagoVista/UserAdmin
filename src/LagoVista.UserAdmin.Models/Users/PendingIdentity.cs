using LagoVista.Core.Attributes;
using LagoVista.Core.Authentication.Models;
using LagoVista.UserAdmin.Models.Resources;
using System;
using LagoVista.UserAdmin.Models;

namespace Security.Models
{
    /// <summary>
    /// Provisional authentication container ("airlock") used to resolve an identity before creating/linking
    /// a durable user account. This object is authenticated for finalize/link flows only and has no app
    /// authorization by itself.
    ///
    /// Invariant: exactly one auth bucket is populated, matching <see cref="FlowType"/>.
    /// Auth buckets: Password, NativeProvider, OAuthExternalLogin, Passkey.
    /// Optional attachment: Invite context.
    /// </summary>
    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.PendingIdentity_Name,
        UserAdminResources.Names.PendingIdentity_Help,
        UserAdminResources.Names.PendingIdentity_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class PendingIdentity
    {
        // -----------------------------
        // Core
        // -----------------------------

        public Guid Id { get; set; }

        public PendingIdentityFlowType FlowType { get; set; }

        public PendingIdentityStatus Status { get; set; }

        public DateTime CreatedAtUtc { get; set; }

        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// Useful for tracing a single auth attempt across client/server logs.
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// The resolved durable user id this PendingIdentity should be linked to (when known).
        /// </summary>
        public Guid? ResolutionTargetUserId { get; set; }

        /// <summary>
        /// Reason code for create/link decisions and/or terminal failures.
        /// </summary>
        public string ResolutionReasonCode { get; set; }

        public string ErrorCode { get; set; }

        public string LastStep { get; set; }

        // -----------------------------
        // Profile (untrusted until verified)
        // -----------------------------

        public string ProposedFirstName { get; set; }

        public string ProposedLastName { get; set; }

        /// <summary>
        /// Email provided by the user or prefilled from a provider. Not trusted until verified.
        /// </summary>
        public string ProposedEmail { get; set; }

        /// <summary>
        /// Verified email captured during the PendingIdentity flow (OTP/magic link).
        /// </summary>
        public string VerifiedEmail { get; set; }

        public DateTime? VerifiedEmailAtUtc { get; set; }

        public int OtpSendCount { get; set; }

        public int OtpVerifyFailCount { get; set; }

        public DateTime? LastOtpSentAtUtc { get; set; }

        // -----------------------------
        // Auth bucket: Native Provider (Apple/Google)
        // -----------------------------

        public string NativeProvider { get; set; } // e.g. "apple", "google"

        /// <summary>
        /// Provider subject identifier (stable per provider/app). This is the primary provider key.
        /// </summary>
        public string NativeProviderSubjectId { get; set; }

        /// <summary>
        /// Hash of relevant provider claims snapshot for troubleshooting/auditing without storing raw tokens.
        /// </summary>
        public string NativeProviderClaimsHash { get; set; }

        public string NativeProviderEmail { get; set; }

        public bool? NativeProviderEmailVerifiedFlag { get; set; }

        public bool? NativeProviderEmailIsRelay { get; set; }

        // -----------------------------
        // Auth bucket: OAuth External Login
        // -----------------------------

        public string OAuthProvider { get; set; } // e.g. "github", "microsoft", etc.

        public string OAuthIssuer { get; set; }

        public string OAuthSubject { get; set; }

        public string OAuthScopes { get; set; }

        public DateTime? OAuthAuthTimeUtc { get; set; }

        public string OAuthClaimsHash { get; set; }

        // -----------------------------
        // Auth bucket: Passkey (future)
        // -----------------------------

        public string PasskeyCredentialId { get; set; }

        public string PasskeyPublicKey { get; set; }

        public string PasskeyRpId { get; set; }

        public string PasskeyAaguid { get; set; }

        public long? PasskeySignCount { get; set; }

        public string PasskeyAttestationFormat { get; set; }

        // -----------------------------
        // Auth bucket: Password
        // -----------------------------

        /// <summary>
        /// During password-based signup, do not store raw password. Store a hash or a secure reference.
        /// </summary>
        public string PasswordHash { get; set; }

        public string PasswordHashAlgorithm { get; set; }

        /// <summary>
        /// Optional reference to a secure secret store if you choose not to persist the hash here.
        /// </summary>
        public string PasswordSecretId { get; set; }

        // -----------------------------
        // Optional attachment: Invite context
        // -----------------------------

        public string InviteId { get; set; }

        public string InviteType { get; set; } // e.g. "org", "customer", "project"

        public string InvitedOrgId { get; set; }

        public string InvitedCustomerId { get; set; }

        public string InvitedEmail { get; set; }

        public DateTime? InviteIssuedAtUtc { get; set; }

        public DateTime? InviteExpiresAtUtc { get; set; }

        /// <summary>
        /// Hash/signature reference for validating the invite token without storing sensitive raw data.
        /// </summary>
        public string InviteProofHash { get; set; }
    }

    public enum PendingIdentityFlowType
    {
        Unknown = 0,
        Password = 1,
        NativeProvider = 2,
        OAuthExternalLogin = 3,
        Passkey = 4
    }

    public enum PendingIdentityStatus
    {
        Unknown = 0,
        Pending = 1,
        EmailVerificationRequired = 2,
        VerifyingEmail = 3,
        Resolved = 4,
        Expired = 5,
        Canceled = 6,
        Failed = 7
    }
}
