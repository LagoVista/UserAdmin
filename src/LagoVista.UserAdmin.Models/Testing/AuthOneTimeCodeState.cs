namespace LagoVista.UserAdmin.Models.Testing
{
    public enum AuthOneTimeCodeStatus
    {
        DontCare,
        NotSet,
        Valid,
        Expired,
        Consumed
    }

    public class AuthOneTimeCodeState
    {
        public AuthOneTimeCodeStatus Status { get; set; } = AuthOneTimeCodeStatus.DontCare;

        public int? AttemptCount { get; set; }
    }
}
