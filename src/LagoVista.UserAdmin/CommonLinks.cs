// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e517fe679a6df5f337645f349297c67bea1621dc2d9c4ba1805439908e28e354
// IndexVersion: 2
// --- END CODE INDEX META ---
namespace LagoVista.UserAdmin
{
    public static class CommonLinks
    {
        public const string AuthEntry = "/auth";
        public const string AuthWelcome = "/auth/welcome";
        public const string AuthWelcomeBack = "/auth/welcome/back";

        public const string AuthContinueEmail = "/auth/continue/email";
        public const string AuthContinueEmailPassword = "/auth/continue/email/password";
        public const string AuthContinuePasskey = "/auth/continue/passkey";
        public const string AuthContinueProvider = "/auth/continue/provider";
        public const string AuthContinueTotp = "/auth/continue/totp";
        public const string AuthContinueTotpUnable = "/auth/continue/totp/unable";
        public const string AuthSignInUnable = "/auth/sign-in/unable";
        public const string AuthSignInLockedOut = "/auth/sign-in/locked-out";

        public const string PasswordResetRequest = "/auth/password/reset/request";
        public const string PasswordResetSent = "/auth/password/reset/sent";
        public const string PasswordResetComplete = "/auth/password/reset/complete";

        public const string MagicLinkSent = "/auth/magic-link/sent";
        public const string MagicLinkHandle = "/auth/magic-link/handle";

        public const string OAuthLogins = "/auth/oauth/logins";
        public const string OAuthStart = "/auth/oauth/start";
        public const string OAuthHandle = "/auth/oauth/handle";
        public const string OAuthAccessDenied = "/auth/oauth/access-denied";
        public const string OAuthFault = "/auth/oauth/fault";

        public const string EmailVerificationRequest = "/auth/email-verification/request";
        public const string EmailVerificationConfirm = "/auth/email-verification/confirm";
        public const string EmailVerificationSent = "/auth/email-verification/sent";
        public const string EmailVerificationConfirmed = "/auth/email-verification/confirmed";
        public const string EmailVerificationFailed = "/auth/email-verification/failed";

        public const string RegistrationCreateAccount = "/auth/registration/create-account";
        public const string RegistrationCompleteProfile = "/auth/registration/complete-profile";

        public const string InvitationReview = "/auth/invitation/{invitation-id}";
        public const string InvitationAccepted = "/auth/invitation/accepted";
        public const string InvitationFailed = "/auth/invitation/failed";
        public const string Invitations = "/auth/invitations";

        public const string TotpEnrollmentStart = "/auth/mfa/totp/enroll";
        public const string TotpEnrollmentConfirm = "/auth/mfa/totp/confirm";
        public const string PasskeyStepUp = "/auth/mfa/passkey/step-up";

        public const string PasskeyEnrollmentStart = "/auth/passkey/enrollment/start";
        public const string PasskeyEnrollmentConfirm = "/auth/passkey/enrollment/confirm";
        public const string PasskeyManagement = "/auth/passkey/management";

        public const string UserState = "/auth/user/state";
        public const string OrganizationCreate = "/auth/organization/create";
        public const string OrganizationCreating = "/auth/organization/creating";
        public const string Logout = "/auth/logout";

        public const string Home = "/home";
        public const string HomeWelcome = "/home/welcome";

        // Temporary member aliases keep existing server call sites compiling while
        // their references are migrated. All aliases emit canonical route paths.
        public const string Login = AuthEntry;
        public const string LoginOut = Logout;
        public const string Register = RegistrationCreateAccount;
        public const string CompleteUserRegistration = RegistrationCompleteProfile;
        public const string CreateDefaultOrg = OrganizationCreate;
        public const string ConfirmEmail = EmailVerificationConfirm;
        public const string ConfirmEmailSent = EmailVerificationSent;
        public const string EmailConfirmed = EmailVerificationConfirmed;
        public const string CouldNotConfirmEmail = EmailVerificationFailed;
        public const string CreatingOrganization = OrganizationCreating;
        public const string ForgotPasswordSent = PasswordResetSent;
        public const string AcceptInviteId = "/auth/invitation/{inviteid}";
        public const string InviteAccepted = InvitationAccepted;
        public const string InviteAcceptedFailed = InvitationFailed;
        public const string TOTPEnrollStart = TotpEnrollmentStart;
        public const string TOTPEnrollConfirm = TotpEnrollmentConfirm;
        public const string PasskeyEnrollStart = PasskeyEnrollmentStart;
        public const string PasskeyEnrollConfirm = PasskeyEnrollmentConfirm;
    }
}