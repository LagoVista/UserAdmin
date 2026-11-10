using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    /// <summary>
    /// An optional business context within an organization. Tenant ownership remains with OwnerOrganization.
    /// </summary>
    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.BusinessUnit_Title, UserAdminResources.Names.BusinessUnit_Help,
        UserAdminResources.Names.BusinessUnit_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources),
        GetListUrl: "/api/org/businessunits", GetUrl: "/api/org/businessunit/{id}", SaveUrl: "/api/org/businessunit",
        FactoryUrl: "/api/org/businessunit/factory", DeleteUrl: "/api/org/businessunit/{id}",
        Icon: "icon-ae-building", ClusterKey: "org", ModelType: EntityDescriptionAttribute.ModelTypes.DomainEntity,
        Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal,
        IndexInclude: true, IndexTier: EntityDescriptionAttribute.IndexTiers.Primary, IndexPriority: 85,
        IndexTagsCsv: "organizationdomain,org,domainentity,businessunit")]
    public class BusinessUnit : UserAdminModelBase, IKeyedEntity, INamedEntity, IDescriptionEntity,
        IValidateable, IOwnedEntity, IIconEntity, IFormDescriptor, ISummaryFactory
    {
        public BusinessUnit()
        {
            Icon = "icon-ae-building";
        }

        [FormField(LabelResource: UserAdminResources.Names.BusinessUnit_IsActive,
            HelpResource: UserAdminResources.Names.BusinessUnit_IsActive_Help,
            FieldType: FieldTypes.CheckBox, IsRequired: false, ResourceType: typeof(UserAdminResources))]
        public bool IsActive { get; set; } = true;

        public BusinessUnitSummary CreateSummary()
        {
            var summary = new BusinessUnitSummary { IsActive = IsActive };
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
                nameof(Description),
                nameof(IsActive),
            };
        }

        ISummaryData ISummaryFactory.CreateSummary()
        {
            return CreateSummary();
        }
    }

    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.BusinessUnits_Title, UserAdminResources.Names.BusinessUnit_Help,
        UserAdminResources.Names.BusinessUnit_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources),
        GetListUrl: "/api/org/businessunits", GetUrl: "/api/org/businessunit/{id}", SaveUrl: "/api/org/businessunit",
        FactoryUrl: "/api/org/businessunit/factory", DeleteUrl: "/api/org/businessunit/{id}", Icon: "icon-ae-building")]
    public class BusinessUnitSummary : SummaryData
    {
        public bool IsActive { get; set; }
    }
}
