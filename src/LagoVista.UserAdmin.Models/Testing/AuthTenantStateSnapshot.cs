using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Testing
{

    public enum SetCondition
    {
        [EnumLabel(AuthTenantStateSnapshot.SetCondition_Set, UserAdminResources.Names.SetCondition_Set, typeof(UserAdminResources))]
        Set,
        [EnumLabel(AuthTenantStateSnapshot.SetCondition_NotSet, UserAdminResources.Names.SetCondition_NotSet, typeof(UserAdminResources))]
        NotSet,
        [EnumLabel(AuthTenantStateSnapshot.SetCondition_DontCare, UserAdminResources.Names.SetCondition_DontCare, typeof(UserAdminResources))]
        DontCare
    }

    /// <summary>
    /// JSON-friendly snapshot of the auth + tenant/org state used for ceremony preconditions and postconditions.
    /// This is intentionally a POCO (no serializer-specific attributes).
    ///
    /// Design notes:
    /// - Use nullable booleans so the DSL can specify only the fields it cares about.
    /// - Keep identifiers as strings (Id/ToId pattern) for cross-process compatibility.
    /// </summary>


    [EntityDescription(
        Domains.MiscDomain,
        UserAdminResources.Names.AuthTenantStateSnapshot_Title,
        UserAdminResources.Names.AuthTenantStateSnapshot_Help,
        UserAdminResources.Names.AuthTenantStateSnapshot_Description,
        EntityDescriptionAttribute.EntityTypes.SimpleModel,
        typeof(UserAdminResources),
        FactoryUrl: "/api/sys/testing/auth/usersnapshot/factory")]

    public class AuthTenantStateSnapshot : IFormDescriptor, IValidateable
    {
        public const string SetCondition_Set = "set";
        public const string SetCondition_NotSet = "notset";
        public const string SetCondition_DontCare = "dontcare";

        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_BelongsToOrg, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_BelongsToOrg_Help, 
            FieldType: FieldTypes.Picker, EnumType: typeof(SetCondition), ResourceType: typeof(UserAdminResources))]
        public EntityHeader<SetCondition> BelongsToOrg { get; set; } = EntityHeader<SetCondition>.Create(SetCondition.DontCare);

        /* Auth flags */
        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_EmailConfirmed, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_EmailConfirmed_Help, EnumType: typeof(SetCondition), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<SetCondition> EmailConfirmed { get; set; } = EntityHeader<SetCondition>.Create(SetCondition.DontCare);

        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_PhoneNumberConfirmed, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_PhoneNumberConfirmed_Help, EnumType: typeof(SetCondition), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<SetCondition> PhoneNumberConfirmed { get; set; } = EntityHeader<SetCondition>.Create(SetCondition.DontCare);

        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_TwoFactorEnabled, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_TwoFactorEnabled_Help, EnumType: typeof(SetCondition), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<SetCondition> TwoFactorEnabled { get; set; } = EntityHeader<SetCondition>.Create(SetCondition.DontCare);

        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_IsAccountDisabled, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_IsAccountDisabled_Help, EnumType: typeof(SetCondition), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<SetCondition> IsAccountDisabled { get; set; } = EntityHeader<SetCondition>.Create(SetCondition.DontCare);

        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_IsAnonymous, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_IsAnonymous_Help, EnumType: typeof(SetCondition), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<SetCondition> IsAnonymous { get; set; } = EntityHeader<SetCondition>.Create(SetCondition.DontCare);

        /* UX */
        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_ShowWelcome, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_ShowWelcome_Help, EnumType:typeof(SetCondition), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<SetCondition> ShowWelcome { get; set; } = EntityHeader<SetCondition>.Create(SetCondition.DontCare);

        /* Factors / providers */
        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_ExternalLoginProviders, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_ExternalLoginProviders_Help, FieldType: FieldTypes.StringList, ResourceType: typeof(UserAdminResources))]
        public List<string> ExternalLoginProviders { get; set; } = new List<string>();

        /* MFA freshness */
        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_LastMfaDateTimeUtc, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_LastMfaDateTimeUtc_Help, FieldType: FieldTypes.DateTime, ResourceType: typeof(UserAdminResources))]
        public string LastMfaDateTimeUtc { get; set; }

        /* Setup helpers */
        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_EnsureUserExists, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_EnsureUserExists_Help, EnumType: typeof(SetCondition), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<SetCondition> EnsureUserExists { get; set; } = EntityHeader<SetCondition>.Create(SetCondition.DontCare);

        [FormField(LabelResource: UserAdminResources.Names.AuthTenantStateSnapshot_EnsureUserDoesNotExist, HelpResource: UserAdminResources.Names.AuthTenantStateSnapshot_EnsureUserDoesNotExist_Help, EnumType: typeof(SetCondition), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader<SetCondition> EnsureUserDoesNotExist { get; set; } = EntityHeader<SetCondition>.Create(SetCondition.DontCare);

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(EnsureUserExists),
                nameof(EnsureUserDoesNotExist),
                nameof(BelongsToOrg),
                nameof(EmailConfirmed),
                nameof(PhoneNumberConfirmed),
                nameof(TwoFactorEnabled),
                nameof(IsAccountDisabled),
                nameof(IsAnonymous),
                nameof(ShowWelcome),
                nameof(LastMfaDateTimeUtc),
                nameof(ExternalLoginProviders)
            };
        }
    }

}
