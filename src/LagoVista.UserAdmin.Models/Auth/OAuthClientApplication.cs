using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Auth
{
    public enum OAuthClientTypes
    {
        [EnumLabel(OAuthClientApplication.ClientType_Public, OAuthClientResources.Names.OAuthClientApplication_ClientType_Public, typeof(OAuthClientResources))]
        Public,
        [EnumLabel(OAuthClientApplication.ClientType_Confidential, OAuthClientResources.Names.OAuthClientApplication_ClientType_Confidential, typeof(OAuthClientResources))]
        Confidential,
    }

    public enum OAuthClientStatuses
    {
        [EnumLabel(OAuthClientApplication.Status_Active, OAuthClientResources.Names.OAuthClientApplication_Status_Active, typeof(OAuthClientResources))]
        Active,
        [EnumLabel(OAuthClientApplication.Status_Disabled, OAuthClientResources.Names.OAuthClientApplication_Status_Disabled, typeof(OAuthClientResources))]
        Disabled,
        [EnumLabel(OAuthClientApplication.Status_Revoked, OAuthClientResources.Names.OAuthClientApplication_Status_Revoked, typeof(OAuthClientResources))]
        Revoked,
    }

    [EntityDescription(Domains.SecurityDomain, OAuthClientResources.Names.OAuthClientApplication_Title,
        OAuthClientResources.Names.OAuthClientApplication_Help, OAuthClientResources.Names.OAuthClientApplication_Description,
        EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(OAuthClientResources),
        SaveUrl: "/api/oauth/client", GetListUrl: "/api/oauth/clients", GetUrl: "/api/oauth/client/{id}",
        DeleteUrl: "/api/oauth/client/{id}", FactoryUrl: "/api/oauth/client/factory",
        ListUIUrl: "/security/oauth-clients", EditUIUrl: "/security/oauth-client/{id}", CreateUIUrl: "/security/oauth-client/add",
        Icon: "icon-ae-key", ClusterKey: "security", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration,
        Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential,
        IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Primary, IndexPriority: 90,
        IndexTagsCsv: "securitydomain,oauth,clients,configuration")]
    public class OAuthClientApplication : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity,
        IDescriptionEntity, IFormDescriptor, IFormConditionalFields, ISummaryFactory
    {
        public const string ClientType_Public = "public";
        public const string ClientType_Confidential = "confidential";
        public const string Status_Active = "active";
        public const string Status_Disabled = "disabled";
        public const string Status_Revoked = "revoked";

        public OAuthClientApplication()
        {
            ClientType = EntityHeader<OAuthClientTypes>.Create(OAuthClientTypes.Public);
            Status = EntityHeader<OAuthClientStatuses>.Create(OAuthClientStatuses.Disabled);
            RedirectUris = new List<OAuthClientSettingValue>();
            PostLogoutRedirectUris = new List<OAuthClientSettingValue>();
            AllowedGrantTypes = new List<OAuthClientSettingValue>();
            AllowedScopes = new List<OAuthClientSettingValue>();
            AllowedResources = new List<OAuthClientSettingValue>();
            RequirePkce = true;
            RequireConsent = true;
            Icon = "icon-ae-key";
        }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_ClientId,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_ClientId_Help,
            FieldType: FieldTypes.Text, ResourceType: typeof(OAuthClientResources), IsRequired: true, IsUserEditable: true)]
        public string ClientId { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_ClientType,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_ClientType_Help,
            WaterMark: OAuthClientResources.Names.OAuthClientApplication_ClientType_Select,
            FieldType: FieldTypes.Picker, EnumType: typeof(OAuthClientTypes), ResourceType: typeof(OAuthClientResources), IsRequired: true)]
        public EntityHeader<OAuthClientTypes> ClientType { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_Status,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_Status_Help,
            WaterMark: OAuthClientResources.Names.OAuthClientApplication_Status_Select,
            FieldType: FieldTypes.Picker, EnumType: typeof(OAuthClientStatuses), ResourceType: typeof(OAuthClientResources), IsRequired: true)]
        public EntityHeader<OAuthClientStatuses> Status { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_ClientSecret,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_ClientSecret_Help,
            SecureIdFieldName: nameof(ClientSecretId), FieldType: FieldTypes.Secret,
            ResourceType: typeof(OAuthClientResources), IsUserEditable: true)]
        public string ClientSecret { get; set; }

        public string ClientSecretId { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_RedirectUris,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_RedirectUris_Help,
            FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/oauth/client/value/factory",
            ResourceType: typeof(OAuthClientResources), IsRequired: true)]
        public List<OAuthClientSettingValue> RedirectUris { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_PostLogoutRedirectUris,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_PostLogoutRedirectUris_Help,
            FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/oauth/client/value/factory",
            ResourceType: typeof(OAuthClientResources))]
        public List<OAuthClientSettingValue> PostLogoutRedirectUris { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_AllowedGrantTypes,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_AllowedGrantTypes_Help,
            FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/oauth/client/value/factory",
            ResourceType: typeof(OAuthClientResources), IsRequired: true)]
        public List<OAuthClientSettingValue> AllowedGrantTypes { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_AllowedScopes,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_AllowedScopes_Help,
            FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/oauth/client/value/factory",
            ResourceType: typeof(OAuthClientResources), IsRequired: true)]
        public List<OAuthClientSettingValue> AllowedScopes { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_AllowedResources,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_AllowedResources_Help,
            FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/oauth/client/value/factory",
            ResourceType: typeof(OAuthClientResources), IsRequired: true)]
        public List<OAuthClientSettingValue> AllowedResources { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_RequirePkce,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_RequirePkce_Help,
            FieldType: FieldTypes.CheckBox, ResourceType: typeof(OAuthClientResources))]
        public bool RequirePkce { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_RequireConsent,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_RequireConsent_Help,
            FieldType: FieldTypes.CheckBox, ResourceType: typeof(OAuthClientResources))]
        public bool RequireConsent { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_AccessTokenLifetimeMinutes,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_AccessTokenLifetimeMinutes_Help,
            FieldType: FieldTypes.Integer, ResourceType: typeof(OAuthClientResources))]
        public int? AccessTokenLifetimeMinutes { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_RefreshTokenLifetimeDays,
            HelpResource: OAuthClientResources.Names.OAuthClientApplication_RefreshTokenLifetimeDays_Help,
            FieldType: FieldTypes.Integer, ResourceType: typeof(OAuthClientResources))]
        public int? RefreshTokenLifetimeDays { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_LogoUrl, FieldType: FieldTypes.Text, ResourceType: typeof(OAuthClientResources))]
        public string LogoUrl { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_PrivacyPolicyUrl, FieldType: FieldTypes.Text, ResourceType: typeof(OAuthClientResources))]
        public string PrivacyPolicyUrl { get; set; }

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientApplication_TermsOfServiceUrl, FieldType: FieldTypes.Text, ResourceType: typeof(OAuthClientResources))]
        public string TermsOfServiceUrl { get; set; }

        [CustomValidator]
        public void Validate(ValidationResult result, Actions action)
        {
            if (EntityHeader.IsNullOrEmpty(ClientType)) result.AddUserError("OAuth client type is required.");
            if (EntityHeader.IsNullOrEmpty(Status)) result.AddUserError("OAuth client status is required.");
            if (ClientType?.Id == ClientType_Public && !RequirePkce) result.AddUserError("Public OAuth clients must require PKCE.");
            if (ClientType?.Id == ClientType_Public && (!String.IsNullOrEmpty(ClientSecret) || !String.IsNullOrEmpty(ClientSecretId)))
                result.AddUserError("Public OAuth clients cannot have a client secret.");
            if (Status?.Id == Status_Active && ClientType?.Id == ClientType_Confidential && String.IsNullOrEmpty(ClientSecret) && String.IsNullOrEmpty(ClientSecretId))
                result.AddUserError("An active confidential OAuth client requires a client secret.");

            ValidateValues(result, RedirectUris, nameof(RedirectUris), true);
            ValidateValues(result, AllowedGrantTypes, nameof(AllowedGrantTypes), true);
            ValidateValues(result, AllowedScopes, nameof(AllowedScopes), true);
            ValidateValues(result, AllowedResources, nameof(AllowedResources), true);
        }

        private static void ValidateValues(ValidationResult result, List<OAuthClientSettingValue> values, string fieldName, bool required)
        {
            if (required && (values == null || values.Count == 0))
            {
                result.AddUserError($"{fieldName} requires at least one value.");
                return;
            }

            if (values == null) return;
            var uniqueValues = new HashSet<string>(StringComparer.Ordinal);
            foreach (var value in values)
            {
                if (value == null || String.IsNullOrWhiteSpace(value.Value)) result.AddUserError($"{fieldName} contains an empty value.");
                else if (!uniqueValues.Add(value.Value)) result.AddUserError($"{fieldName} contains the duplicate value [{value.Value}].");
            }
        }

        public OAuthClientApplicationSummary CreateSummary()
        {
            return new OAuthClientApplicationSummary
            {
                Id = Id, Key = Key, Name = Name, Description = Description, Icon = Icon, IsPublic = IsPublic,
                CategoryId = Category?.Id, Category = Category?.Text, CategoryKey = Category?.Key,
                ClientId = ClientId, ClientType = ClientType?.Text, ClientTypeId = ClientType?.Id,
                Status = Status?.Text, StatusId = Status?.Id, RequirePkce = RequirePkce, RequireConsent = RequireConsent,
                HasClientSecret = !String.IsNullOrEmpty(ClientSecretId), RedirectUriCount = RedirectUris?.Count ?? 0,
                ScopeCount = AllowedScopes?.Count ?? 0,
            };
        }

        ISummaryData ISummaryFactory.CreateSummary() => CreateSummary();

        public List<string> GetFormFields()
        {
            return new List<string>
            {
                nameof(Name), nameof(Key), nameof(Description), nameof(ClientId), nameof(ClientType), nameof(Status),
                nameof(ClientSecret), nameof(RedirectUris), nameof(PostLogoutRedirectUris), nameof(AllowedGrantTypes),
                nameof(AllowedScopes), nameof(AllowedResources), nameof(RequirePkce), nameof(RequireConsent),
                nameof(AccessTokenLifetimeMinutes), nameof(RefreshTokenLifetimeDays), nameof(LogoUrl),
                nameof(PrivacyPolicyUrl), nameof(TermsOfServiceUrl),
            };
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals
            {
                ConditionalFields = new List<string> { nameof(ClientSecret) },
                Conditionals = new List<FormConditional>
                {
                    new FormConditional { Field = nameof(ClientType), Value = ClientType_Confidential, VisibleFields = new List<string> { nameof(ClientSecret) } }
                }
            };
        }
    }

    [EntityDescription(Domains.SecurityDomain, OAuthClientResources.Names.OAuthClientApplications_Title,
        OAuthClientResources.Names.OAuthClientApplication_Help, OAuthClientResources.Names.OAuthClientApplication_Description,
        EntityDescriptionAttribute.EntityTypes.Summary, typeof(OAuthClientResources),
        ListUIUrl: "/security/oauth-clients", EditUIUrl: "/security/oauth-client/{id}", CreateUIUrl: "/security/oauth-client/add",
        SaveUrl: "/api/oauth/client", GetListUrl: "/api/oauth/clients", GetUrl: "/api/oauth/client/{id}",
        DeleteUrl: "/api/oauth/client/{id}", FactoryUrl: "/api/oauth/client/factory", Icon: "icon-ae-key")]
    public class OAuthClientApplicationSummary : SummaryData
    {
        public string ClientId { get; set; }
        public string ClientType { get; set; }
        public string ClientTypeId { get; set; }
        public string Status { get; set; }
        public string StatusId { get; set; }
        public bool RequirePkce { get; set; }
        public bool RequireConsent { get; set; }
        public bool HasClientSecret { get; set; }
        public int RedirectUriCount { get; set; }
        public int ScopeCount { get; set; }
    }

    [EntityDescription(Domains.SecurityDomain, OAuthClientResources.Names.OAuthClientSettingValue_Title,
        OAuthClientResources.Names.OAuthClientSettingValue_Help, OAuthClientResources.Names.OAuthClientSettingValue_Help,
        EntityDescriptionAttribute.EntityTypes.Dto, typeof(OAuthClientResources), FactoryUrl: "/api/oauth/client/value/factory")]
    public class OAuthClientSettingValue : IFormDescriptor, IValidateable
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        [FormField(LabelResource: OAuthClientResources.Names.OAuthClientSettingValue_Value,
            FieldType: FieldTypes.Text, ResourceType: typeof(OAuthClientResources), IsRequired: true, IsUserEditable: true)]
        public string Value { get; set; }

        [CustomValidator]
        public void Validate(ValidationResult result, Actions action)
        {
            if (String.IsNullOrWhiteSpace(Value)) result.AddUserError("Value is required.");
        }

        public List<string> GetFormFields() => new List<string> { nameof(Value) };
    }
}
