// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3a166cf9869355f9b3dd36ee3b510de4e1f25cf4f4fcc4878960797156675ee3
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.DistroList_Name, UserAdminResources.Names.DistroList_Help,
        UserAdminResources.Names.DistroList_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        GetListUrl: "/api/distros", SaveUrl: "/api/distro", GetUrl: "/api/distro/{id}", FactoryUrl: "/api/distro/factory", DeleteUrl: "/api/distro/{id}",

        ListUIUrl: "/organization/distrolists", CreateUIUrl: "/organization/distrolist/add", EditUIUrl: "/organization/distrolist/{id}",

        Icon: "icon-pz-rating-star", ClusterKey: "notifications", ModelType: EntityDescriptionAttribute.ModelTypes.Configuration,
        Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true,
        IndexTier: EntityDescriptionAttribute.IndexTiers.Primary, IndexPriority: 80, IndexTagsCsv: "organizationdomain,notifications,configuration,distrolist")]
    public class DistroList : UserAdminModelBase, IKeyedEntity, INamedEntity, IValidateable, IOwnedEntity, IDescriptionEntity, IFormDescriptor, IIconEntity, ICategorized, IFormAdditionalActions, ISummaryFactory, ICustomerOwnedEntity
    {
        public DistroList()
        {
            Icon = "icon-pz-rating-star";
        }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Customer, IsRequired: false, FieldType: FieldTypes.CustomerPicker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader Customer { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.DistroList_ParentList,  EntityHeaderPickerUrl: "/api/distros", IsRequired: false, FieldType: FieldTypes.EntityHeaderPicker, ResourceType: typeof(UserAdminResources))]
        public EntityHeader ParentDistributionList { get; set; }


        public List<AppUserContact> AppUsers { get; set; } = new List<AppUserContact>();

        [FormField(LabelResource: UserAdminResources.Names.DistributionList_ExternalContacts, IsRequired: false, ChildListDisplayMembers: "FirstName,LastName", FieldType: FieldTypes.ChildListInline, EntityHeaderPickerUrl: "/api/distro/externalcontact/factory", ResourceType: typeof(UserAdminResources))]
        public List<ExternalContact> ExternalContacts { get; set; } = new List<ExternalContact>();

        public DistroListSummary CreateSummary()
        {
            var summary = new DistroListSummary();
            summary.Populate(this);
                return summary;
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
               nameof(Name),
               nameof(Key),
               nameof(Icon),
               nameof(Category),
               nameof(ParentDistributionList),
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
    public class DistroListSummary : SummaryData
    {

    }

    public class AppUserContact : EntityHeader
    {
        public string EmailConfirmedTimeStamp { get; set; }
        public string SmsConfirmedTimeStamp { get; set; }
    }
}


