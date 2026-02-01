// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7c4a6d75009494cd9dccd8a325755e91340c87622714cf03a6bee8648560c5ef
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.ViewModels.VerifyIdentity
{
    [EntityDescription(
        Domains.SecurityDomain, UserAdminResources.Names.VerifyPhoneNumberVM_Title, UserAdminResources.Names.VerifyPhoneNumberVM_Help,
        UserAdminResources.Names.VerifyPhoneNumberVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources),

        ClusterKey: "password", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "securitydomain,password,runtimeartifact,restricted")]

    public class VerifyPhoneNumberViewModel : IValidateable
    {
        [FormField(FieldType: FieldTypes.Decimal, LabelResource: UserAdminResources.Names.AppUser_PhoneVerificationCode, ResourceType: typeof(UserAdminResources))]
        public string Code { get; set; }

        [FormField(IsRequired: true, LabelResource: UserAdminResources.Names.AppUser_PhoneNumber, FieldType: FieldTypes.Phone, ResourceType: typeof(UserAdminResources))]
        public string PhoneNumber { get; set; }
    }
}
