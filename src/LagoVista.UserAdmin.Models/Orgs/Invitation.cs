using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Invitation_Title, UserAdminResources.Names.Invitation_Help, UserAdminResources.Names.Invitation_Description, 
            EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class Invitation : TableStorageEntity, IValidateable
    {
        public enum StatusTypes
        {
            New,
            Sent,
            Resent,
            Accepted,
            Declined,
            Revoked,
            Replaced
        }

        public String LinkId { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_EmailAddress, IsRequired: true, FieldType: FieldTypes.Email, ResourceType: typeof(UserAdminResources))]
        public String Email { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.InviteUser_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public String Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.InviteUser_Greeting_Message, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public String Message { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.InviteUser_InvitedByName, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public String InvitedByName { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.InviteUser_InvitedById, IsRequired: true, FieldType: FieldTypes.RowId, ResourceType: typeof(UserAdminResources))]
        public String InvitedById { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Organization_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public String OrganizationName { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Id, IsRequired: true, FieldType: FieldTypes.RowId, ResourceType: typeof(UserAdminResources))]
        public String OrganizationId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [FormField(LabelResource: UserAdminResources.Names.InviteUser_Status, IsRequired: true, FieldType: FieldTypes.JsonDateTime, ResourceType: typeof(UserAdminResources))]
        public StatusTypes Status { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Id, IsRequired: true, FieldType: FieldTypes.JsonDateTime, ResourceType: typeof(UserAdminResources))]
        public String DateSent { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Id, FieldType: FieldTypes.JsonDateTime, ResourceType: typeof(UserAdminResources))]
        public String DateAccepted { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Id, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool Accepted { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Id, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool Declined { get; set; }

        public bool IsActive()
        {
            return Status == StatusTypes.Replaced || Status == StatusTypes.Resent || Status == StatusTypes.Sent;
        }
    }
}
