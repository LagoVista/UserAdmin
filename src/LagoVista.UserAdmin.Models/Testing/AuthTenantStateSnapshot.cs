using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// JSON-friendly snapshot of the auth + tenant/org state used for ceremony preconditions and postconditions.
    /// This is intentionally a POCO (no serializer-specific attributes).
    ///
    /// Design notes:
    /// - Use nullable booleans so the DSL can specify only the fields it cares about.
    /// - Keep identifiers as strings (Id/ToId pattern) for cross-process compatibility.
    /// </summary>
    public class AuthTenantStateSnapshot
    {
        public bool UserExists {get; set;}
        public bool BelongsToOrg {get; set;}

        /* Auth flags */
        public bool? EmailConfirmed { get; set; }
        public bool? PhoneNumberConfirmed { get; set; }
        public bool? TwoFactorEnabled { get; set; }
        public bool? IsAccountDisabled { get; set; }
        public bool? IsAnonymous { get; set; }

        /* UX */
        public bool? ShowWelcome { get; set; }

        /* Factors / providers */
        public List<string> ExternalLoginProviders { get; set; } = new List<string>();

        /* MFA freshness */
        public string LastMfaDateTimeUtc { get; set; }

        /* Setup helpers */
        public bool? EnsureUserExists { get; set; }
        public bool? EnsureUserDoesNotExist { get; set; }
    
    }
}
