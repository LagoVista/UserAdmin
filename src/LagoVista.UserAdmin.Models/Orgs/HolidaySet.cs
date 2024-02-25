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
                          UserAdminResources.Names.HolidaySet_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources), Icon: "icon-ae-calendar",
                          ListUIUrl: "/organization/holidaysets", CreateUIUrl: "/organization/holidayset/add", EditUIUrl: "/organization/holidayset/{id}",
                          GetListUrl: "/api/holidaysets", GetUrl: "/api/holidayset/{id}", SaveUrl: "/api/holidayset", FactoryUrl: "/api/holidayset/factory", DeleteUrl: "/api/holidayset/{id}")]
    public class HolidaySet : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity, IFormDescriptor, IFormDescriptorCol2, IIconEntity, ISummaryFactory, ICategorized
    {
        public HolidaySet()
        {
            Holidays = new List<ScheduledDowntime>();
            Icon = "icon-ae-calendar";
        }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, IsRequired: true, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources))]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key,
            RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Category, FieldType: FieldTypes.Category, WaterMark: UserAdminResources.Names.Common_Category_Select, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public EntityHeader Category { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HolidaySet_Culture_Or_Country, FieldType: FieldTypes.Text, IsRequired: true, ResourceType: typeof(UserAdminResources))]
        public string CultureOrCountry { get; set; }

        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.HolidaySet_Holidays, FieldType: FieldTypes.ChildListInline, IsReferenceField:false, FactoryUrl: "/api/scheduleddowntime/factory", ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
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
                Icon = Icon,
                Name = Name,
                IsPublic = IsPublic,
                Key = Key,
                Category = Category,
                CultureOrCountry = CultureOrCountry
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
    public class HolidaySetSummary : CategorizedSummaryData
    {
        public string CultureOrCountry {get; set;}
    }
}
