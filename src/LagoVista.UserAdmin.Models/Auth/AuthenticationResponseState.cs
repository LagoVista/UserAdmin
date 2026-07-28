namespace LagoVista.UserAdmin.Models.Auth
{
    public enum AuthenticationResponseState
    {
        Unknown = 0,
        Authenticated = 1,
        InvalidCredentials = 2,
        AccountLocked = 3,
        AccountDisabled = 4,
        EmailVerificationRequired = 5,
        RegistrationRequired = 6,
        IdentityLinkRequired = 7,
        MfaRequired = 8,
        PendingIdentityExpired = 9,
        AuthenticationFailed = 10
    }
}
