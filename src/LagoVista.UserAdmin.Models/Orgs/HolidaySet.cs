// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8b0396c184f30e7219fe8aeb5842564d6da1e17af5d85347442c06a7ac5acb31
// IndexVersion: 2
// --- END CODE INDEX META ---
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
    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.HolidaySet_Title, UserAdminResources.Names.HolidaySet_Help,
        UserAdminResources.Names.HolidaySet_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        GetListUrl: "/api/holidaysets", GetUrl: "/api/holidayset/{id}", SaveUrl: "/api/holidayset", FactoryUrl: "/api/holidayset/factory",
        DeleteUrl: "/api/holidayset/{id}",

        ListUIUrl: "/organization/holidaysets", CreateUIUrl: "/organization/holidayset/add", EditUIUrl: "/organization/holidayset/{id}",

        Icon: "icon-ae-calendar", ClusterKey: "calendars", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration,
        Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true,
        IndexTier: EntityDescriptionAttribute.IndexTiers.Primary, IndexPriority: 80, IndexTagsCsv: "organizationdomain,calendars,configuration,holidayset")]
    public class HolidaySet : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity, IFormDescriptor, IFormDescriptorCol2, IIconEntity, ISummaryFactory, ICategorized
    {
        public HolidaySet()
        {
            Holidays = new List<ScheduledDowntime>();
            Icon = "icon-ae-calendar";
        }

        [FormField(LabelResource: UserAdminResources.Names.HolidaySet_Culture_Or_Country, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string CultureOrCountry { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HolidaySet_Holidays, FieldType: FieldTypes.ChildListInline, IsReferenceField:false, FactoryUrl: "/api/scheduleddowntime/factory", ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public List<ScheduledDowntime> Holidays { get; set; }

        public HolidaySetSummary CreateSummary()
        {
            var holiday = new HolidaySetSummary();
            holiday.CultureOrCountry = CultureOrCountry;
            holiday.Populate(this);
            return holiday;
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Category),
                nameof(CultureOrCountry),
                nameof(Description),
            };
        }

        public List<string> GetFormFieldsCol2()
        {
            return new List<string>() { nameof(Holidays) };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.HolidaySets_Title, UserAdminResources.Names.HolidaySet_Help,
                          UserAdminResources.Names.HolidaySet_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources), Icon: "icon-ae-calendar",
                          GetListUrl: "/api/holidaysets", GetUrl: "/api/holidayset/{id}", SaveUrl: "/api/holidayset", FactoryUrl: "/api/holidayset/factory", DeleteUrl: "/api/holidayset/{id}")]
    public class HolidaySetSummary : SummaryData
    {
        public string CultureOrCountry {get; set;}
    }
}
