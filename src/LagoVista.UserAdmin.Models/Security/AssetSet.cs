// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0eba9ef484ae1bd3834f70546d0aede0402b8236d955855a9bb1e1edb5e7b358
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.AssetSet_Title, UserAdminResources.Names.AssetSet_Help,
        UserAdminResources.Names.AssetSet_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources),

        ClusterKey: "access", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration, Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime,
        Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Primary,
        IndexPriority: 85, IndexTagsCsv: "organizationdomain,access,configuration,assetset")]
    public class AssetSet : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity
    {       
        [FormField(LabelResource: UserAdminResources.Names.AssetSet_IsRestricted, HelpResource: UserAdminResources.Names.AssetSet_IsRestricted_Help,  FieldType: FieldTypes.CheckBox, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public bool IsRestricted { get; set; }

        public EntityHeader Subscription { get; set; }
        
        public AssetSetSummary CreateSummary()
        {
            var summary = new AssetSetSummary();
            summary.Populate(this);
            return summary;
        }
    }


    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.AssetSet_Title, UserAdminResources.Names.AssetSet_Help,
        UserAdminResources.Names.AssetSet_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class AssetSetSummary : SummaryData
    {

    }

}
