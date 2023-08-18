using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    [EntityDescription(Domains.OrganizationViewModels, UserAdminResources.Names.InviteUserVM_Title, UserAdminResources.Names.InviteUserVM_Help, UserAdminResources.Names.InviteUserVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
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