using LagoVista.Core.Attributes;
using LagoVista.UserAdmin.Models.Resources;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.ScheduledDowntimePeriod_TItle, UserAdminResources.Names.ScheduledDowntimePeriod_Help,
                              UserAdminResources.Names.ScheduledDowntimePeriod_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class ScheduledDowntimePeriod
    {
        [FormField(LabelResource: UserAdminResources.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
         RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntimePeriod_Start, HelpResource: UserAdminResources.Names.ScheduledDowntimePeriod_Start_Help,
                ValidationRegEx: @"^[0-2]?[0-9]?[0-5]?[0-9]$", FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources))]
        public int Start { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntimePeriod_End, HelpResource: UserAdminResources.Names.ScheduledDowntimePeriod_End_Help,
        ValidationRegEx: @"^[0-2]?[0-9]?[0-5]?[0-9]$", FieldType: FieldTypes.Decimal, ResourceType: typeof(UserAdminResources))]
        public int End { get; set; }
    }
}
