// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d8750846b622491444e360cfa998191a2066d98ecde373c10d4527cb25c589f7
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(
        Domains.SecurityDomain, UserAdminResources.Names.UiCateogry_Title, UserAdminResources.Names.UiCategory_Description,
        UserAdminResources.Names.UiCategory_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),

        FactoryUrl: "/api/module/uicategory/factory",

        ClusterKey: "ui", ModelType: EntityDescriptionAttribute.ModelTypes.Taxonomy, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Secondary,
        IndexPriority: 65, IndexTagsCsv: "securitydomain,ui,taxonomy,category")]
    public class UiCategory : IFormDescriptor
    {
        public UiCategory()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { set; get; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.UiCategory_Icon, IsRequired: true, FieldType: FieldTypes.Icon, WaterMark: UserAdminResources.Names.UiCategory_Icon_Select,
            ResourceType: typeof(UserAdminResources))]
        public string Icon { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Summary { get; set; }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                 nameof(Name),
                 nameof(Key),
                 nameof(Icon),
                 nameof(Summary),
            };
        }
    }
}
