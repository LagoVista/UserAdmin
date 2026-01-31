using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Users;
using System;

namespace LagoVista.UserAdmin.Models.Security.Passkeys
{
    [EntityDescription(
        Domains.AuthDomain,
        UserAdminResources.Names.PasskeySignInResult_Name,
        UserAdminResources.Names.PasskeySignInResult_Help,
        UserAdminResources.Names.PasskeySignInResult_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel,
        typeof(UserAdminResources))]
    public class PasskeySignInResult
    {
        public AppUser User { get; set; }

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
