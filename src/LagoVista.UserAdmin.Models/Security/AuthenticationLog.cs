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
        CreateEmailUser,
        CreateUserError,
        ManualApproveUser,
        EmailValidated,
        PhoneValidated,
        ManualOrgCreate,
        ChangeOrg,
        CreateOrg,
        UserPasswordLogin,
        UserLogout,
        UserPasswordFailedLogin,
        OAuthRedirect,
        OAuthCallback,
        OAuthError,
        OAuthCreateOrg,
        OAuthCreateUser,
        OAuthAppendUserLogin,
        OAuthRemoveUserLogin,
        OAuthLogin,
        AddUserToOrg,
        SetAsOrgAdmin,
        ClearOrgAdmin,
        ChangePassword,
        InviteUser,
        RegisterUser,
        AcceptInvite,
        DeleteUser,
        DeleteOrg,
        DisableUser,
        AcceptTermsAndConditions,
        GrantRole,
        RevokeRole,
    }

    [EntityDescription(Domains.SecurityDomain, UserAdminResources.Names.AuthenticationLogs_Title, LoggingResources.Names.LogRecord_Description,
       UserAdminResources.Names.AuthenticationLogs_Description, EntityDescriptionAttribute.EntityTypes.SimpleModel, typeof(UserAdminResources),
        GetListUrl: "/sys/auth/log", ListUIUrl: "/sysadmin/areas/logs", Icon: "icon-ae-coding-laptop")]
    public class AuthenticationLog : TableStorageEntity
    {
        public AuthenticationLog(AuthLogTypes authType)
        {
            AuthType = authType.ToString();
            PartitionKey = DateTime.UtcNow.ToDateOnly();
            RowKey = DateTime.Now.ToInverseTicksRowKey();
            TimeStamp = DateTime.UtcNow.ToJSONString();
        }

        public AuthenticationLog()
        {

        }

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
