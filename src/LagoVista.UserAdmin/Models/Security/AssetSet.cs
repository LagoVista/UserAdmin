using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Resources;

namespace LagoVista.UserAdmin.Models.Security
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.AssetSet_Title, UserAdminResources.Names.AssetSet_Help, UserAdminResources.Names.AssetSet_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class AssetSet : UserAdminModelBase, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity
    {

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Name, IsRequired: true, ResourceType: typeof(Resources.UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key, RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Description, FieldType: FieldTypes.MultiLineText, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.AssetSet_IsRestricted, HelpResource: UserAdminResources.Names.AssetSet_IsRestricted_Help,  FieldType: FieldTypes.CheckBox, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public bool IsRestricted { get; set; }


        public EntityHeader Subscription { get; set; }
        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }

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
