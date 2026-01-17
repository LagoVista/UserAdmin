using System;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    public class PasskeySignInResult
    {
        public string UserId { get; set; }

        /// <summary>
        /// True when the user is authenticated but still must complete onboarding (e.g., name/email capture + email confirmation)
        /// before being granted access.
        /// </summary>
        public bool RequiresOnboarding { get; set; }

        /// <summary>
        /// Optional relative URL (e.g., /auth/onboarding) for the caller to redirect the user.
        /// </summary>
        public string RedirectUrl { get; set; }

        /// <summary>
        /// Optional informational message for UI.
        /// </summary>
        public string Message { get; set; }
    }
}
