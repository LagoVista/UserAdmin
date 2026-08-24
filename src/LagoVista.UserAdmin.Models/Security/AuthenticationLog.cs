// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cb4a70640bacc88ce8a8f19f03f7e50c160ad4ba6612ef3e2d84b073d280c16f
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Attributes;
using LagoVista.IoT.Logging.Resources;
using LagoVista.UserAdmin.Models.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Security
{
    public enum AuthLogTypes
    {
        PasswordAuthenticationStarted,
        PasswordAuthenticationSucceeded,
        PasswordAuthenticationFailed,
        PasswordAuthUserNotFound,

        PasswordRecoveryRequested,
        PasswordRecoveryCodeGenerated,
        PasswordRecoveryMessageSent,
        PasswordRecoveryCodeVerified,
        PasswordRecoveryCodeVerificationFailed,
        PasswordRecoveryCompleted,
       
        CreateEmailUser,
        ExternalLoginUserCreationStarted,
        CreateUserSuccess,
        CreateUserError,

        ManualApproveUser,
        EmailValidated,
        PhoneValidated,
        ManualOrgCreate,
        ChangeOrg,
        OrganizationCreationStarted,
        OrganizationCreationSucceeded,
        AssignedCurrentOrgToUser,
        OrganizationPopulationStarted,
        OrganizationPopulationSucceeded,
        UserPasswordLogin,
        UserLogout,
        UserPasswordFailedLogin,
        OAuthInitiate,
        OAuthRedirect,
        OAuthCallback,
        OAuthError,
        OAuthCreateOrg,
        OAuthAppendUserLogin,
        OAuthRemoveUserLogin,
        OAuthLoginFinalizationStarted,
        OAuthLoginFinalizationSucceeded,
        OAuthLogin,
        OAuthAccessDefined,
        OAuthFault,
        OAuthAccessTicketReceived,
        OAuthCreatingTicket,
        OAuthBackChannelHandler,
        OAuthBackChannelHandlerSuccess,
        OAuthBackChannelHandlerFailure,
        AddUserToOrg,
        SetAsOrgAdmin,
        ClearOrgAdmin,
        PasswordSetByAdminSucceeded,
        PasswordSetByAdminFailed,
        ChangePasswordSuccess,
        ChangePasswordFailed,
        InviteUser,
        RegisterUser,

        InviteAcceptanceStarted,
        InviteAcceptanceSucceeded,
        InviteAcceptanceFailed,
        
        UserDeletionStarted,
        UserDeletionFailed,
        UserDeletionSucceeded,

        OrganizationDeletionStarted,
        OrganizationDeletionSucceeded,

        RemoveUserFromOrg,
        DisableUser,
        AcceptTermsAndConditions,
        GrantRole,
        RevokeRole,
        AutoConfirmEmail,
        ConfirmEmailSuccess,
        ConfirmEmailFailed,
        ConfirmPhoneSuccess,
        ConfirmPhoneFailed,
        SendPasswordResetLink,
        ResetPasswordSuccess,
        ResetPasswordFailed,
        SetSystemAdminNotAuthorized,
        SetSystemAdmin,

        GenerateRefreshToken,
        GenerateRefreshTokenSuccess,
        GenerateRefreshTokenFailed,

        RenewRefreshToken,
        RenewRefreshTokenSuccess,
        RenewRefreshTokenFailed,

        AccessTokenGrant,
        AccessTokenGrantSuccess,
        AccessTokenGrantFailure,

        SingleUseTokenGrant,
        SingleUseTokenGrantSuccess,
        SingleUseTokenGrantFailure,

        RefreshTokenGrant,
        RefreshTokenGrantSuccess,
        RefreshTokenGrantFailed,

        AddSubscription,
        RemoveSubscription,
        RemovingAllSubscriptionsForOrg,
        RemovedAllSubscriptionsForOrg,

        SendingEmailConfirm,
        SendEmailConfirmSuccess,
        SendEmailConfirmFailed,

        EmailConfirmSuccess,
        EmailConfirmFailed,

        SendSMSConfirmSuccess,
        SendSMSConfirmFailed,

        SMSConfirmedBypass,
        SMSConfirmSuccess,
        SMSConfirmFailed,

        SendingOrgInvitation,
        SendOrgInvitationSuccess,
        SendOrgInvitationFailed,

        ResendOrgInvitation,
        ResendOrgInvitationSuccess,
        ResendOrgInvitationFailed,

        SysAdminGetAllOrgs,
        SysAdminSearchAllOrgs,
        SysAdminGetOwnedObjects,
        SysAdminGetOrg,
        SysAdminUpdateOrg,

        UnauthorizedCall,

        PasskeyBeginRegistrationStart,
        PasskeyBeginRegistrationSuccess,
        PasskeyBeginRegistrationFailed,

        PasskeyCompleteRegistrationStart,
        PasskeyCompleteRegistrationSuccess,
        PasskeyCompleteRegistrationFailed,

        PasskeySetupStarted,
        PasskeySetupSucceeded,
        PasskeySetupFailed,

        PasskeyAuthenticationOptionsSent,
        PasskeyAuthenticationOptionsBeginSent,
        PasskeyAuthenticationOptionsBeginFailed,

        PasskeyCompleteAuthenticationStart,
        PasskeyCompleteAuthenticationSuccess,
        PasskeyCompleteAuthenticationFailed,

        PasskeyBeginPasswordlessRegistrationStart,
        PasskeyBeginPasswordlessRegistrationFailed,
        PasskeyBeginPasswordlessRegistrationSuccess,

        PasskeyCompletePasswordlessRegistrationStart,
        PasskeyCompletePasswordlessRegistrationFailed,
        PasskeyCompletePasswordlessRegistrationSuccess,

        PasskeyBeginPasswordlessAuthenticationStart,
        PasskeyBeginPasswordlessAuthenticationFailed,
        PasskeyBeginPasswordlessAuthenticationSuccess,
    
        PasskeyCompletePasswordlessAuthenticationStart,
        PasskeyCompletePasswordlessAuthenticationFailed,
        PasskeyCompletePasswordlessAuthenticationSuccess,

        TotpBeginEnrollmentStart,
        TotpBeginEnrollmentFailed,
        TotpBeginEnrollmentSuccess,
    
        TotpConfirmEnrollmentStart,
        TotpConfirmEnrollmentFailed,
        TotpConfirmEnrollmentSuccess,

        TotpVerifyStart,
        TotpVerifyFailed,
        TotpVerifySuccess,

        TotpRotateRecoveryCodesStart,
        TotpRotateRecoveryCodesFailed,
        TotpRotateRecoveryCodesSuccess,

        TotpConsumeRecoveryCodeStart,
        TotpConsumeRecoveryCodeFailed,
        TotpConsumeRecoveryCodeSuccess,

        TotpDisableMfaStart,
        TotpDisableMfaFailed,
        TotpDisableMfaSuccess,

        TotpAdministrativeResetStart,
        TotpAdministrativeResetFailed,
        TotpAdministrativeResetSuccess,

        MagicLinkRequested,
        MagicLinkSent,
        MagicLinkConsumed,
        MagicLinkConsumptionFailed,
        MagicLinkExchangeIssued,
        MagicLinkExchangeSucceeded,
        MagicLinkExchangeFailed
    }

    [EntityDescription(
        Domains.SecurityDomain, UserAdminResources.Names.AuthenticationLogs_Title, LoggingResources.Names.LogRecord_Description,
        UserAdminResources.Names.AuthenticationLogs_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),

        GetListUrl: "/sys/auth/log",

        ListUIUrl: "/sysadmin/areas/logs",

        Icon: "icon-ae-coding-laptop", ClusterKey: "audit", ModelType: EntityDescriptionAttribute.ModelTypes.RuntimeArtifact,
        Lifecycle: EntityDescriptionAttribute.Lifecycles.Audit, Sensitivity: EntityDescriptionAttribute.Sensitivities.Internal, IndexInclude: false,
        IndexTier: EntityDescriptionAttribute.IndexTiers.Exclude, IndexPriority: 10, IndexTagsCsv: "securitydomain,audit,runtimeartifact")]
    public class AuthenticationLog : IActivityRecord
    {
        public AuthenticationLog()
        {
            Id = Guid.NewGuid().ToString("N");
            CreationDate = DateTime.UtcNow;
        }

        public AuthenticationLog(AuthLogTypes authType) : this()
        {
            AuthType = authType.ToString();
        }

        public string Id { get; set; }
        public string OrganizationId { get; set; }
        public string Organization { get; set; }
        public DateTime CreationDate { get; set; }

        public string IPAddress { get; set; }
        public string InviteId { get; set; }
        public string RedirectUri { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string AuthType { get; set; }
        public string ChallengeId { get; set; }
        public string CredentialId { get; set; }
        public string AssertionId { get; set; }
        public string OAuthProvider { get; set; }
        public string Extras { get; set; }
        public string Errors { get; set; }
    }
}
