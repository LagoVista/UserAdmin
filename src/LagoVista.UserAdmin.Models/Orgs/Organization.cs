using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Security;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public enum OrgStatuses
    {
        [EnumLabel(Organization.Organization_OrgStatuses_Active, UserAdminResources.Names.Organization_OrgStatuses_Active, typeof(UserAdminResources))]
        Active,
        [EnumLabel(Organization.Organization_OrgStatuses_Deactivated, UserAdminResources.Names.Organization_OrgStatuses_Deactivated, typeof(UserAdminResources))]
        Deactivated,
        [EnumLabel(Organization.Organization_OrgStatuses_Spam, UserAdminResources.Names.Organization_OrgStatuses_Spam, typeof(UserAdminResources))]
        Spam,
    }


    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organization_Title, UserAdminResources.Names.Organization_Help,
        UserAdminResources.Names.Organization_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),
        EditUIUrl: "/organization/orgaccount", Icon: "icon-ae-building", SaveUrl: "/api/org", GetUrl: "/api/org/{id}")]
    public class Organization : UserAdminModelBase, INamedEntity, IKeyedEntity, IValidateable, IOwnedEntity, IFormDescriptor, IFormDescriptorCol2, IIconEntity, ISummaryFactory
    {
        public const string Organization_OrgStatuses_Active = "active";
        public const string Organization_OrgStatuses_Deactivated = "deactivated";
        public const string Organization_OrgStatuses_Spam = "spam";

        public Organization()
        {
            Locations = new List<EntityHeader>();
            OrgStatus = EntityHeader<OrgStatuses>.Create(OrgStatuses.Active);
            Key = Guid.NewGuid().ToId().ToLower();
            DefaultTheme = "default";
            Icon = "icon-ae-building";
            TimeZone = new EntityHeader()
            {
                Id = "UTC",
                Text = "(UTC) Coordinated Universal Time",
            };
        }

        [FormField(LabelResource: UserAdminResources.Names.Common_Namespace, NamespaceType: NamespaceTypes.Organization, NamespaceUniqueMessageResource: UserAdminResources.Names.Organization_NamespaceInUse,
            FieldType: FieldTypes.NameSpace, IsRequired: true, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public string Namespace { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_WebSite, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public String WebSite { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Status, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public String Status { get; set; }

        public bool InitializationCompleted { get; set; }
        public string InitializationCompletedDate { get; set; }
        public EntityHeader InitializationCompletedBy { get; set; }

        public EntityHeader<OrgStatuses> OrgStatus { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_Logo_Light, UserAdminResources.Names.Organization_Logo_LightColor_Help, FieldType: FieldTypes.FileUpload, DisplayImageSize:"800x224", GeneratedImageSize:"1024x1024", UploadUrl: "/api/media/resource/public/upload", ResourceType: typeof(UserAdminResources))]
        public EntityHeader LightLogo { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_Logo_DarkColor, UserAdminResources.Names.Organization_Logo_DarkColor_Help, FieldType: FieldTypes.FileUpload, DisplayImageSize: "800x224", GeneratedImageSize: "1024x1024", UploadUrl: "/api/media/resource/public/upload", ResourceType: typeof(UserAdminResources))]
        public EntityHeader DarkLogo { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_TagLine, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string TagLine { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultTheme, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string DefaultTheme { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_HeroBackground, GeneratedImageSize: "1792x1024", DisplayImageSize:"1350x900", FieldType: FieldTypes.FileUpload, UploadUrl: "/api/media/resource/public/upload", ResourceType: typeof(UserAdminResources))]
        public string HeroBackgroundImage { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_HeroTitle, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string HeroTitle { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultLandingPage, WaterMark:UserAdminResources.Names.Organization_DefaultLandingPage_Select, EntityHeaderPickerUrl:"/api/landingpages", EditorPath: "/contentmanagement/landingpage/{id}", FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultLandingPage { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_LandingPageHostName, HelpResource: UserAdminResources.Names.Organization_LandingPageHostName_Help, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string LandingPageHostName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_TimeZome, IsRequired: true, WaterMark: UserAdminResources.Names.Common_TimeZome_Picker, FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader TimeZone { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_Owner, FieldType: FieldTypes.UserPicker, IsRequired: true, IsUserEditable: true,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader Owner { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_Primary_Location, IsRequired: false, IsUserEditable: true, FieldType: FieldTypes.EntityHeaderPicker,
           EntityHeaderPickerUrl: "/api/org/locations", WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader PrimaryLocation { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_BillingLocation, IsRequired: false, IsUserEditable: true, FieldType: FieldTypes.EntityHeaderPicker,
           EntityHeaderPickerUrl: "/api/org/locations",  WaterMark: UserAdminResources.Names.Organization_BillingLocation_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader BillingLocation { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Admin_Contact, FieldType: FieldTypes.UserPicker, IsRequired: false,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader AdminContact { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_SalesContact, FieldType: FieldTypes.UserPicker, IsRequired: false,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader SalesContact { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Billing_Contact, FieldType: FieldTypes.UserPicker, IsRequired: false,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader BillingContact { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Technical_Contact, FieldType: FieldTypes.UserPicker, IsRequired: false,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, ResourceType: typeof(UserAdminResources))]
        public EntityHeader TechnicalContact { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultRepo, FieldType: FieldTypes.EntityHeaderPicker, IsRequired: false, EntityHeaderPickerUrl: "/api/devicerepos",
            WaterMark: UserAdminResources.Names.Organization_DefaultRepo_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultDeviceRepository { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultInstance, FieldType: FieldTypes.EntityHeaderPicker, IsRequired: false, EntityHeaderPickerUrl: "/api/deployment/instances",
            WaterMark: UserAdminResources.Names.Organization_DefaultInstance_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultInstance { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Organization_AppUrl, HelpResource:UserAdminResources.Names.Organization_AppUrl_Help, FieldType: FieldTypes.WebLink, ResourceType: typeof(UserAdminResources))]
        public string AppUrl { get; set; } = "https://www.nuviot.com";


        [FormField(LabelResource: UserAdminResources.Names.Organization_LinkedInUrl, FieldType: FieldTypes.WebLink, ResourceType: typeof(UserAdminResources))]
        public string LinkeInUrl { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_FacebookUrl, FieldType: FieldTypes.WebLink, ResourceType: typeof(UserAdminResources))]
        public string FacebookUrl { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_XUrl, FieldType: FieldTypes.WebLink, ResourceType: typeof(UserAdminResources))]
        public string XUrl { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_YouTubeUrl, FieldType: FieldTypes.WebLink, ResourceType: typeof(UserAdminResources))]
        public string YouTubeUrl { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_VimeoUrl, FieldType: FieldTypes.WebLink, ResourceType: typeof(UserAdminResources))]
        public string VimeoUrl { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultDemoInstance, FieldType: FieldTypes.EntityHeaderPicker, IsRequired: false, EntityHeaderPickerUrl: "/api/deployment/instances",
            WaterMark: UserAdminResources.Names.Organization_DefaultInstance_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultDemoInstance { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultTestInstance, FieldType: FieldTypes.EntityHeaderPicker, IsRequired: false, EntityHeaderPickerUrl: "/api/deployment/instances",
            WaterMark: UserAdminResources.Names.Organization_DefaultInstance_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultTestInstance { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultDevelopmentInstance, FieldType: FieldTypes.EntityHeaderPicker, IsRequired: false, EntityHeaderPickerUrl: "/api/deployment/instances",
            WaterMark: UserAdminResources.Names.Organization_DefaultInstance_Select, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultDevelopmentInstance { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultProjectLead, HelpResource: UserAdminResources.Names.Organization_DefaultProjectLead_Help, FieldType: FieldTypes.UserPicker,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultProjectLead { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultProjectAdminLead, HelpResource: UserAdminResources.Names.Organization_DefaultProjectAdminLead_Help, FieldType: FieldTypes.UserPicker,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultProjectAdminLead { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultBusinessDevelopmentRep, HelpResource: UserAdminResources.Names.Organization_DefaultBusinessDevelopmentRep_Help, FieldType: FieldTypes.UserPicker,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultBusinessDevelopmentRep { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultContributor, HelpResource: UserAdminResources.Names.Organization_DefaultContributor_Help, FieldType: FieldTypes.UserPicker,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultContributor { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultQAResource, HelpResource: UserAdminResources.Names.Organization_DefaultQAResource_Help, FieldType: FieldTypes.UserPicker,
            WaterMark: UserAdminResources.Names.Organization_DefaultResource_Watermark, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultQAResource { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultIndustry, WaterMark: UserAdminResources.Names.Organization_DefaultIndustry_Select, HelpResource: UserAdminResources.Names.Organization_DefaultIndustry_Help,
            FieldType: FieldTypes.EntityHeaderPicker, EntityHeaderPickerUrl: "/api/industries", ResourceType: typeof(UserAdminResources))]
        public EntityHeader DefaultIndustry { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Organization_TestingIndustry, WaterMark:UserAdminResources.Names.Organization_TestingIndustry_Select, HelpResource: UserAdminResources.Names.Organization_TestingIndustry_Help,
            FieldType: FieldTypes.EntityHeaderPicker, EntityHeaderPickerUrl: "/api/industries", ResourceType: typeof(UserAdminResources))]
        public EntityHeader TestingIndustry { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_TestingIndustryNiche, FieldType: FieldTypes.EntityHeaderPicker, WaterMark: UserAdminResources.Names.Organization_TestingIndustryNiche_Select, EntityHeaderPickerUrl: "/api/industry/{industry.id}/niches", ResourceType: typeof(UserAdminResources))]
        public EntityHeader TestingIndustryNiche { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_IsForProductLine, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool IsForProductLine { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_PrimaryBgColor, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources))]
        public string PrimaryBgColor { get; set; } = "#1976D2";

        [FormField(LabelResource: UserAdminResources.Names.Organization_AccentColor, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources))]
        public string AccentColor { get; set; } = "#D48D17";

        [FormField(LabelResource: UserAdminResources.Names.Common_PrimaryTextColor, FieldType: FieldTypes.Color, ResourceType: typeof(UserAdminResources))]
        public string PrimaryTextColor { get; set; } = "#F4F4F4";

        public List<ModuleSummary> AdditionalModules { get; set; } = new List<ModuleSummary>();


        [FormField(LabelResource: UserAdminResources.Names.Organization_Locations, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public List<EntityHeader> Locations { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_HomePage, HelpResource: UserAdminResources.Names.Organization_HomePage_Help, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string HomePage { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_EndUserHomePage, HelpResource: UserAdminResources.Names.Organization_EndUserHomePage_Help, FieldType: FieldTypes.Text, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string EndUserHomePage { get; set; }


        public bool IsArchived { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Organization_DefaultTeamsWebHook, HelpResource: UserAdminResources.Names.Organization_DefaultTeamsWebHook_Help, FieldType: FieldTypes.WebLink, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string DefaultTeamsWebHook { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Namespace),
                nameof(Icon),
                nameof(Owner),
                nameof(AdminContact),
                nameof(SalesContact),
                nameof(BillingContact),
                nameof(TechnicalContact),
                nameof(DefaultProjectLead),
                nameof(DefaultProjectAdminLead),
                nameof(DefaultContributor),
                nameof(DefaultQAResource),
                nameof(DefaultBusinessDevelopmentRep),
                nameof(FacebookUrl),
                nameof(XUrl),
                nameof(LinkeInUrl),
                nameof(VimeoUrl),
                nameof(YouTubeUrl),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>()
            {
                nameof(TimeZone),
                nameof(PrimaryLocation),
                nameof(BillingLocation),
                nameof(DefaultTheme),
                nameof(WebSite),
                nameof(HomePage),
                nameof(EndUserHomePage),
                nameof(DefaultInstance),
                nameof(DefaultIndustry),
                nameof(DefaultDemoInstance),
                nameof(DefaultTestInstance),
                nameof(DefaultDevelopmentInstance),
                nameof(DefaultDeviceRepository),
                nameof(DefaultTeamsWebHook),
                nameof(TestingIndustry),
                nameof(TestingIndustryNiche),
                nameof(DefaultLandingPage),
                nameof(LandingPageHostName),
                nameof(HeroTitle),
                nameof(HeroBackgroundImage),
                nameof(LightLogo),
                nameof(DarkLogo),
                nameof(TagLine),
                nameof(AppUrl),
                nameof(PrimaryTextColor),
                nameof(PrimaryBgColor),
                nameof(AccentColor),
            };
        }

        public PublicOrgInformation ToPublicOrgInfo()
        {
            if (!IsForProductLine)
                throw new InvalidOperationException("Should only get public information for product line.");

            return new PublicOrgInformation()
            {
                Id = Id,
                AccentColor = AccentColor,
                HeroBackgroundImage = HeroBackgroundImage,
                HeroTitle = HeroTitle,
                Icon = Icon,
                DarkLogo = DarkLogo,
                LightLogo = LightLogo,
                DefaultLandingPage = DefaultLandingPage,
                Name = Name,
                DefaultIndustry = DefaultIndustry,
                Namespace = Namespace,
                PrimaryBgColor = PrimaryBgColor,
                PrimaryTextColor = PrimaryTextColor,
                TagLine = TagLine
            };
        }

        public override string ToString()
        {
            return Namespace;
        }

        public OrganizationSummary CreateSummary()
        {
            return new OrganizationSummary()
            {
                Id = Id,
                Text = Name,
                Name = Name,
                HomePage = HomePage,
                Namespace = Namespace,
                Icon = Icon,
                TagLine = TagLine,
                DarkLogo = DarkLogo,
                LightLogo = LightLogo,
                PrimaryBgColor = PrimaryBgColor,
                PrimaryTextColor = PrimaryTextColor,
                AccentColor = AccentColor,
                IsForProductLine = IsForProductLine,
                DefaultTheme = DefaultTheme,
                DefaultInstance = DefaultInstance,
                DefaultTestInstance = DefaultTestInstance,
                DefaultDevelopmentInstance = DefaultDevelopmentInstance,
                DefaultDemoInstance = DefaultDemoInstance,
                DefaultDeviceRepository = DefaultDeviceRepository
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Organizations_Title, UserAdminResources.Names.Organization_Help,
     UserAdminResources.Names.Organization_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),
     EditUIUrl: "/organization/orgaccount", Icon: "icon-ae-building",
     SaveUrl: "/api/org", GetUrl: "/api/org/{id}")]
    public class OrganizationSummary : SummaryData, IOrganizationSummary
    {
        public string Text { get; set; }
        public string Namespace { get; set; }
        public EntityHeader DarkLogo { get; set; }
        public EntityHeader LightLogo { get; set; }
        public string TagLine { get; set; }
        public string DefaultTheme { get; set; }
        public string HomePage { get; set; }
        public string PrimaryBgColor { get; set; }
        public string PrimaryTextColor { get; set; }
        public string AccentColor { get; set; }
        public EntityHeader DefaultDeviceRepository { get; set; }
        public EntityHeader DefaultInstance { get; set; }
        public EntityHeader DefaultDemoInstance { get; set; }
        public EntityHeader DefaultTestInstance { get; set; }
        public EntityHeader DefaultDevelopmentInstance { get; set; }

        public bool IsForProductLine { get; set; }

        public EntityHeader ToEntityHeader()
        {
            if (String.IsNullOrEmpty(Namespace))
            {
                char[] arr = Text.ToCharArray();

                arr = Array.FindAll<char>(arr, (c => (char.IsLetter(c)
                                                  || char.IsWhiteSpace(c)
                                                  || c == '-')));
                Namespace = new string(arr).ToLower();
            }

            return EntityHeader.Create(Id, Namespace, Text);
        }
    }




    public class OrgHostNameRedirect
    {
        public string HostName { get; set; }
        public string OrgNs { get; set; }
        public EntityHeader LandingPage { get; set; }
    }


    public class PublicOrgInformation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public EntityHeader DarkLogo { get; set; }
        public EntityHeader LightLogo { get; set; }

        public EntityHeader DefaultLandingPage { get; set; }
        public EntityHeader DefaultIndustry { get; set; }

        public string TagLine { get; set; }
        public string Icon { get; set; }
        public string PrimaryBgColor { get; set; }
        public string AccentColor { get; set; }
        public string PrimaryTextColor { get; set; }
        public string HeroBackgroundImage { get; set; }
        public string HeroTitle { get; set; }
    }
}
