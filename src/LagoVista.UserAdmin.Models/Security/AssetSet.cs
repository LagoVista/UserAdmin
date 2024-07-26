using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.AssetSet_Title, UserAdminResources.Names.AssetSet_Help, UserAdminResources.Names.AssetSet_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class AssetSet : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity
    {       
        [FormField(LabelResource: UserAdminResources.Names.Common_Description, FieldType: FieldTypes.MultiLineText, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AssetSet_IsRestricted, HelpResource: UserAdminResources.Names.AssetSet_IsRestricted_Help,  FieldType: FieldTypes.CheckBox, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public bool IsRestricted { get; set; }


        public EntityHeader Subscription { get; set; }
        
        public AssetSetSummary CreateSummary()
        {
            return new AssetSetSummary()
            {
                Id = Id,
                Name = Name,
                IsPublic = false,
                Key = Key
            };
        }
    }

    public class AssetSetSummary : SummaryData
    {

    }

}
