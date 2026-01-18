// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8cd74b2237f110e527cdf5bd03341df9bb920804a62a4356867a921340f7aac5
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Calendar
{
    public enum CalendarEventTypes
    {
        [EnumLabel(CalendarEvent.CalendarEventType_Networking, UserAdminResources.Names.CalendarEventType_Networking, typeof(UserAdminResources))]
        Networking,

        [EnumLabel(CalendarEvent.CalendarEventType_UserGroup, UserAdminResources.Names.CalendarEventType_UserGroup, typeof(UserAdminResources))]
        UserGroup,

        [EnumLabel(CalendarEvent.CalendarEventType_OutOfOffice, UserAdminResources.Names.CalendarEventType_OutOfOffice, typeof(UserAdminResources))]
        OutOfOffice,

        [EnumLabel(CalendarEvent.CalendarEventType_Holiday, UserAdminResources.Names.CalendarEventType_Holiday, typeof(UserAdminResources))]
        Holiday,

        [EnumLabel(CalendarEvent.CalendarEventType_Other, UserAdminResources.Names.CalendarEventType_Other, typeof(UserAdminResources))]
        Other,
    }

    [EntityDescription(Domains.MiscDomain, UserAdminResources.Names.Calendar_ObjectTitle, UserAdminResources.Names.Calendar_ObjectDescription, UserAdminResources.Names.Calendar_ObjectDescription,
        EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),
        ListUIUrl: "/business/calendar", 
        SaveUrl: "/api/calendar/event", GetListUrl:"/api/calendar/month/{year}/{month}", FactoryUrl: "/api/calendar/event/factory", DeleteUrl: "/api/calendar/event/{id}", GetUrl: "/api/calendar/event/{id}")]
    public class CalendarEvent : UserAdminModelBase, IKeyedEntity, INamedEntity, IOwnedEntity, IValidateable
    {
        public const string CalendarEventType_Networking = "networking";
        public const string CalendarEventType_UserGroup = "usergroup";
        public const string CalendarEventType_OutOfOffice = "outofoffice";
        public const string CalendarEventType_Holiday = "holiday";
        public const string CalendarEventType_Other = "other";

        public CalendarEvent()
        {
            Id = Guid.NewGuid().ToId();
            Key = Guid.NewGuid().ToId().ToLower();
        }

        [FormField(LabelResource: UserAdminResources.Names.Calendar_Date, FieldType: FieldTypes.Date, ResourceType: typeof(UserAdminResources), IsRequired: true, IsUserEditable: true)]
        public string Date { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Calendar_AllDay, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public bool AllDay { get; set; }


        public CalendarEventSummary CreateSummary()
        {
            var summary = new CalendarEventSummary()
            {
                AllDay = AllDay,
                Name = Name, 
                Id = Id,
                Date = Date,
                StartTime = StartTime,
                EndTime = EndTime,
                EventType = EventType.Text,
                EventTypeKey = EventType.Key
            };
            
            return summary;
        }

        [FormField(LabelResource: UserAdminResources.Names.Calendar_Start, FieldType: FieldTypes.Time, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public string StartTime { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Calendar_End, FieldType: FieldTypes.Time, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public string EndTime { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Calendar_EventLink, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public string EventLink { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Calendar_EventType, ResourceType: typeof(UserAdminResources), IsRequired: true, FieldType: FieldTypes.Picker, EnumType: typeof(CalendarEventTypes),
            WaterMark: UserAdminResources.Names.Calendar_EventType_Select)]
        public EntityHeader<CalendarEventTypes> EventType { get; set; }
 
        [CustomValidator]
        public void Validate(ValidationResult result)
        {
            if (AllDay)
            {
                StartTime = null;
                EndTime = null;
            }
            else
            {
                if (String.IsNullOrEmpty(StartTime)) result.AddUserError("Start time is required if not an All Day event.");
                if (String.IsNullOrEmpty(EndTime)) result.AddUserError("End time is required if not an All Day event.");

                if (!String.IsNullOrEmpty(StartTime) && !StartTime.IsValidTime()) result.AddUserError("Invalid start time.");
                if (!String.IsNullOrEmpty(EndTime) && !EndTime.IsValidTime()) result.AddUserError("Invalid End time.");

                if (!String.IsNullOrEmpty(StartTime) && !String.IsNullOrEmpty(EndTime))
                {
                    if (StartTime.CompareTo(EndTime) == 1)
                        result.AddUserError("Start Time must be before End Time.");
                }

            }
        }
    }

    public class CalendarEventSummary
    {
        public string Id { get; set; }
        public string Date { get; set; }
        public string Name { get; set; }
        public bool AllDay { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public string EventTypeKey { get; set; }
        public string EventType { get; set; }
    }
}
