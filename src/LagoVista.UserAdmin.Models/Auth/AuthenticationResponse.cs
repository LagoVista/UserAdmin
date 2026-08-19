using System;

namespace LagoVista.UserAdmin.Models.Auth
{
    public class AuthenticationResponse
    {
        public AuthenticationResponseState AuthenticationState { get; set; }

        public string AuthenticationReasonCode { get; set; }

        public string PendingIdentityId { get; set; }

        public string MaskedEmail { get; set; }

        /// <summary>
        /// Backward-compatible primary provider hint. Clients should prefer AvailableMfaProviders
        /// when AuthenticationState is MfaRequired.
        /// </summary>
        public string Provider { get; set; }

        /// <summary>
        /// Authentication methods that may satisfy the outstanding MFA requirement.
        /// Known values are "totp" and "passkey".
        /// </summary>
        public string[] AvailableMfaProviders { get; set; }

        public string InviteId { get; set; }

        public string ResponseMessage { get; set; }

        public string RedirectPage { get; set; }

        public bool CanEnterApplication => AuthenticationState == AuthenticationResponseState.Authenticated;

        public AuthenticationResponse()
        {
            AuthenticationReasonCode = String.Empty;
            PendingIdentityId = String.Empty;
            MaskedEmail = String.Empty;
            Provider = String.Empty;
            AvailableMfaProviders = Array.Empty<string>();
            InviteId = String.Empty;
            ResponseMessage = String.Empty;
            RedirectPage = String.Empty;
        }
    }
}
