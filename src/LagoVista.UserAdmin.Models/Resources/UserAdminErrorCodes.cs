﻿using LagoVista.IoT.Logging;
using LagoVista.UserAdmin.Models.Resources;
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
        public static ErrorCode AuthSingleuseToken_TokenNotFound => new ErrorCode() { Code = "AUTH039", Message = UserAdminResources.AuthSingleuseToken_TokenNotFound };
        public static ErrorCode AuthSingleuseToken_UserNotFound => new ErrorCode() { Code = "AUTH040", Message = UserAdminResources.AuthSingleuseToken_UserNotFound };

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
        public static ErrorCode RegMissingPassword => new ErrorCode() { Code = "AUTH022", Message = UserAdminResources.RegErr_MissingPassword };
        public static ErrorCode RegInvalidEmailAddress => new ErrorCode() { Code = "AUTH023", Message = UserAdminResources.RegErr_InvalidEmailAddress };
        public static ErrorCode RegMissingPhoneNumber => new ErrorCode() { Code = "AUTH024", Message = UserAdminResources.RegErr_MissingPhoneNumber };
        public static ErrorCode RegErrorSendingSMS => new ErrorCode() { Code = "AUTH025", Message = UserAdminResources.RegErr_ErrorSendingPhoneNumber };
        public static ErrorCode RegErrorSendingEmail => new ErrorCode() { Code = "AUTH026", Message = UserAdminResources.RegErr_ErrorSendingEmail };
        public static ErrorCode RegErrorUserExists => new ErrorCode() { Code = "AUTH027", Message = UserAdminResources.RegErr_UserAlreadyExists };
        public static ErrorCode AuthOrgNotAuthorized => new ErrorCode() { Code = "AUTH028", Message = UserAdminResources.AuthErr_OrgNotAuthorized };
        public static ErrorCode AuthErrorUpdatingUser => new ErrorCode() { Code = "AUTH029", Message = UserAdminResources.AuthErr_ErrorUpdatingUser };
        public static ErrorCode AuthUserLockedOut => new ErrorCode() { Code = "AUTH030", Message = UserAdminResources.AuthErr_UserLockedOut };

        public static ErrorCode AuthNotSysAdmin => new ErrorCode() { Code = "AUTH031", Message = UserAdminResources.AuthError_NotSysAdmin };

        public static ErrorCode AuthNotOrgAdmin => new ErrorCode() { Code = "AUTH032", Message = UserAdminResources.AuthErr_NotOrgAdmin };
        public static ErrorCode AuthCantRemoveSelfFromOrgAdmin => new ErrorCode() { Code = "AUTH033", Message = UserAdminResources.AuthErr_CanNotRemoveSelfFromOrgAdmin };

        public static ErrorCode AuthCouldNotFindUser => new ErrorCode() { Code = "AUTH034", Message = UserAdminResources.AuthErr_CouldNotFindUser };

        public static ErrorCode AuthAlreadyInOrg => new ErrorCode() { Code = "AUTH035", Message = UserAdminResources.AuthErr_CurrentOrgAlreadySet };
        public static ErrorCode AuthInviteNotActive => new ErrorCode() { Code = "AUTH036", Message = UserAdminResources.AuthErr_InviteNotActive };

        public static ErrorCode AuthMissingRepoIdForDeviceUser => new ErrorCode() { Code = "AUTH037", Message = "When logging in as a device user you must provide a device repo id." };

        public static ErrorCode RegisterUserExists_3rdParty => new ErrorCode() { Code = "AUTH038", Message = UserAdminResources.RegisterUserExists_3rdParty };


        public static ErrorCode InviteIsNull => new ErrorCode() { Code = "INVT001", Message = UserAdminResources.InviteErr_InviteIsNull };
        public static ErrorCode InviteEmailIsEmpty=> new ErrorCode() { Code = "INVT002", Message = UserAdminResources.InviteErr_EmailIsEmpty };
        public static ErrorCode InviteNameIsEmpty => new ErrorCode() { Code = "INVT003", Message = UserAdminResources.InviteErr_NameIsRequired };
        public static ErrorCode InviteEmailIsInvalid => new ErrorCode() { Code = "INVT004", Message = UserAdminResources.InviteErr_EmailInvalid };
    }
}
