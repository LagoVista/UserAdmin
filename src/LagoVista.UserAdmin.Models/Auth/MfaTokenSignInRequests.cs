using LagoVista.Core.Authentication.Models;

namespace LagoVista.UserAdmin.Models.Auth
{
    public class TotpTokenSignInRequest
    {
        public string MfaChallengeId { get; set; }
        public string Totp { get; set; }
        public AuthRequest Auth { get; set; }
    }

    public class RecoveryCodeTokenSignInRequest
    {
        public string MfaChallengeId { get; set; }
        public string RecoveryCode { get; set; }
        public AuthRequest Auth { get; set; }
    }
}
