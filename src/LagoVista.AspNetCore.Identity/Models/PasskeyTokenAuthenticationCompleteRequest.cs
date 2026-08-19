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
        public PasskeyAuthenticationCompleteRequest Passkey { get; set; }
    }

    public class PasskeyEmailAuthenticationTokenCompleteRequest
    {
        public string Email { get; set; }
        public PasskeyAuthenticationCompleteRequest Passkey { get; set; }
        public AuthRequest Auth { get; set; }
    }
}
