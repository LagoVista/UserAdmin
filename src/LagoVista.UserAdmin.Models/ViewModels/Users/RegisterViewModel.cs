using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.Core.Interfaces;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.ViewModels.Users
{
    [EntityDescription(Domains.UserViewModels, UserAdminResources.Names.RegisterVM_Title, UserAdminResources.Names.RegisterVM_Help, UserAdminResources.Names.RegisterVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class RegisterViewModel : IValidateable, IFormDescriptor
    {
        public string AppId { get; set; }
        public string AppInstanceId { get; set; }
        public string ClientType { get; set; }
        public string DeviceId { get; set; }
        public string InviteId { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.AppUser_FirstName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string FirstName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_LastName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string LastName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Email { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_Password, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Password { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_ConfirmPassword, CompareTo: nameof(Password), CompareToMsgResource: UserAdminResources.Names.AppUser_PasswordConfirmPasswordMatch, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string ConfirmPassword { get; set; }

		public List<string> GetFormFields()
		{
            return new List<string>()
			{
				nameof(FirstName),
				nameof(LastName),
				nameof(Email),
				nameof(Password),
				nameof(ConfirmPassword),
			};
		}
	}
}