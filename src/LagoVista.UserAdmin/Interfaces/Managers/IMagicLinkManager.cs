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
        /// <summary>
        /// Non-enumerating. Always returns Success even if user not found.
        /// Sends email only when an existing user is found.
        /// </summary>
        Task<InvokeResult> RequestSignInLinkAsync(MagicLinkRequest request, MagicLinkRequestContext context);

        Task<InvokeResult<string>> RequestSignInLinkAsyncForTesting(MagicLinkRequest request, MagicLinkRequestContext context);


        /// <summary>
        /// Consumes a magic link code. For portal, returns the user for cookie sign-in.
        /// For mobile, returns an exchange code that can be exchanged for a JWT.
        /// </summary>
        Task<InvokeResult<UserLoginResponse>> ConsumeAsync(string code, MagicLinkConsumeContext context);

        /// <summary>
        /// Exchanges a short-lived exchange code (mobile) and returns the user for JWT issuance.
        /// </summary>
        Task<InvokeResult<AppUser>> ExchangeAsync(string exchangeCode, MagicLinkExchangeContext context);
    }



    [EntityDescription(
    Domains.AuthDomain,
    UserAdminResources.Names.MagicLinkRequest_Title,
    UserAdminResources.Names.MagicLinkRequest_Help,
    UserAdminResources.Names.MagicLinkRequest_Description,
    EntityDescriptionAttribute.EntityTypes.Dto,
    typeof(UserAdminResources),
    ClusterKey: "auth",
    ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified,
    Shape: EntityDescriptionAttribute.EntityShapes.Dto,
    Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
    Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential,
    IndexInclude: false)]
    public class MagicLinkRequest
    {
        public string Email { get; set; }

        /// <summary>
        /// "portal" or "mobile" (use MagicLinkAttempt.Channel_* constants).
        /// </summary>
        public string Channel { get; set; }

        public string ReturnUrl { get; set; }
    }

    [EntityDescription(
    Domains.AuthDomain,
    UserAdminResources.Names.MagicLinkRequestContext_Title,
    UserAdminResources.Names.MagicLinkRequestContext_Help,
    UserAdminResources.Names.MagicLinkRequestContext_Description,
    EntityDescriptionAttribute.EntityTypes.Dto,
    typeof(UserAdminResources),
    ClusterKey: "auth",
    ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified,
    Shape: EntityDescriptionAttribute.EntityShapes.Dto,
    Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
    Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal,
    IndexInclude: false)]
    public class MagicLinkRequestContext
    {
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string CorrelationId { get; set; }
    }

    [EntityDescription(
    Domains.AuthDomain,
    UserAdminResources.Names.MagicLinkConsumeContext_Title,
    UserAdminResources.Names.MagicLinkConsumeContext_Help,
    UserAdminResources.Names.MagicLinkConsumeContext_Description,
    EntityDescriptionAttribute.EntityTypes.Dto,
    typeof(UserAdminResources),
    ClusterKey: "auth",
    ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified,
    Shape: EntityDescriptionAttribute.EntityShapes.Dto,
    Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
    Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal,
    IndexInclude: false)]

    public class MagicLinkConsumeContext
    {
        /// <summary>
        /// "portal" or "mobile"
        /// </summary>
        public string Channel { get; set; }

        public string IpAddress { get; set; }
        public string UserAgent { get; set; }

        /// <summary>
        /// Optional allowlisted return URL. Portal consume may redirect to it.
        /// </summary>
        public string ReturnUrl { get; set; }
    }


    [EntityDescription(
    Domains.AuthDomain,
    UserAdminResources.Names.MagicLinkExchangeContext_Title,
    UserAdminResources.Names.MagicLinkExchangeContext_Help,
    UserAdminResources.Names.MagicLinkExchangeContext_Description,
    EntityDescriptionAttribute.EntityTypes.Dto,
    typeof(UserAdminResources),
    ClusterKey: "auth",
    ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified,
    Shape: EntityDescriptionAttribute.EntityShapes.Dto,
    Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
    Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal,
    IndexInclude: false)]
    public class MagicLinkExchangeContext
    {
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string CorrelationId { get; set; }
    }

    [EntityDescription(
    Domains.AuthDomain,
    UserAdminResources.Names.MagicLinkConsumeResponse_Title,
    UserAdminResources.Names.MagicLinkConsumeResponse_Help,
    UserAdminResources.Names.MagicLinkConsumeResponse_Description,
    EntityDescriptionAttribute.EntityTypes.Dto,
    typeof(UserAdminResources),
    ClusterKey: "auth",
    ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified,
    Shape: EntityDescriptionAttribute.EntityShapes.Dto,
    Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
    Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential,
    IndexInclude: false)]

    public class MagicLinkConsumeResponse
    {
        /// <summary>
        /// Consumed attempt (ConsumedAtUtc set).
        /// </summary>
        public MagicLinkAttempt Attempt { get; set; }

        public string Redirect { get; set; }

        /// <summary>
        /// For mobile, the short-lived exchange code to be sent back to the app.
        /// For portal, this should be null.
        /// </summary>
        public string ExchangeCode { get; set; }
    }

    [EntityDescription(
    Domains.AuthDomain,
    UserAdminResources.Names.MagicLinkExchangeResponse_Title,
    UserAdminResources.Names.MagicLinkExchangeResponse_Help,
    UserAdminResources.Names.MagicLinkExchangeResponse_Description,
    EntityDescriptionAttribute.EntityTypes.Dto,
    typeof(UserAdminResources),
    ClusterKey: "auth",
    ModelType: EntityDescriptionAttribute.ModelTypes.Unspecified,
    Shape: EntityDescriptionAttribute.EntityShapes.Dto,
    Lifecycle: EntityDescriptionAttribute.Lifecycles.RunTime,
    Sensitivity: EntityDescriptionAttribute.Sensitivities.Confidential,
    IndexInclude: false)]
    public class MagicLinkExchangeResponse
    {
        /// <summary>
        /// Attempt after successful exchange consumption.
        /// </summary>
        public MagicLinkAttempt Attempt { get; set; }

        /// <summary>
        /// Resolved user id (must be present for sign-in-only flow).
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// Normalized email used for resolution/audit.
        /// </summary>
        public string Email { get; set; }
    }
}
