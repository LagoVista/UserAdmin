using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.UserAdmin.Models.Apps
{
    [EntityDescription(Domains.MiscDomain, UserAdminResources.Names.ClientApp_Title, UserAdminResources.Names.ClientApp_Help, UserAdminResources.Names.ClientApp_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources))]
    public class ClientApp :  IOwnedEntity, IValidateable, IKeyedEntity, INoSQLEntity, IFormDescriptor
    {
        public ClientApp()
        {
            DeviceTypes = new ObservableCollection<EntityHeader>();
            DeviceConfigurations = new ObservableCollection<EntityHeader>();
        }

        [JsonProperty("id")]
        public String Id { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }


        public string DatabaseName { get; set; }
        public string EntityType { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key, RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public String Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_IsPublic, FieldType: FieldTypes.Bool, ResourceType: typeof(UserAdminResources))]
        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.ClientApp_AppAuthKey1, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: false)]
        public String AppAuthKeyPrimary { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ClientApp_AppAuthKey2, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: false)]
        public String AppAuthKeySecondary { get; set; }

        public EntityHeader ToEntityHeader()
        {
            return new EntityHeader()
            {
                Id = Id,
                Text = Name,
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(ClientApp.Name),
                nameof(ClientApp.Key),
                nameof(ClientApp.Description),
                nameof(ClientApp.DeploymentInstance),
                nameof(ClientApp.DeviceTypes),
                nameof(ClientApp.DeviceConfigurations),
            };
        }

        public ClientAppSummary CreateSummary()
        {
            return new ClientAppSummary()
            {
                Description = Description,
                Id = Id,
                IsPublic = IsPublic,
                Key = Key,
                Name = Name,
                InstanceId = DeploymentInstance.Id,
                InstanceName = DeploymentInstance.Text
            };
        }

        [FormField(LabelResource: UserAdminResources.Names.ClientApp_Instance, FieldType: FieldTypes.EntityHeaderPicker, WaterMark: UserAdminResources.Names.ClientApp_SelectInstance, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public EntityHeader DeploymentInstance { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ClientApp_DeviceTypes, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public ObservableCollection<EntityHeader> DeviceTypes { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ClientApp_DeviceConfigs, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources))]
        public ObservableCollection<EntityHeader> DeviceConfigurations { get; set; }
    }

    public class ClientAppSummary : SummaryData
    {
        public string InstanceId { get; set; }
        public string InstanceName { get; set; }
    }
}
