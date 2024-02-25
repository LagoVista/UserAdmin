using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.ScheduledDowntimePeriod_TItle, UserAdminResources.Names.ScheduledDowntimePeriod_Help,
                       UserAdminResources.Names.ScheduledDowntimePeriod_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources),
                       FactoryUrl: "/api/scheduleddowntime/period/factory")]
    public class ScheduledDowntimePeriod : IIDEntity, INamedEntity, IKeyedEntity, IFormDescriptor, IFormConditionalFields
    {
        public ScheduledDowntimePeriod()
        {
            Id = Guid.NewGuid().ToId();
            StartOfDay = true;
            EndOfDay = true;
        }

        public string Id { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
         RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntimePeriod_Start, HelpResource: UserAdminResources.Names.ScheduledDowntimePeriod_Start_Help,
                ValidationRegEx: @"^[0-2]?[0-9]?[0-5]?[0-9]$", RegExValidationMessageResource: UserAdminResources.Names.ScheduledDowntimePeriod_InvalidTimeFormat, FieldType: FieldTypes.Time, ResourceType: typeof(UserAdminResources))]
        public string Start { get; set; }
        
        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntimePeriod_End, HelpResource: UserAdminResources.Names.ScheduledDowntimePeriod_End_Help,
        ValidationRegEx: @"^[0-2]?[0-9]?[0-5]?[0-9]$", RegExValidationMessageResource:UserAdminResources.Names.ScheduledDowntimePeriod_InvalidTimeFormat, FieldType: FieldTypes.Time, ResourceType: typeof(UserAdminResources))]
        public string End { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntimePeriod_StartOfDay, HelpResource:UserAdminResources.Names.ScheduledDowntimePeriod_StartOfDay_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool StartOfDay { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.ScheduledDowntimePeriod_EndOfDay, HelpResource: UserAdminResources.Names.ScheduledDowntimePeriod_EndOfDay_Help, FieldType: FieldTypes.CheckBox, ResourceType: typeof(UserAdminResources))]
        public bool EndOfDay { get; set; }

        public FormConditionals GetConditionalFields()
        {
            return new FormConditionals()
            {
                ConditionalFields = new List<string>() { nameof(Start), nameof(End) },
                Conditionals = new List<FormConditional>()
                 {
                     new FormConditional()
                     {
                         Field = nameof(StartOfDay),
                         Value = "false",
                         VisibleFields = new List<string>() {nameof(Start)},
                         RequiredFields = new List<string>() {nameof(Start)}
                     },
                     new FormConditional()
                     {
                         Field = nameof(EndOfDay),
                         Value = "false",
                         VisibleFields = new List<string>() {nameof(End)},
                         RequiredFields = new List<string>() {nameof(End)}
                     }
                 }
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(StartOfDay),
                nameof(Start),
                nameof(EndOfDay),
                nameof(End),
                nameof(Description),
            };
        }
    }
}
