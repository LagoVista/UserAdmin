using LagoVista.Core.Authentication.Models;

namespace LagoVista.UserAdmin.Models.Auth.Passkeys
{
    public class PasskeyEmailAuthenticationBeginRequest
    {
        public string Email { get; set; }
        public string PasskeyUrl { get; set; }
    }

    public class PasskeyEmailAuthenticationCompleteRequest
    {
        public string Email { get; set; }
        public bool StepUp { get; set; }
        public PasskeyAuthenticationCompleteRequest Passkey { get; set; }
    }

    public class PasskeyEmailAuthenticationTokenCompleteRequest
    {
        public string Email { get; set; }
        public bool StepUp { get; set; }
        public PasskeyAuthenticationCompleteRequest Passkey { get; set; }
        public AuthRequest Auth { get; set; }
    }

    public class PasskeyMfaAuthenticationBeginRequest
    {
        public string MfaChallengeId { get; set; }
        public string PasskeyUrl { get; set; }
    }

    public class PasskeyMfaAuthenticationCompleteRequest
    {
        public string MfaChallengeId { get; set; }
        public PasskeyAuthenticationCompleteRequest Passkey { get; set; }
    }

    public class PasskeyMfaAuthenticationTokenCompleteRequest
    {
        public string MfaChallengeId { get; set; }
        public PasskeyAuthenticationCompleteRequest Passkey { get; set; }
        public AuthRequest Auth { get; set; }
    }
}
