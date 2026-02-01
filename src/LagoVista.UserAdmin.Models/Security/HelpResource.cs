// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4f105422577276417ed3b0be7331f4a6036d7248da25b3fffd28869fc6574a5a
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(
        Domains.SecurityDomain, UserAdminResources.Names.HelpResource_Title, UserAdminResources.Names.HelpResource_Description,
        UserAdminResources.Names.HelpResource_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),

        ClusterKey: "help", ModelType: EntityDescriptionAttribute.ModelTypes.Document, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Primary,
        IndexPriority: 80, IndexTagsCsv: "securitydomain,help,document")]
    public class HelpResource
    {
        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HelpResource_Guide, FieldType: FieldTypes.EntityHeaderPicker, WaterMark: UserAdminResources.Names.HelpResource_SelectGuide, ResourceType: typeof(UserAdminResources))]
        public EntityHeader HelpResourceGuide { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HelpResource_Link, FieldType: FieldTypes.WebLink, ResourceType: typeof(UserAdminResources))]
        public string Link { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HelpResource_Summary, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Summary { get; set; }
    }
}
