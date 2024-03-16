using LagoVista.Core;
using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

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

    public class AuthenticationLog : TableStorageEntity
    {
        public AuthenticationLog(AuthLogTypes authType)
        {
            AuthType = authType.ToString();
            PartitionKey = Enum.GetName(typeof(AuthLogTypes), authType).ToString().ToLower(); ;
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
