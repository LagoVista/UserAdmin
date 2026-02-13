// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e517fe679a6df5f337645f349297c67bea1621dc2d9c4ba1805439908e28e354
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin
{
    public static class CommonLinks
    {
        
        public const string Login = "/auth/login";
        public const string LoginOut = "/auth/logout";

        public const string Home = "/home";
        public const string HomeWelcome = "/home/welcome";
        public const string Register = "/auth/register";
        public const string CompleteUserRegistration = "/auth/user/register";
        public const string CreateDefaultOrg = "/auth/org/create";

        public const string ConfirmEmail = "/auth/user/email/confirm";
        public const string ConfirmEmailSent = "/auth/user/email/confirm/sent";
        public const string EmailConfirmed = "/auth/user/email/confirmed";
        public const string CouldNotConfirmEmail = "/auth/user/email/couldnotconfirm";

        public const string CreatingOrganization = "/auth/org/creating";

        public const string ForgotPasswordSent = "/auth/password/forgot/sent";

        public const string Invitations = "/auth/invitations";
        public const string AcceptInviteId = "/auth/invite/accept/{inviteid}";
        public const string InviteAccepted = "/auth/invite/accepted";
        public const string InviteAcceptedFailed = "/auth/invite/failed";

        public const string OAuthFault = "/auth/oauth/fault";
        public const string OAuthAccessDenied = "/auth/oauth/accessdenied";

        public const string TOTPEnrollStart = "/auth/mfa/totp/enroll";
        public const string TOTPEnrollConfirm = "/auth/mfa/totp/confirm";

        public const string PasskeyEnrollStart = "/auth/passkey/enroll/start";
        public const string PasskeyEnrollConfirm = "/auth/passkey/enroll/confirm";
    }
}
