using LagoVista.Core.Attributes;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.Resources;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.REpos.Account
{
    public interface IMagicLinkManager
    {
        Task<InvokeResult> RequestSignInLinkAsync(MagicLinkRequest request, MagicLinkRequestContext context);
        Task<InvokeResult<string>> RequestSignInLinkAsyncForTesting(MagicLinkRequest request, MagicLinkRequestContext context);
        Task<InvokeResult<AuthenticationResponse>> ConsumeAsync(string code, MagicLinkConsumeContext context);
        Task<InvokeResult<AppUser>> ExchangeAsync(string exchangeCode, MagicLinkExchangeContext context);
    }

    [EntityDescription(Domains.AuthDomain, UserAdminResources.Names.MagicLinkRequest_Title, UserAdminResources.Names.MagicLinkRequest_Help, UserAdminResources.Names.MagicLinkRequest_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), ClusterKey: "auth", ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified, Shape: EntityDescriptionAttribute.EntityShapes.Dto, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: false)]
    public class MagicLinkRequest
    {
        public string Email { get; set; }
        public string Channel { get; set; }
        public string ReturnUrl { get; set; }
    }

    [EntityDescription(Domains.AuthDomain, UserAdminResources.Names.MagicLinkRequestContext_Title, UserAdminResources.Names.MagicLinkRequestContext_Help, UserAdminResources.Names.MagicLinkRequestContext_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), ClusterKey: "auth", ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified, Shape: EntityDescriptionAttribute.EntityShapes.Dto, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: false)]
    public class MagicLinkRequestContext
    {
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string CorrelationId { get; set; }
    }

    [EntityDescription(Domains.AuthDomain, UserAdminResources.Names.MagicLinkConsumeContext_Title, UserAdminResources.Names.MagicLinkConsumeContext_Help, UserAdminResources.Names.MagicLinkConsumeContext_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), ClusterKey: "auth", ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified, Shape: EntityDescriptionAttribute.EntityShapes.Dto, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: false)]
    public class MagicLinkConsumeContext
    {
        public string Channel { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string ReturnUrl { get; set; }
    }

    [EntityDescription(Domains.AuthDomain, UserAdminResources.Names.MagicLinkExchangeContext_Title, UserAdminResources.Names.MagicLinkExchangeContext_Help, UserAdminResources.Names.MagicLinkExchangeContext_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), ClusterKey: "auth", ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified, Shape: EntityDescriptionAttribute.EntityShapes.Dto, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: false)]
    public class MagicLinkExchangeContext
    {
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string CorrelationId { get; set; }
    }

    [EntityDescription(Domains.AuthDomain, UserAdminResources.Names.MagicLinkConsumeResponse_Title, UserAdminResources.Names.MagicLinkConsumeResponse_Help, UserAdminResources.Names.MagicLinkConsumeResponse_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), ClusterKey: "auth", ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified, Shape: EntityDescriptionAttribute.EntityShapes.Dto, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: false)]
    public class MagicLinkConsumeResponse
    {
        public MagicLinkAttempt Attempt { get; set; }
        public string Redirect { get; set; }
        public string ExchangeCode { get; set; }
    }

    [EntityDescription(Domains.AuthDomain, UserAdminResources.Names.MagicLinkExchangeResponse_Title, UserAdminResources.Names.MagicLinkExchangeResponse_Help, UserAdminResources.Names.MagicLinkExchangeResponse_Description, EntityDescriptionAttribute.EntityTypes.Dto, typeof(UserAdminResources), ClusterKey: "auth", ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified, Shape: EntityDescriptionAttribute.EntityShapes.Dto, Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime, Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential, IndexInclude: false)]
    public class MagicLinkExchangeResponse
    {
        public MagicLinkAttempt Attempt { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}
