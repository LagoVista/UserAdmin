using LagoVista.Core.Authentication.Models;

namespace LagoVista.UserAdmin.Models.Auth.Passkeys
{
    public class PasskeyTokenAuthenticationCompleteRequest
    {
        public PasskeyAuthenticationCompleteRequest Passkey { get; set; }
        public AuthRequest Auth { get; set; }
    }
}
