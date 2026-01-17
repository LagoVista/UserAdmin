using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Orgs;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Testing
{
    /// <summary>
    /// Minimal surface for black-box auth ceremony verification.
    /// Implemented by AppUser so snapshot/verification can be computed without coupling to full user model.
    /// </summary>
    public interface IAppUserAuthTenantState
    {
        string Id { get; }
        string UserName { get; }
        string Email { get; }
        bool EmailConfirmed { get; }

        string PhoneNumber { get; }
        bool PhoneNumberConfirmed { get; }

        bool TwoFactorEnabled { get; }
        string LastMfaDateTimeUtc { get; }

        bool IsAccountDisabled { get; }
        bool IsAnonymous { get; }

        bool ShowWelcome { get; }

        /// <summary>
        /// Tenant/org context for the user.
        /// </summary>
        OrganizationSummary CurrentOrganization { get; }
        List<EntityHeader> Organizations { get; }

        /// <summary>
        /// External OAuth login providers attached to the user.
        /// </summary>
        List<ExternalLogin> ExternalLogins { get; }
    }
}
