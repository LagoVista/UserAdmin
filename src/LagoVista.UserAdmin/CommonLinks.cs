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
        public const string CreateDefaultOrg = "/auth/org/createdefault";
        public const string ConfirmEmail = "/auth/email/confirm";
        public const string CreatingOrganization = "/auth/org/creating";

        public const string ForgotPassword = "/auth/email/confirm";
        public const string AcceptInviteId = "/auth/invite/accept/{inviteid}";
        public const string InviteAccepted = "/auth/invite/accepted";

        public const string OAuthFault = "/auth/oauth/fault";
        public const string OAuthAccessDenied = "/auth/oauth/accessdenied";
    }
}
