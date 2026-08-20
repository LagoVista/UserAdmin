namespace LagoVista.UserAdmin.Models.Auth
{
    public class RecoveryCodeSignInRequest
    {
        public string Email { get; set; }
        public string RecoveryCode { get; set; }
        public string MfaChallengeId { get; set; }
        public bool RememberMe { get; set; }
    }
}
