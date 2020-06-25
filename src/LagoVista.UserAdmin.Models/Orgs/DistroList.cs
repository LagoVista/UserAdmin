using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.DistroList_Name
        , UserAdminResources.Names.DistroList_Help, UserAdminResources.Names.DistroList_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class DistroList : UserAdminModelBase, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity
    {
        public DistroList()
        {
            AppUsers = new List<EntityHeader>();
        }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key, RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType:FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }

        public List<EntityHeader> AppUsers { get; set; }

        [CustomValidator]
        public void Validate(ValidationResult result, Actions action)
        {
            if (AppUsers.Count == 0)
            {
                result.AddUserError("Must have at least one person on the distribution list.");
            }
        }

        public DistroListSummary CreateSummary()
        {
            return new DistroListSummary()
            {
                Description = Description,
                Id = Id,
                IsPublic = IsPublic,
                Key = Key,
                Name = Name
            };
        }
    }

    public class DistroListSummary : SummaryData
    {

    }
}
