// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 59ddaff5b69d843c268d2896a096eab840ad6ae8700e6122a0676016ee620d41
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models.UIMetaData;
using System;

namespace LagoVista.UserAdmin.Models
{
    [DomainDescriptor]
    public class Domains
    {

        public const string NotificationsDomain = "NotificationsDomain";
        public const string AuthDomain = "AuthDomain";
        public const string UserDomain = "UserDomain";
        public const string OrganizationDomain = "OrganizationDomain";
        public const string OrgLocations = "OrgLocations";
        public const string SecurityDomain = "SecurityDomain";
        public const string EmailServicesDomain = "EmailServices";
        public const string MiscDomain = "MiscDomain";
        public const string AuthTesting = "AuthTesting";

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

        [DomainDescription(AuthDomain)]
        public static DomainDescription AuthenticationDomain
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Authentication Models and Services.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "Auth Domain",
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


        [DomainDescription(AuthDomain)]
        public static DomainDescription AuthDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Authentication Models and Services.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "Auth Domain",
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