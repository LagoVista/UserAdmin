// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 428893e78b20733cdf7a31dcb7a3f49c4ebd63985f658a7197793ee7b920dabb
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Resources;
using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Orgs
{
    [EntityDescription(
        Domains.OrganizationDomain, UserAdminResources.Names.Subscription_Title, UserAdminResources.Names.Subscription_Help,
        UserAdminResources.Names.Subscription_Description, EntityDescriptionAttribute.EntityTypes.OrganizationModel, typeof(UserAdminResources),

        GetListUrl: "/api/subscriptions", GetUrl: "/api/subscription/{id}", SaveUrl: "/api/subscription", FactoryUrl: "/api/subscription/factory",

        CreateUIUrl: "/organization/subscription/add", ListUIUrl: "/organization/subscriptions", EditUIUrl: "/organization/suscription/{id}",

        Icon: "icon-ae-bill-1", ClusterKey: "subscriptions", ModelType: EntityDescriptionAttribute.ModelTypes.DomainEntity,
        Lifecycle: EntityDescriptionAttribute.Lifecycles.DesignTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: true,
        IndexTier: EntityDescriptionAttribute.IndexTiers.Aux, IndexPriority: 35, IndexTagsCsv: "organizationdomain,subscriptions,domainentity,dto")]
    public class Subscription : RelationalEntityBase, IValidateable, IKeyedEntity, INamedEntity, IFormDescriptor, IFormDescriptorCol2
    {
        public const string Status_OK = "ok";
        public const string Status_FreeAccount = "freeaccount";
        public const string Status_TrialAccount = "trialaccount";
        public const string Status_NoPaymentDetails = "nopaymentdetails";

        public const string PaymentTokenStatus_OK = "ok";
        public const string PaymentTokenStatus_Waived = "waived";
        public const string PaymentTokenStatus_Empty = "empty";
        public const string PaymentTokenStatus_Invalid = "invalid";

        public const string SubscriptionKey_Trial = "trial";

        public Subscription()
        {
            Icon = "icon-ae-bill-1";
        }

        [MapTo("CustomerId")]
        [FormField(LabelResource: UserAdminResources.Names.Subscription_Customer,  IsUserEditable: true, FieldType: FieldTypes.CustomerPicker, ResourceType: typeof(UserAdminResources), IsRequired: false)]
        public EntityHeader Customer { get; set; }

        public string PaymentTokenSecretId { get; set; }

        [IgnoreOnMapTo()]
        [FormField(LabelResource: UserAdminResources.Names.Subscription_PaymentMethod, HelpResource:UserAdminResources.Names.Subscription_PaymentMethod_Help,  SecureIdFieldName: "PaymentTokenSecretId", IsUserEditable:false, FieldType: FieldTypes.Secret, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string PaymentToken { get; set; }


        [FormField(LabelResource: Resources.UserAdminResources.Names.Common_Icon, FieldType: FieldTypes.Icon, ResourceType: typeof(UserAdminResources), IsRequired: false, IsUserEditable: true)]
        public string Icon { get; set; }

        public string PaymentTokenCustomerId { get; set; }
        public string PaymentAccountType { get; set; } = "stripe";
        public string PaymentAccountId { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Subscription_IsTrial, FieldType: FieldTypes.Bool, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public bool IsTrial { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Subscription_IsActive, FieldType: FieldTypes.Bool, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public bool IsActive { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Subscription_ActiveDate, FieldType: FieldTypes.Date, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public CalendarDate? ActiveDate { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Subscription_InActiveDate, FieldType: FieldTypes.Date, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public CalendarDate? InActiveDate { get; set; }
        [FormField(LabelResource: UserAdminResources.Names.Subscription_TrialStartDate, FieldType: FieldTypes.Date, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public CalendarDate? TrialStartDate { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Subscription_TrialEndDate, FieldType: FieldTypes.Date, IsUserEditable: false, ResourceType: typeof(UserAdminResources))]
        public CalendarDate? TrialExpirationDate { get; set; }


        [FormField(LabelResource: UserAdminResources.Names.Subscription_PaymentMethod_Date, FieldType: FieldTypes.Date, IsUserEditable:false, ResourceType: typeof(UserAdminResources))]
        public CalendarDate? PaymentTokenDate { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Subscription_PaymentMethod_Expires, FieldType: FieldTypes.Date, ResourceType: typeof(UserAdminResources), IsUserEditable: false)]
        public CalendarDate? PaymentTokenExpires { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Subscription_StartDate, FieldType: FieldTypes.Date, ResourceType: typeof(UserAdminResources), IsRequired:true, IsUserEditable: true)]
        public CalendarDate Start { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Subscription_EndDate, FieldType: FieldTypes.Date, ResourceType: typeof(UserAdminResources), IsUserEditable: true)]
        public CalendarDate? End { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Subscription_PaymentMethod_Status, FieldType: FieldTypes.ReadonlyLabel, IsUserEditable:false, ResourceType: typeof(UserAdminResources), IsRequired: false)]
        public string PaymentTokenStatus { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Subscription_Status, FieldType: FieldTypes.ReadonlyLabel, ResourceType: typeof(UserAdminResources), IsUserEditable:false, IsRequired: false)]
        public String Status { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Key, HelpResource: UserAdminResources.Names.Common_Key_Help, FieldType: FieldTypes.Key, RegExValidationMessageResource: UserAdminResources.Names.Common_Key_Validation, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public LagoVistaKey Key { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Name, FieldType: FieldTypes.Text, ResourceType: typeof(UserAdminResources), IsRequired: true)]
        public string Name { get; set; }

        [FormField(LabelResource: UserAdminResources.Names.Common_Description, FieldType: FieldTypes.MultiLineText, ResourceType: typeof(UserAdminResources), IsRequired: false)]
        public string Description { get; set; }

        [CustomValidator]
        public void Validate(ValidationResult result, Actions action)
        {
            if (action == Actions.Update && Key == SubscriptionKey_Trial)
            {
                result.AddUserError("Can not update trial subscription.");
            }
        }

        public SubscriptionSummary CreateSummary()
        {
            return new SubscriptionSummary()
            {
                Id = Id,
                Name = Name,
                Icon = Icon,
                PaymentTokenStatus = PaymentTokenStatus,
                Key = Key,
                Description = Description,
                IsPublic = false
                
            };
        }

        public List<string> GetFormFields()
        {
            return new List<string>()
            {
                nameof(Name),
                nameof(Key),
                nameof(Icon),
                nameof(Start),
                nameof(End),
                nameof(ActiveDate),
                nameof(InActiveDate),
                nameof(End),
                nameof(IsTrial),
                nameof(TrialStartDate),
                nameof(TrialExpirationDate),
                nameof(Description),
            };
        }

        public List<string> GetFormFieldsCol2()
        {

            return new List<string>()
            {
                nameof(Customer),
                nameof(Status),
                nameof(PaymentToken),
                nameof(PaymentTokenStatus),
                nameof(PaymentTokenDate),
                nameof(PaymentTokenExpires),
            };
        }

        public EntityHeader ToEntityHeader()
        {
            return EntityHeader.Create(Id.ToString(), Name); 
        }
    }

    [EntityDescription(Domains.OrganizationDomain, UserAdminResources.Names.Subscriptions_Title, UserAdminResources.Names.Subscription_Help,
        UserAdminResources.Names.Subscription_Description, EntityDescriptionAttribute.EntityTypes.Summary, typeof(UserAdminResources), Icon: "icon-ae-bill-1",
        GetListUrl: "/api/subscriptions", GetUrl: "/api/subscription/{id}", SaveUrl: "/api/subscription", FactoryUrl: "/api/subscription/factory")]
    public class SubscriptionSummary 
    {
        public GuidString36 Id { get; set; }
        public LagoVistaKey Icon { get; set; }
        public string Description { get; set; } 

        public bool IsPublic { get; set; }  
        public string Name { get; set; }
        public string Key { get; set; }
        public string Status { get; set; }
        public string PaymentTokenStatus { get; set; }
    }
}
