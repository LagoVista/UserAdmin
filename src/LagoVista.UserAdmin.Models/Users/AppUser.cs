// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c9ae3e045a75f873d74b4a3a328b35679c7c3f242d29350aaf6c24e99f58f767
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LagoVista.UserAdmin.Models.Users
{
    public enum LoginTypes
    {
        DeviceOwner,
        AppUser,
        Kiosk,
        AppEndUser,
    }

    public enum UserSetupStates
    {
        Unknown,
        RequiresBasicRegistration, /* Has Valid Email (not confirmed), has Valid First/Last Name */
        RequiresEmailConfirmation,
        RequiresOrgAssingment,
        Ready,
    }

    [EntityDescription(
        Domains.UserDomain, UserAdminResources.Names.AppUser_Title, UserAdminResources.Names.AppUser_Help, UserAdminResources.Names.AppUser_Description,
        EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources),

        ClusterKey: "users", ModelType: EntityDescriptionAttribute.ModelTypes.DomainEntity, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Primary,
        IndexPriority: 95, IndexTagsCsv: "userdomain,users,domainentity")]
    public class AppUser : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, ISummaryFactory, IFormDescriptor, IFormDescriptorCol2, IAppUserAuthTenantState
    {
        public AppUser(String email, String createdBy) : this(email, email, createdBy)
        {

        }
            
        public AppUser(String email, string userName, String createdBy)
        {
            Id = Guid.NewGuid().ToId();
            Key = Guid.NewGuid().ToId().ToLower(); 
            Email = email;
            UserName = userName;
            CreatedBy = new EntityHeader()
            {
                Id = Id,
                Text = createdBy
            };
            CreationDate = DateTime.UtcNow.ToJSONString();

            IsPreviewUser = false;

            LastUpdatedBy = new EntityHeader()
            {
                Id = Id,
                Text = createdBy
            };

            LastUpdatedDate = DateTime.UtcNow.ToJSONString();

            ProfileImage = new ImageDetails()
            {
                Width = 128,
                Height = 128,
                ImageUrl = "https://bytemaster.blob.core.windows.net/userprofileimages/watermark.png",
                Id = "b78ca749a1e64ce59df4aa100050dcc7"
            };

            IsUserDevice = false;

            Organizations = new List<EntityHeader>();
            CurrentOrganizationRoles = new List<EntityHeader>();
            ExternalLogins = new List<ExternalLogin>();
        }

        public UserSetupStates GetUserSetupState() {
            if(String.IsNullOrEmpty(Email) || String.IsNullOrEmpty(FirstName) || String.IsNullOrEmpty(LastName))
            {
                return UserSetupStates.RequiresBasicRegistration;
            }
            else if(!EmailConfirmed)
            {
                return UserSetupStates.RequiresEmailConfirmation;
            }
            else if(Organizations == null || Organizations.Count == 0)
            {
                return UserSetupStates.RequiresOrgAssingment;
            }
            else 
            {
                return UserSetupStates.Ready;
            }
        }

        public AppUser()
        {
            ProfileImage = new ImageDetails()
            {
                Width = 128,
                Height = 128,
                ImageUrl = "https://bytemaster.blob.core.windows.net/userprofileimages/watermark.png",
                Id = "b78ca749a1e64ce59df4aa100050dcc7"
            };

            IsUserDevice = false;
            ExternalLogins = new List<ExternalLogin>();
        }

        private string _key = Guid.NewGuid().ToId().ToLower();
        public override string Key {
            get
            {
                if( _key == null )
                    _key = Guid.NewGuid().ToId().ToLower();

                return _key.ToLower();

            }
            set { _key = value; }
        }

        public string SendGridSenderId { get; set; }
        public bool SendGridVerified { get; set; }
        public string SendGridVerifiedFailedReason { get; set; }

        public LoginTypes LoginType { get; set; } = LoginTypes.AppUser;

        public string LoginTypeName
        {
            get{return LoginType.ToString();}
        }

        public bool IsAnonymous { get; set; } = false;

        public string LastLogin { get; set; }

        public bool ShowWelcome { get; set; } = true;

        public bool HasGeneratedPassword { get; set; }

        public Dictionary<string, string> Preferences { get; set; } = new Dictionary<string, string>();

        public List<EntityHeader<string>> Notes { get; set; } = new List<EntityHeader<string>>();

        public List<ExternalLogin> ExternalLogins { get; set; }

        public List<EntityHeader> Organizations { get; set; }
        public OrganizationSummary CurrentOrganization { get; set; }
        public List<EntityHeader> CurrentOrganizationRoles { get; set; }

        public bool IsCustomerAdmin { get; set; }
        public EntityHeader Customer {  get; set; }
        public EntityHeader CustomerContact { get; set; }

        public EntityHeader CurrentRepo { get; set; }
        public EntityHeader CurrentInstance { get; set; }
        public EntityHeader CurrentDevice { get; set; }
        public EntityHeader CurrentDeviceConfig { get; set; }

        public string SignatureSvgSecretId { get; set; } 
        public string InitialsImageSvgSecretId { get; set; }

        public string HomePage { get; set; }
        public string MobileHomePage { get; set; }
        public string CurrentDeviceId { get; set; }

        /* MFA (Platform) */

        // Plaintext slot used only during set/rotate; manager encrypts/stores and sets SecretId.
        public string AuthenticatorKey { get; set; }
        public string AuthenticatorKeySecretId { get; set; }

        // One string containing recovery-code material (e.g., JSON of hashed codes + metadata).
        public string RecoveryCodes { get; set; }
        public string RecoveryCodesSecretId { get; set; }

        // ISO8601 timestamp (DateTime.UtcNow.ToJSONString()) of last successful MFA (for step-up freshness).
        public string LastMfaDateTimeUtc { get; set; }

        // Replay protection: last accepted TOTP timestep (30s steps). 0 => never accepted.
        public long LastTotpAcceptedTimeStep { get; set; }


        public EntityHeader EndUserAppOrg { get; set; }

        public ImageDetails ProfileImage { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Bio, FieldType: FieldTypes.MultiLineText, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string Bio { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.AppUser_UserTitle, FieldType:FieldTypes.Text, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string Title { get; set; }

        private string _email;
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, IsRequired: false, FieldType: FieldTypes.Email, ResourceType: typeof(UserAdminResources))]
        public string Email
        {
            get { return _email; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _email = null;
                }
                else
                {
                    _email = value.ToUpper();
                }
            }
        }
        public bool EmailConfirmed { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_FirstName, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string FirstName { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.AppUser_LastName, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string LastName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_IsSystemAdmin, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsSystemAdmin { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_IsOrgAdmin, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsOrgAdmin { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_IsAppBuilder, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsAppBuilder { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_IsAppBuilder, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsFinanceAdmin { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_IsUserDevice, FieldType: FieldTypes.CheckBox, IsUserEditable:false, ResourceType: typeof(UserAdminResources))]
        public bool IsUserDevice { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Appuser_IsRuntimeUser, FieldType: FieldTypes.CheckBox, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public bool IsRuntimeuser { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Appuser_IsAccountDisabled, FieldType: FieldTypes.CheckBox, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public bool IsAccountDisabled { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_TeamsAccountName, FieldType: FieldTypes.Text, IsUserEditable: true, ResourceType: typeof(UserAdminResources))]
        public string TeamsAccountName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_SSN, HelpResource:UserAdminResources.Names.AppUser_SSN_Help, SecureIdFieldName:nameof(SsnSecretId), FieldType: FieldTypes.Secret, ResourceType: typeof(UserAdminResources))]
        public string Ssn { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_PrimaryOrganization, HelpResource: UserAdminResources.Names.AppUser_PrimaryOrganization_Help, FieldType: FieldTypes.Text, IsUserEditable: true, ResourceType: typeof(UserAdminResources))]
        public EntityHeader PrimaryOrganization { get; set; }

        public string SsnSecretId { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.AppUser_Language, IsRequired: false, WaterMark: UserAdminResources.Names.AppUser_Language_Select, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader Language { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_TimeZome, IsRequired: false, WaterMark:UserAdminResources.Names.Common_TimeZome_Picker, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader TimeZone { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Address1, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Address1 { get; set; }
      
        [FormField(LabelResource: UserAdminResources.Names.Common_Address2, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Address2 { get; set; }
        
        
        [FormField(LabelResource: UserAdminResources.Names.Common_City, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string City { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_State, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string State { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_Country, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Country { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Common_PostalCode, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string PostalCode { get; set; }
        public List<EntityHeader> MediaResources { get; set; } = new List<EntityHeader>();


        [FormField(LabelResource: UserAdminResources.Names.AppUser_EmailSignature, FieldType: FieldTypes.HtmlEditor, ResourceType: typeof(UserAdminResources))]
        public string EmailSignature { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_PhoneNumber, FieldType: FieldTypes.Phone, ResourceType: typeof(UserAdminResources))]
        public string PhoneNumber { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool PhoneNumberConfirmedForBilling { get; set; }

        public bool TermsAndConditionsAccepted { get; set; }
        public string TermsAndConditionsAcceptedIPAddress { get; set; }
        public string TermsAndConditionsAcceptedDateTime { get; set; }

        public bool AdvancedUser { get; set; }


        public string PaymentAccount1 { get; set; }
        public string RoutingAccount1 { get; set; }
        public string PaymentAccount1Secureid { get; set; }
        public string RoutingAccount1SecureId { get; set; }


        public string PaymentAccount2 { get; set; }
        public string RoutingAccount2 { get; set; }
        public string PaymentAccount2Secureid { get; set; }
        public string RoutingAccount2SecureId { get; set; }

        public string[] PendingInviteIds {get; set;} = new string[] { };

        public override String Name
        {
            get { return $"{FirstName} {LastName}"; }
            set { }
        }


        private string _userName;
        [FormField(LabelResource: UserAdminResources.Names.AppUser_UserName, IsUserEditable:false, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string UserName
        {
            get { return _userName; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    _userName = null;
                }
                else
                {
                    _userName = value.ToUpper();
                }
            }
        }

        public new EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id, UserName, $"{FirstName} {LastName}");
        }

        public int ViewedSystemNotificationIndex { get; set; }

        public bool IsPreviewUser { get; set; }

        public int AccessFailedCount { get; set; }
        public string LockoutDate { get; set; }
        public bool LockoutEnabled { get; set; }
        public string PasswordHash { get; set; }
        public string SecurityStamp { get; set; }
        public bool TwoFactorEnabled { get; set; }

        public EntityHeader PrimaryDevice { get; set; }
        public EntityHeader DeviceRepo { get; set; }
        public EntityHeader DeviceConfiguration { get; set; }

        public IList<ThirdPartyLoginInfo> Logins { get; set; }

        public UserInfo ToUserInfo()
        {
            return new UserInfo()
            {
                Id = Id,
                Key = Id,
                Name = Name,
                IsSystemAdmin = IsSystemAdmin,
                FirstName = FirstName,
                LastName = LastName,
                CreatedBy = CreatedBy,
                CreationDate = CreationDate,
                IsPreviewUser = IsPreviewUser,
                IsOrgAdmin = IsOrgAdmin,
                IsAppBuilder = IsAppBuilder,
                IsRuntimeUser = IsRuntimeuser,
                IsUserDevice = IsUserDevice,
                IsAccountDisabled = IsAccountDisabled,
                CurrentOrganization = CurrentOrganization?.ToEntityHeader(),
                Email = Email,
                Bio = Bio,
                Title = Title,
                PrimaryDevice = PrimaryDevice,
                DeviceConfiguration = DeviceConfiguration,
                DeviceRepo = DeviceRepo,
                EmailConfirmed = EmailConfirmed,                
                LastUpdatedBy = LastUpdatedBy,
                LastUpdatedDate = LastUpdatedDate,
                PhoneNumber = PhoneNumber,
                TermsAndConditionsAccepted = TermsAndConditionsAccepted,
                TermsAndConditionsAcceptedDateTime = TermsAndConditionsAcceptedDateTime,
                TermsAndConditionsAcceptedIPAddress = TermsAndConditionsAcceptedIPAddress,
                UserName = UserName,
                PhoneNumberConfirmed = PhoneNumberConfirmed,
                ProfileImageUrl = ProfileImage,
                TeamsAccountName = TeamsAccountName,
                ExternalLogins = ExternalLogins,
                ShowWelcome = ShowWelcome,
                Notes = Notes,
                SsnSecretId = SsnSecretId,
                Address1 = Address1,
                EmailSignature = EmailSignature,
                Address2 = Address2,
                City = City,
                State = State,
                PostalCode = PostalCode,
                Country = Country,
                LastLogin = LastLogin,
                Roles = CurrentOrganizationRoles,
                MediaResources = MediaResources,
                PrimaryOrganization = PrimaryOrganization,
                TimeZone = TimeZone,
            };
        }

        public UserInfoSummary CreateSummary(bool isOrgAdmin = false, bool isAppBuilder = false)
        {
            return new UserInfoSummary()
            {
                Email = Email,
                CreationDate = CreationDate,
                EmailConfirmed = EmailConfirmed,
                Id = Id,
                IsSystemAdmin = IsSystemAdmin,
                IsOrgAdmin = isOrgAdmin,
                IsAppBuilder = isAppBuilder,
                IsAccountDisabled = IsAccountDisabled,
                IsRuntimeUser = IsRuntimeuser,                
                IsUserDevice = IsUserDevice,
                Name = Name,
                PhoneNumber = PhoneNumber,
                PhoneNumberConfirmed = PhoneNumberConfirmed,
                ProfileImageUrl = ProfileImage,
                Title = Title,
                Customer = Customer,
                CustomerContact = CustomerContact,
                LoginType = LoginType.ToString(),
                TeamsAccountName = TeamsAccountName,
                CurrentOrganization = CurrentOrganization?.ToEntityHeader(),
                Key = Id,
                LastLogin = LastLogin,
                TimeZone = TimeZone,
            };
        }

        public ISummaryData CreateSummary()
        {
            return this.CreateSummary(false, false);
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(UserName),
                nameof(Email),
                nameof(PhoneNumber),
                nameof(Address1),
                nameof(Address2),
                nameof(City),
                nameof(State),
                nameof(PostalCode),
                nameof(Country),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(Title),
                nameof(Bio),
                nameof(Language),
                nameof(TimeZone),
            };
        }

        public List<PushNotificationChannel> PushNotificationChannels { get; set; } = new List<PushNotificationChannel>();
    }

    public class AppUserTotpEnrollmentInfo
    {
        public string Secret { get; set; }
        public string OtpAuthUri { get; set; }
    }
}
