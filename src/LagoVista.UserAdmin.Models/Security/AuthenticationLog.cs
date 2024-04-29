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
        PasswordAuthSuccess,
        PaswwordAuthFailed,
        PasswordAuthUserNotFound,
        CreateEmailUser,
        CreateUserError,
        ManualApproveUser,
        EmailValidated,
        PhoneValidated,
        ManualOrgCreate,
        ChangeOrg,
        CreatingOrg,
        CreatedOrg,
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
        OAuthCreateUser,
        OAuthAppendUserLogin,
        OAuthRemoveUserLogin,
        OAuthFinalizeLogin,
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
        AcceptInvite,
        AcceptInviteFailed,
        DeleteUser,
        DeleteOrg,
        DisableUser,
        AcceptTermsAndConditions,
        GrantRole,
        RevokeRole,
        ConfirmEmailSuccess,
        ConfirmEmailFailed,
        ConfirmPhoneSuccess,
        ConfirmPhoneFailed,
        SendPasswordResetLink,
        ResetPasswordSuccess,
        ResetPasswordFailed,
        SetSystemAdminNotAuthorized,
        SetSystemAdmin,

        SendEmailConfirmSuccess,
        SendEmailConfirmFailed,

        EmailConfirmSuccess,
        EmailConfirmFailed,

        SendSMSConfirmSuccess,
        SendSMSConfirmFailed,

        SMSConfirmedBypass,
        SMSConfirmSuccess,
        SMSConfirmFailed,
    }

    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.AuthenticationLogs_Title, LoggingResources.Names.LogRecord_Description,
       UserAdminResources.Names.AuthenticationLogs_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),
        GetListUrl: "/sys/auth/log", ListUIUrl: "/sysadmin/areas/logs", Icon: "icon-ae-coding-laptop")]
    public class AuthenticationLog : TableStorageEntity
    {
        public AuthenticationLog(AuthLogTypes authType)
        {
            AuthType = authType.ToString();
            PartitionKey = DateTime.UtcNow.ToDateOnly().Replace("/","");
            RowKey = DateTime.Now.ToInverseTicksRowKey();
            TimeStamp = DateTime.UtcNow.ToJSONString();
        }

        public AuthenticationLog()
        {

        }

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
