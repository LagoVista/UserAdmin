// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2a15b2533164e0fba5da785fc7b143d03bd60a18494a5a4d60fcb3ce58d9996b
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(
        Domains.UserDomain, UserAdminResources.Names.InviteUserVM_Title, UserAdminResources.Names.InviteUserVM_Help,
        UserAdminResources.Names.InviteUserVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources),

        ClusterKey: "invites", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Aux,
        IndexPriority: 30, IndexTagsCsv: "userdomain,invites,runtimeartifact,viewmodel")]
    public class InviteUserViewModel : IFormDescriptor
    {
        [FormField(LabelResource: UserAdminResources.Names.Common_EmailAddress, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.InviteUser_Name,  IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.InviteUser_Greeting_Label, FieldType: FieldTypes.MultiLineText, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Message { get; set; }

		public List<string> GetFormFields()
		{
            return new List<string>()
            {
                nameof(Name), 
                nameof(Email), 
                nameof(Message)
            };
		}
	}
}