namespace LagoVista.UserAdmin.Models.Auth
{
    public enum EmailVerificationSendOutcome
    {
        Sent,
        Resent,
        Throttled
    }

    public class EmailVerificationSendResult
    {
        public EmailVerificationSendOutcome Outcome { get; set; }
        public string VerificationCode { get; set; }
        public int RetryAfterSeconds { get; set; }
    }
}
