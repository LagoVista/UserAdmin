using LagoVista.IoT.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.UserAdmin.Resources
{
    public class UserAdminErrorCodes
    {
        public static ErrorCode AuthInvalidCredentials => new ErrorCode() { Code = "AUTH001", Message = UserAdminResources.AuthErr_InvalidCredentials };
        public static ErrorCode AuthInvalidRefreshToken => new ErrorCode() { Code = "AUTH002", Message = UserAdminResources.AuthErr_InvalidRefreshToken };
        public static ErrorCode AuthRefreshTokenExpired => new ErrorCode() { Code = "AUTH003", Message = UserAdminResources.AuthErr_RefreshTokenExpired };
        public static ErrorCode AuthInvalidGrantType => new ErrorCode() { Code = "AUTH004", Message = UserAdminResources.AuthErr_InvalidGrantType };
        public static ErrorCode AuthCouldNotFindUserAccount => new ErrorCode() { Code = "AUTH005", Message = UserAdminResources.AuthErr_CouldNotFindUserAccount };
        public static ErrorCode AuthMissingAppId => new ErrorCode() { Code = "AUTH006", Message = UserAdminResources.AuthErr_MissingAppId };
        public static ErrorCode AuthMissingClientType => new ErrorCode() { Code = "AUTH007", Message = UserAdminResources.AuthErr_MissingClientType };
        public static ErrorCode AuthMissingAppInstanceId => new ErrorCode() { Code = "AUTH008", Message = UserAdminResources.AuthErr_MissingAppInstanceid };
        public static ErrorCode AuthMissingRefreshToken => new ErrorCode() { Code = "AUTH009", Message = UserAdminResources.AuthErr_MissingRefreshToken };
        public static ErrorCode AuthRefrshTokenInvalidFormat => new ErrorCode() { Code = "AUTH010", Message = UserAdminResources.AuthErr_RefreshToken_InvalidFormat };
        public static ErrorCode AuthRefrshTokenNotFound => new ErrorCode() { Code = "AUTH011", Message = UserAdminResources.AuthErr_RefreshToken_NotFound };

        public static ErrorCode AuthMissingEmail => new ErrorCode() { Code = "AUTH012", Message = UserAdminResources.AuthErr_MissingEmailAddress };
        public static ErrorCode AuthMissingPassword => new ErrorCode() { Code = "AUTH013", Message = UserAdminResources.AuthErr_MissingPassword };
        public static ErrorCode AuthEmailInvalidFormat => new ErrorCode() { Code = "AUTH014", Message = UserAdminResources.AuthErr_EmailInvalidFormat };
        public static ErrorCode AuthRefreshTokenNotInStorage => new ErrorCode() { Code = "AUTH015", Message = UserAdminResources.AuthErr_RefreshTokenNotInStoraage };
        public static ErrorCode AuthUserIsNullForRefresh => new ErrorCode() { Code = "AUTH016", Message = UserAdminResources.AuthErr_UserIsNullForRefresh };
        public static ErrorCode AuthRequestNull => new ErrorCode() { Code = "AUTH017", Message = UserAdminResources.AuthErr_AuthRequestNull };
        public static ErrorCode AuthMissingDeviceId => new ErrorCode() { Code = "AUTH018", Message = UserAdminResources.AuthErr_MissingDeviceId };
        public static ErrorCode RegMissingFirstLastName => new ErrorCode() { Code = "AUTH019", Message = UserAdminResources.RegErr_MissingFirstName };
        public static ErrorCode RegMissingLastName => new ErrorCode() { Code = "AUTH020", Message = UserAdminResources.RegErr_MissingLastName };
        public static ErrorCode RegMissingEmail => new ErrorCode() { Code = "AUTH021", Message = UserAdminResources.AuthErr_MissingEmailAddress };
        public static ErrorCode RegInvalidEmailAddress => new ErrorCode() { Code = "AUTH022", Message = UserAdminResources.RegErr_InvalidEmailAddress };
        public static ErrorCode RegMissingPhoneNumber => new ErrorCode() { Code = "AUTH023", Message = UserAdminResources.RegErr_MissingPhoneNumber };
        public static ErrorCode RegErrorSendingSMS => new ErrorCode() { Code = "AUTH024", Message = UserAdminResources.RegErr_ErrorSendingPhoneNumber };
        public static ErrorCode RegErrorSendingEmail => new ErrorCode() { Code = "AUTH025", Message = UserAdminResources.RegErr_ErrorSendingEmail };
        public static ErrorCode AuthOrgNotAuthorized => new ErrorCode() { Code = "AUTH026", Message = UserAdminResources.AuthErr_OrgNotAuthorized };
        public static ErrorCode AuthErrorUpdatingUser => new ErrorCode() { Code = "AUTH027", Message = UserAdminResources.AuthErr_ErrorUpdatingUser };


    }
}
