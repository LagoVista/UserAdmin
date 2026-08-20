namespace LagoVista.UserAdmin.Models.Auth
{
    public class TotpSignInRequest
    {
        public string Email { get; set; }
        public string Totp { get; set; }
        public string MfaChallengeId { get; set; }
        public bool RememberMe { get; set; }
    }
}
