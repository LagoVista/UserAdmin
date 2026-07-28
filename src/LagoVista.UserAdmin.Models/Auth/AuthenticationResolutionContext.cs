using System;

namespace LagoVista.UserAdmin.Models.Auth
{
    public class AuthenticationResolutionContext
    {
        public bool CredentialValidated { get; set; }
        public bool UserExists { get; set; }
        public bool AccountLocked { get; set; }
        public bool AccountDisabled { get; set; }
        public bool AuthBucketLinked { get; set; }
        public bool HasPendingIdentity { get; set; }
        public bool PendingIdentityExpired { get; set; }
        public bool EmailVerified { get; set; }
        public bool VerifiedEmailMatchesExistingUser { get; set; }
        public bool ProfileComplete { get; set; }
        public bool DurableUserResolved { get; set; }
        public bool MfaRequired { get; set; }

        public string ReasonCode { get; set; }
        public string PendingIdentityId { get; set; }
        public string MaskedEmail { get; set; }
        public string Provider { get; set; }
        public string InviteId { get; set; }

        public AuthenticationResolutionContext()
        {
            ReasonCode = String.Empty;
            PendingIdentityId = String.Empty;
            MaskedEmail = String.Empty;
            Provider = String.Empty;
            InviteId = String.Empty;
        }
    }
}
