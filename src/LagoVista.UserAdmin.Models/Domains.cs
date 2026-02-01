// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 59ddaff5b69d843c268d2896a096eab840ad6ae8700e6122a0676016ee620d41
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models.UIMetaData;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models
{
    [DomainDescriptor]
    public class Domains
    {

        public const string NotificationsDomain = "NotificationsDomain";
        public const string UserDomain = "UserDomain";
        public const string OrganizationDomain = "OrganizationDomain";
        public const string OrgLocations = "OrgLocations";
        public const string SecurityDomain = "SecurityDomain";
        public const string EmailServicesDomain = "EmailServices";
        public const string MiscDomain = "MiscDomain";
        public const string AuthTesting = "AuthTesting";
        public const string AuthDomain = "AuthDomain";

        [DomainDescription(AuthDomain)]
        public static DomainDescription AuthDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Authentication and authorization services.",
                        DomainType = DomainDescription.DomainTypes.BusinessObject,
                        Name = "Auth",
                        CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major=4,
                        Minor=0,
                        Build = 001,
                        DateStamp =  new DateTime(2026,1,31),
                        Revision=1,
                        ReleaseNotes = "Initial unstable preview"
                    }
,
                        Clusters = new List<Cluster>() {
    new Cluster() { Key = "login", Name = "Login", Description = "Sign-in and post-login payloads" },
    new Cluster() { Key = "mfa", Name = "MFA", Description = "Two-factor and verification view models" },
    new Cluster() { Key = "invites", Name = "Invites", Description = "Invite acceptance and onboarding/auth bridging" },
    new Cluster() { Key = "passkeys", Name = "Passkeys", Description = "WebAuthn/passkey challenges and credentials" },
    new Cluster() { Key = "oauth", Name = "OAuth", Description = "Mobile OAuth correlation and pending auth records" },
    new Cluster() { Key = "links", Name = "Secure Links", Description = "Time-limited secure links and tracking" }
}};
            }
        }


        [DomainDescription(AuthTesting)]
        public static DomainDescription AuthTestingDomain
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Serices and Models for Authentication and authorization services.",
                        DomainType = DomainDescription.DomainTypes.BusinessObject,
                        Name = "AuthTesting",
                        CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major=4,
                        Minor=0,
                        Build = 001,
                        DateStamp =  new DateTime(2026,1,31),
                        Revision=1,
                        ReleaseNotes = "Initial unstable preview"
                    }
,
                        Clusters = new List<Cluster>() {
                        new Cluster() { Key = "scenarios", Name = "Scenarios", Description = "Auth flow test scenarios (one step with setup/action/expected end-state)" },
                        new Cluster() { Key = "runner", Name = "Runner", Description = "Runner plans and the structured inputs/actions/options used to execute scenarios" },
                        new Cluster() { Key = "runs", Name = "Runs", Description = "Persisted test run executions and append-only run telemetry" },
                        new Cluster() { Key = "verification", Name = "Verification", Description = "Final verification snapshots and auth-log review results" },
                        new Cluster() { Key = "artifacts", Name = "Artifacts", Description = "Evidence produced by runs (screenshots, traces, logs, video, exports)" },
                        new Cluster() { Key = "credentials", Name = "Test Credentials", Description = "Test-user credential inputs (tokens, optional passkey refs) used by the runner" }
                    }                };
            }
        }

        [DomainDescription(NotificationsDomain)]
        public static DomainDescription NotificationsDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Notifications for users or systems.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "Notifications",
                    CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major = 0,
                        Minor = 8,
                        Build = 001,
                        DateStamp = new DateTime(2016, 12, 20),
                        Revision = 1,
                        ReleaseNotes = "Initial unstable preview"
                    }
                };
            }
        }

        [DomainDescription(UserDomain)]
        public static DomainDescription UserDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Models and data structures for managing user accounts, ownership, roles, and related associations.",
                        DomainType = DomainDescription.DomainTypes.BusinessObject,
                        Name = "User",
                        CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major=4,
                        Minor=0,
                        Build = 001,
                        DateStamp =  new DateTime(2026,1,31),
                        Revision=2,
                        ReleaseNotes = "Initial unstable preview"
                    }
,
                        Clusters = new List<Cluster>() {
                            new Cluster() { Key = "users", Name = "Users", Description = "Core user accounts and profile information" },
                            new Cluster() { Key = "registration", Name = "Registration", Description = "Registration and initial account setup flows" },
                            new Cluster() { Key = "invites", Name = "Invitations", Description = "Inviting users and accepting invitations" },
                            new Cluster() { Key = "security", Name = "Security", Description = "Password/email/phone confirmation and security-sensitive actions" },
                            new Cluster() { Key = "phone", Name = "Phone", Description = "Phone verification and call logging" },
                            new Cluster() { Key = "access", Name = "Access Control", Description = "Roles, teams, and user membership models" },
                            new Cluster() { Key = "mru", Name = "Recents", Description = "Most-recently-used navigation and quick access models" },
                            new Cluster() { Key = "assets", Name = "Assets", Description = "User-managed assets and membership link entities" }
                        }};
            }
        }



        [DomainDescription(OrgLocations)]
        public static DomainDescription OrgLocationsDOmainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Models and sevices associated with organization locations.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "Organization Locations",
                    CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major = 0,
                        Minor = 8,
                        Build = 001,
                        DateStamp = new DateTime(2016, 12, 20),
                        Revision = 1,
                        ReleaseNotes = "Initial unstable preview"
                    }
                };
            }
        }

      

        [DomainDescription(OrganizationDomain)]
        public static DomainDescription OrganizationDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Models and Data Structures used to create or update entities assocaiated with an organiztion.",
                        DomainType = DomainDescription.DomainTypes.BusinessObject,
                        Name = "Organization",
                        CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major=4,
                        Minor=0,
                        Build = 001,
                        DateStamp =  new DateTime(2026,1,31),
                        Revision=1,
                        ReleaseNotes = "Initial unstable preview"
                    }
,
                        Clusters = new List<Cluster>() {
                        new Cluster() { Key = "org", Name = "Organization", Description = "Organization container, membership, invitations, and org settings" },
                        new Cluster() { Key = "locations", Name = "Locations", Description = "Locations and sub-locations within an organization" },
                        new Cluster() { Key = "access", Name = "Access Control", Description = "Asset sets and permission/access models" },
                        new Cluster() { Key = "notifications", Name = "Notifications", Description = "Distribution lists, external contacts, and inbox-style items" },
                        new Cluster() { Key = "calendars", Name = "Calendars", Description = "Holiday sets and scheduled downtime configuration" },
                        new Cluster() { Key = "diagrams", Name = "Diagrams", Description = "Location diagrams, shapes, and layers" },
                        new Cluster() { Key = "subscriptions", Name = "Subscriptions", Description = "Subscription and billing-related models" },
                        new Cluster() { Key = "favorites", Name = "Favorites", Description = "User favorites and quick navigation models (org scoped)" },
                        new Cluster() { Key = "authassist", Name = "Auth Assist", Description = "Auth helper payloads used in org flows (reset links, passkey packets, tokens)" },
                        new Cluster() { Key = "integrations", Name = "Integrations", Description = "Social/API integration DTOs and responses (e.g., Twitter OAuth)" }
                    }                };
            }
        }

        [DomainDescription(SecurityDomain)]
        public static DomainDescription SecurityDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Models and Data Strucutes that are used to authenticate or authorize a user",
                        DomainType = DomainDescription.DomainTypes.BusinessObject,
                        Name = "Security",
                        CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major=4,
                        Minor=0,
                        Build = 001,
                        DateStamp =  new DateTime(2026,1,31),
                        Revision=1,
                        ReleaseNotes = "Initial unstable preview"
                    }
,
                        Clusters = new List<Cluster>() {
                        new Cluster() { Key = "audit", Name = "Audit Logs", Description = "Authentication/access auditing and diagnostic logs" },
                        new Cluster() { Key = "ui", Name = "UI Security Map", Description = "Modules/areas/pages/features used to secure and render UI" },
                        new Cluster() { Key = "functions", Name = "Function Map", Description = "Function map and function definitions used for fine-grained authorization" },
                        new Cluster() { Key = "roles", Name = "Roles & Access", Description = "Role access definitions, DTO mappings, and scoped user-role assignments" },
                        new Cluster() { Key = "password", Name = "Password & Verification", Description = "Password change/reset/set and identity verification flows" },
                        new Cluster() { Key = "help", Name = "Help Content", Description = "Help resources and user guidance content" }
                    }                };
            }
        }


        [DomainDescription(MiscDomain)]
        public static DomainDescription MiscDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "A collection of miscellaneous classes that provide strucutes and data to othe parts of the system.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "miscellaneous",
                    CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major = 0,
                        Minor = 8,
                        Build = 001,
                        DateStamp = new DateTime(2016, 12, 20),
                        Revision = 1,
                        ReleaseNotes = "Initial unstable preview"
                    }
                };
            }
        }

        [DomainDescription(EmailServicesDomain)]
        public static DomainDescription EmaailServicesDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "A collection of classes that support sending emails, at this time with SendGrid, this might change at a later date",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "emailservices",
                    CurrentVersion = new Core.Models.VersionInfo()
                    {
                        Major = 0,
                        Minor = 8,
                        Build = 001,
                        DateStamp = new DateTime(2025, 04, 29),
                        Revision = 1,
                        ReleaseNotes = "Initial unstable preview"
                    }
                };
            }
        }
    }
}