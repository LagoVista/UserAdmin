// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: eec73d34ccdefd1d4fa928caf2fe58a76e2f2488dfdf8e58d8da405859068aca
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(
        Domains.AuthDomain, UserAdminResources.Names.ForgotPasswordVM_Title, UserAdminResources.Names.ForgotPasswordVM_Help,
        UserAdminResources.Names.ForgotPasswordVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources),

        ClusterKey: "login", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Restricted, IndexInclude: false, IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude,
        IndexPriority: 5, IndexTagsCsv: "authdomain,login,runtimeartifact")]

    public class ForgotPasswordViewModel : IValidateable
    {
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; }
    }
}
