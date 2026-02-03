using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Auth
{
    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.MagicLinkAttempt_Title,
        UserAdminResources.Names.MagicLinkAttempt_Help,
        UserAdminResources.Names.MagicLinkAttempt_Description,
        EntityDescriptionAttribute.EntityTypes.Dto,
        typeof(UserAdminResources),
        ClusterKey: "auth",
        ModelType: EntityDescriptionAttribute.ModelTypes.DomainEntity,
        Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential,
        IndexInclude: true,
        IndexTier: EntityDescriptionAttribute.IndexTiers.Primary,
        IndexPriority: 95,
        IndexTagsCsv: "authdomain,magiclink,signin,domainentity")]
    public class MagicLinkAttempt
    {
        public const string Channel_Portal = "portal";
        public const string Channel_Mobile = "mobile";

        public const string Purpose_SignIn = "signin";

        /// <summary>
        /// Primary identifier for the attempt record.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Normalized email (lowercase + trimmed) used for user resolution.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Resolved user id for the email at request time (if known).
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Portal or Mobile (string constant for stability).
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// Fixed as SignIn for SEC-000005 (string constant for stability).
        /// </summary>
        public string Purpose { get; set; }

        /// <summary>
        /// Hash of the magic-link code (raw code is never persisted).
        /// </summary>
        public string CodeHash { get; set; }

        /// <summary>
        /// UTC expiry for the magic-link code.
        /// </summary>
        public DateTime ExpiresAtUtc { get; set; }

        /// <summary>
        /// UTC timestamp when the magic-link code was successfully consumed (single-use).
        /// </summary>
        public DateTime? ConsumedAtUtc { get; set; }

        // ----- Mobile exchange support (only used when Channel == mobile) -----

        /// <summary>
        /// Hash of short-lived exchange code used by mobile to mint JWT.
        /// </summary>
        public string ExchangeCodeHash { get; set; }

        /// <summary>
        /// UTC expiry for the exchange code (short TTL).
        /// </summary>
        public DateTime? ExchangeExpiresAtUtc { get; set; }

        /// <summary>
        /// UTC timestamp when the exchange code was consumed (single-use).
        /// </summary>
        public DateTime? ExchangeConsumedAtUtc { get; set; }

        // ----- Context / telemetry (non-authoritative) -----

        /// <summary>
        /// Optional return URL, must be allowlisted/validated at consume time.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        /// IP address snapshot (request time).
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User agent snapshot (request time).
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Optional correlation id for tracing request -> send -> consume -> exchange.
        /// </summary>
        public string CorrelationId { get; set; }
    }
}
