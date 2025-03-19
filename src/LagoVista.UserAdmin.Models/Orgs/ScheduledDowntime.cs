using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    public enum DowntimeTypes
    {
        [EnumLabel(ScheduledDowntime.DownTimeType_Holiday, UserAdminResources.Names.DownTimeType_Holiday, typeof(UserAdminResources), SortOrder: 2)]
        Holiday,

        [EnumLabel(ScheduledDowntime.DownTimeType_Maintenance, UserAdminResources.Names.DownTimeType_ScheduledMaintenance, typeof(UserAdminResources), SortOrder: 3)]
        Maintenance,

        [EnumLabel(ScheduledDowntime.DownTimeType_Admin, UserAdminResources.Names.DownTimeType_Admin, typeof(UserAdminResources), SortOrder: 1)]
        Admin,

        [EnumLabel(ScheduledDowntime.DownTimeType_Other, UserAdminResources.Names.DownTimeType_Other, typeof(UserAdminResources), SortOrder: 5)]
        Other,

        [EnumLabel(ScheduledDowntime.DownTimeType_WorkWeek, UserAdminResources.Names.DownTimeType_WorkWeek, typeof(UserAdminResources), SortOrder: 4)]
        WorkWeek,
    }

    public enum DaysOfWeek
    {
        [EnumLabel(ScheduledDowntime.DayOfWeek_Sunday, UserAdminResources.Names.DayOfWeek_Sunday, typeof(UserAdminResources))]
        Sunday,
        [EnumLabel(ScheduledDowntime.DayOfWeek_Monday, UserAdminResources.Names.DayOfWeek_Monday, typeof(UserAdminResources))]
        Monday,
        [EnumLabel(ScheduledDowntime.DayOfWeek_Tuesday, UserAdminResources.Names.DayOfWeek_Tuesday, typeof(UserAdminResources))]
        Tuesday,
        [EnumLabel(ScheduledDowntime.DayOfWeek_Wednesday, UserAdminResources.Names.DayOfWeek_Wednesday, typeof(UserAdminResources))]
        Wednesday,
        [EnumLabel(ScheduledDowntime.DayOfWeek_Thursday, UserAdminResources.Names.DayOfWeek_Thursday, typeof(UserAdminResources))]
        Thursday,
        [EnumLabel(ScheduledDowntime.DayOfWeek_Friday, UserAdminResources.Names.DayOfWeek_Friday, typeof(UserAdminResources))]
        Friday,
        [EnumLabel(ScheduledDowntime.DayOfWeek_Saturday, UserAdminResources.Names.DayOfWeek_Saturday, typeof(UserAdminResources))]
        Saturday,
    }

    public enum Months
    {
        [EnumLabel(ScheduledDowntime.Month_January, UserAdminResources.Names.Month_January, typeof(UserAdminResources))]
        January,
        [EnumLabel(ScheduledDowntime.Month_February, UserAdminResources.Names.Month_February, typeof(UserAdminResources))]
        February,
        [EnumLabel(ScheduledDowntime.Month_March, UserAdminResources.Names.Month_March, typeof(UserAdminResources))]
        March,
        [EnumLabel(ScheduledDowntime.Month_April, UserAdminResources.Names.Month_April, typeof(UserAdminResources))]
        April,
        [EnumLabel(ScheduledDowntime.Month_May, UserAdminResources.Names.Month_May, typeof(UserAdminResources))]
        May,
        [EnumLabel(ScheduledDowntime.Month_June, UserAdminResources.Names.Month_June, typeof(UserAdminResources))]
        June,
        [EnumLabel(ScheduledDowntime.Month_July, UserAdminResources.Names.Month_July, typeof(UserAdminResources))]
        July,
        [EnumLabel(ScheduledDowntime.Month_August, UserAdminResources.Names.Month_August, typeof(UserAdminResources))]
        August,
        [EnumLabel(ScheduledDowntime.Month_September, UserAdminResources.Names.Month_September, typeof(UserAdminResources))]
        September,
        [EnumLabel(ScheduledDowntime.Month_October, UserAdminResources.Names.Month_October, typeof(UserAdminResources))]
        October,
        [EnumLabel(ScheduledDowntime.Month_November, UserAdminResources.Names.Month_November, typeof(UserAdminResources))]
        November,
        [EnumLabel(ScheduledDowntime.Month_December, UserAdminResources.Names.Month_December, typeof(UserAdminResources))]
        December,
    }

    public enum ScheduleTypes
    {
        [EnumLabel(ScheduledDowntime.ScheduleType_SpecificDate, UserAdminResources.Names.ScheduledDowntime_ScheduleType_SpecificDate, typeof(UserAdminResources))]
        SpecificDate,

        [EnumLabel(ScheduledDowntime.ScheduleType_DayInMonth, UserAdminResources.Names.ScheduledDowntime_ScheduleType_DayInMonth, typeof(UserAdminResources))]
        DayInMonth,

        [EnumLabel(ScheduledDowntime.ScheduleType_DayOfWeekOfWeekInMonth, UserAdminResources.Names.ScheduledDowntime_ScheduleType_DayOfWeekOfWeekInMonth, typeof(UserAdminResources))]
        DayOfWeekOfWeekInMonth,

        [EnumLabel(ScheduledDowntime.ScheduleType_DayOfWeek, UserAdminResources.Names.ScheduledDowntime_ScheduleType_DayOfWeek, typeof(UserAdminResources))]
        DayOfWeek,


        [EnumLabel(ScheduledDowntime.ScheduleType_WeekDay, UserAdminResources.Names.ScheduleType_WeekDay, typeof(UserAdminResources))]
        WeekDay,


        [EnumLabel(ScheduledDowntime.ScheduleType_WeekEnd, UserAdminResources.Names.ScheduleType_WeekEnd, typeof(UserAdminResources))]
        WeekEnd,
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.ScheduledDowntime_Title, UserAdminResources.Names.ScheduledDowntime_Help,
                            UserAdminResources.Names.ScheduledDowntime_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),
                            ListUIUrl: "/organization/downtimes", EditUIUrl: "/organization/downtime/{id}", CreateUIUrl: "/organization/downtime/add", Icon: "icon-ae-call-time",
                            SaveUrl: "/api/scheduleddowntime", GetListUrl: "/api/scheduleddowntimes", GetUrl: "/api/scheduleddowntime/{id}",
                            DeleteUrl: "/api/scheduleddowntime/{id}", FactoryUrl: "/api/scheduleddowntime/factory")]
    public class ScheduledDowntime : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity, ICloneable,
                                    IFormDescriptor, IFormConditionalFields, ISummaryFactory, ICategorized
    {
        public const string DownTimeType_Holiday = "holiday";
        public const string DownTimeType_Maintenance = "maintenance";
        public const string DownTimeType_Admin = "admin";
        public const string DownTimeType_Other = "other";
        public const string DownTimeType_WorkWeek = "workweek";

        public const string ScheduleType_SpecificDate = "specificdate";
        public const string ScheduleType_DayOfWeek = "dayofweek";
        public const string ScheduleType_DayInMonth = "dayinmonth";
        public const string ScheduleType_DayOfWeekOfWeekInMonth = "dayofweekofweekinmomth";
        public const string ScheduleType_WeekDay = "weekday";
        public const string ScheduleType_WeekEnd = "weekend";

        public const string Month_January = "january";
        public const string Month_February = "febrary";
        public const string Month_March = "march";
        public const string Month_April = "april";
        public const string Month_May = "may";
        public const string Month_June = "june";
        public const string Month_July = "july";
        public const string Month_August = "august";
        public const string Month_September = "september";
        public const string Month_October = "october";
        public const string Month_November = "november";
        public const string Month_December = "december";

        public const string DayOfWeek_Sunday = "sunday";
        public const string DayOfWeek_Monday = "monday";
        public const string DayOfWeek_Tuesday = "tuesday";
        public const string DayOfWeek_Wednesday = "wednesday";
        public const string DayOfWeek_Thursday = "thursday";
        public const string DayOfWeek_Friday = "friday";
        public const string DayOfWeek_Saturday = "saturday";

        public string OriginalId { get; set; }

        public ScheduledDowntime()
        {
            Periods = new List<ScheduledDowntimePeriod>();
            AllDay = true;
            Icon = "icon-ae-call-time";
        }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Description, FieldType: FieldTypes.MultiLineText, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_ScheduleType, SortEnums:true, EnumType: (typeof(ScheduleTypes)), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources),
                WaterMark: UserAdminResources.Names.ScheduledDowntime_ScheduleType_Select, IsRequired: true, IsUserEditable: true)]
        public EntityHeader<ScheduleTypes> ScheduleType { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_DowntimeType, SortEnums:true, EnumType: (typeof(DowntimeTypes)), FieldType: FieldTypes.Picker, ResourceType: typeof(UserAdminResources),
            WaterMark: UserAdminResources.Names.DownTimeType_Select, IsRequired: true, IsUserEditable: true)]
        public EntityHeader<DowntimeTypes> DowntimeType { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_Year, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public int? Year { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_Month, WaterMark: UserAdminResources.Names.Month_Select, FieldType: FieldTypes.Picker, EnumType: typeof(Months), ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader<Months> Month { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_Day, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public int? Day { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_DayOfWeek, WaterMark: UserAdminResources.Names.ScheduledDowntime_DayOfWeek_Select, FieldType: FieldTypes.Picker, EnumType: typeof(DaysOfWeek), ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public EntityHeader<DaysOfWeek> DayOfWeek { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheudledDowntime_Week, FieldType: FieldTypes.Integer, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public int? Week { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_AllDay, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public bool AllDay { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntime_Periods, FieldType: FieldTypes.ChildListInline, FactoryUrl: "/api/scheduleddowntime/period/factory", ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
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
                Icon = Icon,
                IsPublic = IsPublic,
                Key = Key,
                CategoryId = Category?.Id,
                Category = Category?.Text,
                CategoryKey = Category?.Key,
                DowntimeType = DowntimeType.Text,
                DowntimeTypeId = DowntimeType.Id,
                ScheduleType = ScheduleType.Text,
                ScheduleTypeId = ScheduleType.Id
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Category),
                nameof(ScheduleType),
                nameof(DowntimeType),
                nameof(Year),
                nameof(Month),
                nameof(Day),
                nameof(DayOfWeek),
                nameof(Week),
                nameof(AllDay),
                nameof(Periods),
            };
        }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(Periods), nameof(Year), nameof(Month), nameof(Day), nameof(DayOfWeek), nameof(Week) },
                Conditionals = new List<FormConditional>()
                {
                    new FormConditional()
                    {
                        Field = nameof(AllDay),
                        Value = "false",
                        VisibleFields = new List<string>() {nameof(Periods)}
                    },
                    new FormConditional()
                    {
                        Field = nameof(ScheduleType),
                        Value = ScheduleType_DayInMonth,
                        RequiredFields = new List<string>() { nameof(Month), nameof(Day) },
                        VisibleFields = new List<string>() { nameof(Month), nameof(Day) }
                    },
                    new FormConditional()
                    {
                        Field = nameof(ScheduleType),
                        Value = ScheduleType_DayOfWeek,
                        RequiredFields = new List<string>() { nameof(DayOfWeek) },
                        VisibleFields = new List<string>() { nameof(DayOfWeek) }
                    },
                    new FormConditional()
                    {
                        Field = nameof(ScheduleType),
                        Value = ScheduleType_DayOfWeekOfWeekInMonth,
                        RequiredFields = new List<string>() { nameof(Month), nameof(Week), nameof(DayOfWeek) },
                        VisibleFields = new List<string>() { nameof(Month), nameof(Week), nameof(DayOfWeek) }
                    },
                    new FormConditional()
                    {
                        Field = nameof(ScheduleType),
                        Value = ScheduleType_SpecificDate,
                        RequiredFields = new List<string>() { nameof(Day), nameof(Month), nameof(Year) },
                        VisibleFields = new List<string>() { nameof(Day), nameof(Month), nameof(Year) }
                    }
                }
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.ScheduledDowntimes_Title, UserAdminResources.Names.ScheduledDowntime_Help,
                          UserAdminResources.Names.ScheduledDowntime_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources),
                          ListUIUrl: "/organization/downtimes", EditUIUrl: "/organization/downtime/{id}", CreateUIUrl: "/organization/downtime/add",
                          SaveUrl: "/api/scheduleddowntime", GetListUrl: "/api/scheduleddowntimes", GetUrl: "/api/scheduleddowntime/{id}", Icon: "icon-ae-call-time",
                          DeleteUrl: "/api/scheduleddowntime/{id}", FactoryUrl: "/api/scheduleddowntime/factory")]
    public class ScheduledDowntimeSummary : SummaryData
    {
        public string ScheduleType { get; set; }
        public string ScheduleTypeId { get; set; }

        public string DowntimeType { get;  set; }
        public string DowntimeTypeId { get; set; }
    }
}
