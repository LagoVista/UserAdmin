using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.DistroList_Name,
        UserAdminResources.Names.DistroList_Help, UserAdminResources.Names.DistroList_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),
        ListUIUrl: "/organization/distrolists", CreateUIUrl: "/organization/distrolist/add", EditUIUrl: "/organization/distrolist/{id}",
        Icon: "icon-pz-rating-star", GetListUrl: "/api/distros", SaveUrl: "/api/distro", GetUrl: "/api/distro/{id}", FactoryUrl: "/api/distro/factory", DeleteUrl: "/api/distro/{id}")]
    public class DistroList : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity, IFormDescriptor, IIconEntity, ICategorized, IFormAdditionalActions, ISummaryFactory
    {
        public DistroList()
        {
            Icon = "icon-pz-rating-star";
        }

        [FormField(LabelResource: UserAdminResources.Names.Common_Category, FieldType: FieldTypes.Category, WaterMark: UserAdminResources.Names.Common_Category_Select, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public EntityHeader Category { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Common_Description, IsRequired: false, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources))]
        public string Description { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Customer, IsRequired: false, FieldType: FieldTypes.CustomerPicker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader Customer { get; set; }

        public List<AppUserContact> AppUsers { get; set; } = new List<AppUserContact>();

        [FormField(LabelResource: UserAdminResources.Names.DistributionList_ExternalContacts, IsRequired: false, ChildListDisplayMember: "firstName", FieldType: FieldTypes.ChildListInline, EntityHeaderPickerUrl: "/api/distro/externalcontact/factory", ResourceType: typeof(UserAdminResources))]
        public List<ExternalContact> ExternalContacts { get; set; } = new List<ExternalContact>();

        public DistroListSummary CreateSummary()
        {
            return new DistroListSummary()
            {
                Description = Description,
                Id = Id,
                IsPublic = IsPublic,
                Key = Key,
                Icon = Icon,
                Name = Name,
                Category = Category,
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
               nameof(Customer),
               nameof(Description),
               nameof(ExternalContacts),
            };
        }

        public List<FormAdditionalAction> GetAdditionalActions()
        {
            return new List<FormAdditionalAction>()
            {
                new FormAdditionalAction()
                {
                     ForEdit = true,
                     ForCreate = false,
                     Key = "confirm",
                     Title = "Confirm",
                     Icon = "fa-check"

                }
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.DistroLists_Name,
         UserAdminResources.Names.DistroList_Help, UserAdminResources.Names.DistroList_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources),
         Icon: "icon-pz-rating-star", GetListUrl: "/api/distros", SaveUrl: "/api/distro", GetUrl: "/api/distro/{id}", FactoryUrl: "/api/distro/factory", DeleteUrl: "/api/distro/{id}")]
    public class DistroListSummary : CategorizedSummaryData
    {

    }

    public class AppUserContact : EntityHeader
    {
        public string EmailConfirmedTimeStamp { get; set; }
        public string SmsConfirmedTimeStamp { get; set; }
    }
}


