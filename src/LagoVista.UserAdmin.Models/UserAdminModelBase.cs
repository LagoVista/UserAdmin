using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models
{
    public abstract class UserAdminModelBase : ModelBase, IIDEntity, IAuditableEntity, INoSQLEntity
    {
        public String EntityType { get; set; }
        public String DatabaseName { get; set; }


        [JsonProperty("id")]
        [FormField(LabelResource: UserAdminResources.Names.Common_Id, ResourceType: typeof(UserAdminResources))]
        public String Id { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_CreationDate, FieldType: FieldTypes.JsonDateTime, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: false)]
        public String CreationDate { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_CreatedBy, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: false)]
        public EntityHeader CreatedBy { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_LastUpdatedDate, FieldType: FieldTypes.JsonDateTime, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: false)]
        public String LastUpdatedDate { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_LastUpdatedBy, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: false)]
        public EntityHeader LastUpdatedBy { get; set; }

        public List<EntityChangeSet> AuditHistory { get; set; } = new List<EntityChangeSet>();

        public bool IsDeleted { get; set; }
        public EntityHeader DeletedBy { get; set; }
        public string DeletionDate { get; set; }
        public bool IsDeprecated { get; set; }
        public EntityHeader DeprecatedBy { get; set; }
        public string DeprecationDate { get; set; }
        public string DeprecationNotes { get; set; }

    }
}
