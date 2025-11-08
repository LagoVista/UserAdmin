// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cb4a70640bacc88ce8a8f19f03f7e50c160ad4ba6612ef3e2d84b073d280c16f
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Models;
using LagoVista.IoT.Logging.Resources;
using LagoVista.UserAdmin.Models.Resources;
using System;

namespace LagoVista.UserAdmin.Models.Security
{
    public enum AuthLogTypes
    {
        PasswordAuthStart,
        PasswordAuthSuccess,
        PaswwordAuthFailed,
        PasswordAuthUserNotFound,
       
        CreateEmailUser,
        CreateExernalLoginUser,
        CreateUserSuccess,
        CreateUserError,

        ManualApproveUser,
        EmailValidated,
        PhoneValidated,
        ManualOrgCreate,
        ChangeOrg,
        CreatingOrg,
        CreatedOrg,
        AssignedCurrentOrgToUser,
        PopulatingNewOrg,
        PopulatedNewOrg,
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
        OAuthFinalizingLogin,
        OAuthFinalizedLogin,
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
        ChangePasswordSuccess,
        ChangePasswordFailed,
        InviteUser,
        RegisterUser,


        AcceptingInvite,
        AcceptedInvite,
        AcceptInviteFailed,
        
        DeletingUser,
        DeleteUserFailed,
        DeletedUser,

        DeletingOrg,
        DeletedOrg,

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

        UnauthorizedCall
    }

    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.AuthenticationLogs_Title, LoggingResources.Names.LogRecord_Description,
       UserAdminResources.Names.AuthenticationLogs_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),
        GetListUrl: "/sys/auth/log", ListUIUrl: "/sysadmin/areas/logs", Icon: "icon-ae-coding-laptop")]
    public class AuthenticationLog : TableStorageEntity
    {
        public AuthenticationLog(AuthLogTypes authType)
        {
            AuthType = authType.ToString();
            PartitionKey = (500000 - (Convert.ToInt32(DateTime.UtcNow.ToDateOnly().Replace("/","")) - 200000000)).ToString();
         
            RowKey = DateTime.Now.ToInverseTicksRowKey();
            TimeStamp = DateTime.UtcNow.ToJSONString();
        }

        public AuthenticationLog()
        {

        }

        public string IPAddress { get; set; }

        public string InviteId { get; set; }

        public string RedirectUri { get; set; }

        public string UserName { get; set; }
        public string UserId { get; set; }
        public string TimeStamp { get; set; }

        public string OrgId { get; set; }
        public string OrgName { get; set; }

        public string AuthType { get; set; }

        public string OAuthProvider { get; set; }
        public string Extras { get; set; }
        public string Errors { get; set; }
    }
}
