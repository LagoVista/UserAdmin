using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public enum DowntimeTypes
    {
        [EnumLabel(ScheduledDowntime.DownTimeType_Holiday, UserAdminResources.Names.DownTimeType_Holiday, typeof(UserAdminResources))]
        Holiday,

        [EnumLabel(ScheduledDowntime.DownTimeType_Maintenance, UserAdminResources.Names.DownTimeType_ScheduledMaintenance, typeof(UserAdminResources))]
        Maintenance,

        [EnumLabel(ScheduledDowntime.DownTimeType_Admin, UserAdminResources.Names.DownTimeType_Admin, typeof(UserAdminResources))]
        Admin,

        [EnumLabel(ScheduledDowntime.DownTimeType_Other, UserAdminResources.Names.DownTimeType_Other, typeof(UserAdminResources))]
        Other,
    }

    public enum ScheduleTypes
    {
        [EnumLabel(ScheduledDowntime.ScheduleType_SpecificDate, UserAdminResources.Names.ScheduledDowntime_ScheduleType_SpecificDate, typeof(UserAdminResources))]
        SpecificDate,
       
        [EnumLabel(ScheduledDowntime.ScheduleType_ScheduleDayMonth, UserAdminResources.Names.ScheduledDowntime_ScheduleType_SpecificMonthDay, typeof(UserAdminResources))]
        SpecificMonthDay,
        
        [EnumLabel(ScheduledDowntime.ScheduleType_ScheduleDayOfWeekInMonth, UserAdminResources.Names.ScheduledDowntime_ScheduleType_Week_DayOfWeek, typeof(UserAdminResources))]
        SpecificDayOfWeekInWeekOfMonth
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.ScheduledDowntime_Title, UserAdminResources.Names.ScheduledDowntime_Help, 
                            UserAdminResources.Names.ScheduledDowntime_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources))]
    public class ScheduledDowntime : UserAdminModelBase, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity, ICloneable
    {
        public const string DownTimeType_Holiday = "holiday";
        public const string DownTimeType_Maintenance = "maintenance";
        public const string DownTimeType_Admin = "admin";
        public const string DownTimeType_Other = "other";

        public const string ScheduleType_SpecificDate = "specificdate";
        public const string ScheduleType_ScheduleDayMonth = "specificdatemonth";
        public const string ScheduleType_ScheduleDayOfWeekInMonth = "specificdayofweekofmomth";

        public string OriginalId { get; set; }

        public bool IsPublic { get; set; }
        public EntityHeader OwnerOrganization { get; set; }
        public EntityHeader OwnerUser { get; set; }

        public ScheduledDowntime()
        {
            Periods = new List<ScheduledDowntimePeriod>();
        }
        
        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key, 
            RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }
  
        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_ScheduleType, EnumType: (typeof(ScheduleTypes)), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources),
                WaterMark: UserAdminResources.Names.ScheduledDowntime_ScheduleType_Select, IsRequired: true, IsUserEditable: true)]
        public EntityHeader<ScheduleTypes> ScheduleType { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_DowntimeType, EnumType: (typeof(DowntimeTypes)), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources), 
            WaterMark: UserAdminResources.Names.DownTimeType_Select, IsRequired: true, IsUserEditable: true)]
        public EntityHeader<DowntimeTypes> DowntimeTypes { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_Year, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public int? Year { get; set; }
      
        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_Month, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public int? Month { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_Day, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public int? Day { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_DayOfWeek, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader DayOfWeek { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.ScheudledDowntime_Week, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public int? Week { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_AllDay, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public bool AllDay { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_Periods, FieldType: FieldTypes.ChildList, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public List<ScheduledDowntimePeriod> Periods { get; set; }

        [CustomValidator]
        public void Validate(ValidationResult result)
        {

        }

        public ScheduledDowntimeSummary CreateSummary()
        {
            return new ScheduledDowntimeSummary()
            {
                Description = Description,
                Id = Id,
                Name = Name,
                IsPublic = IsPublic,
                Key = Key
            };
        }
    }

    public class ScheduledDowntimeSummary: SummaryData
    {

    }
}
