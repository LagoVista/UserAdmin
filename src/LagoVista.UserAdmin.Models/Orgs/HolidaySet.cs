using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.HolidaySet_Title, UserAdminResources.Names.HolidaySet_Help,
                          UserAdminResources.Names.HolidaySet_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class HolidaySet : UserAdminModelBase, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity
    {
        public HolidaySet()
        {
            Holidays = new List<ScheduledDowntime>();
        }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HolidaySet_Culture_Or_Country, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string CompanyOrCulture { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.HolidaySet_Holidays, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public List<ScheduledDowntime> Holidays { get; set; }

        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }

        public HolidaySetSummary CreateSummary()
        {
            return new HolidaySetSummary()
            {
                Description = Description,
                Id = Id,
                Name = Name,
                IsPublic = IsPublic,
                Key = Key
            };
        }
    }

    public class HolidaySetSummary : SummaryData
    {

    }
}
