using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// Semantic ceremony identifiers intended for logs, test runs, and DSL specs.
    /// These are constant-backed strings so segments can be used for grouping/filtering.
    /// Examples: "auth.login.password", "auth.login.oauth", "auth.logout".
    /// </summary>
    public static class CeremonyIds
    {
        public const string LoginWithPassword = "auth.login.password";
        public const string LoginWithOAuth = "auth.login.oauth";
        public const string Logout = "auth.logout";

        public const string InviteAcceptance = "auth.invite.accept";
        public const string EmailConfirmation = "auth.email.confirm";
        public const string ResetPassword = "auth.password.reset";

        public static readonly IReadOnlyList<string> All = new[]
        {
            LoginWithPassword,
            LoginWithOAuth,
            Logout,
            InviteAcceptance,
            EmailConfirmation,
            ResetPassword,
        };
    }

    /// <summary>
    /// A single user intent execution through auth mechanisms.
    /// V1 constraint: 1 ceremony = 1 test run.
    /// </summary>
    public enum CeremonyTypes
    {
        LoginEmailPassword,
        InviteAcceptance,
        EmailConfirmation,
        ResetPassword,
    }
}
