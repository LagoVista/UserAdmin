using LagoVista.Core.Attributes;
using LagoVista.Core.Models.UIMetaData;
using System;

namespace LagoVista.UserAdmin.Models
{
    [DomainDescriptor]
    public class Domains
    {
        public const string UserDomain = "UserDomain";
        public const string UserViewModels = "UserViewModels";
        public const string OrganizationDomain = "OrganizationDomain";
        public const string OrganizationViewModels = "OrganizationViewModels";
        public const string SecurityDomain = "SecurityDomain";
        public const string SecurityViewModels = "SecurityViewModels";
        public const string EmailServicesDomain = "EmailServices";
        public const string MiscDomain = "MiscDomain";

        [DomainDescription(UserDomain)]
        public static DomainDescription UserDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "Models and Data Structures that allow a user to create or update entities associated with users.",
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

        [DomainDescription(UserViewModels)]
        public static DomainDescription UserViewModelsDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "View Models that allow a user to create or update entities associated with users.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "User View Models",
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

        [DomainDescription(OrganizationViewModels)]
        public static DomainDescription OrganizationViewModelsDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "View Models that are used to create or update entities associated with an organization.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "Organization View Models",
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

        [DomainDescription(SecurityViewModels)]
        public static DomainDescription SecurityViewModelsDomainDescription
        {
            get
            {
                return new DomainDescription()
                {
                    Description = "View Models that are used to authenticate or authorize a user.",
                    DomainType = DomainDescription.DomainTypes.BusinessObject,
                    Name = "Security View Models",
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
