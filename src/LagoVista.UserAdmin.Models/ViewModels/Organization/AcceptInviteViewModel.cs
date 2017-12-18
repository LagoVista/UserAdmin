using LagoVista.Core.Attributes;
using System;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.ViewModels.Organization
{
    public enum AcceptTypes
    {
        Login,
        Register
    }

    [EntityDescription(Domains.UserViewModels, UserAdminResources.Names.AcceptInviteVM_Title, UserAdminResources.Names.AcceptInviteVM_Help, UserAdminResources.Names.AcceptInviteVM_Description, EntityDescriptionAttribute.EntityTypes.ViewModel, typeof(UserAdminResources))]
    public class AcceptInviteViewModel
    {
        [FormField(FieldType: FieldTypes.Hidden, IsRequired: true)]
        public string InviteId { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AppUser_FirstName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String RegisterFirstName { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.AppUser_LastName, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String RegisterLastName { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public String RegisterEmail { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Password, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string RegisterPassword { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.AppUser_ConfirmPassword, CompareTo: nameof(RegisterPassword), CompareToMsgResource: UserAdminResources.Names.AppUser_PasswordConfirmPasswordMatch, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string RegisterConfirmPassword { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.AppUser_RememberMe, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool RegisterRememberMe { get; set; }



        [FormField(LabelResource: UserAdminResources.Names.AppUser_Email, FieldType: FieldTypes.Email, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string LoginEmail { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.AppUser_Password, FieldType: FieldTypes.Password, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string LoginPassword { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.AppUser_RememberMe, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool LoginRememberMe { get; set; }



        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String OrganizationId { get; set; }
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String OrganizationName { get; set; }


        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String InvitedById { get; set; }
        [FormField(IsRequired: true, FieldType: FieldTypes.Hidden)]
        public String InvitedByName { get; set; }

         
        public bool Active { get; set; }


        public EntityHeader GetInvitedByEntityHeader() { return EntityHeader.Create(InvitedById, InvitedByName); }

        public EntityHeader GetOrgEntityHeader() { return EntityHeader.Create(OrganizationId, OrganizationName); }

        public static AcceptInviteViewModel CreateFromInvite(Invitation invite)
        {
            return new AcceptInviteViewModel()
            {
                Active = !invite.Accepted && !invite.Declined && 
                    (invite.Status == Invitation.StatusTypes.New || invite.Status == Invitation.StatusTypes.Sent),
                InviteId = invite.RowKey,
                InvitedById = invite.InvitedById,
                InvitedByName = invite.InvitedByName,
                OrganizationId = invite.OrganizationId,
                OrganizationName = invite.OrganizationName,
            };
        }
    }
}
