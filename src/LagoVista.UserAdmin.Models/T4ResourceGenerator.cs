using System.Globalization;
using System.Reflection;

//Resources:UserAdminResources:AcceptInviteVM_Description
namespace LagoVista.UserAdmin.Models.Resources
{
	public class UserAdminResources
	{
        private static global::System.Resources.ResourceManager _resourceManager;
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static global::System.Resources.ResourceManager ResourceManager 
		{
            get 
			{
                if (object.ReferenceEquals(_resourceManager, null)) 
				{
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LagoVista.UserAdmin.Models.UserAdminResources", typeof(UserAdminResources).GetTypeInfo().Assembly);
                    _resourceManager = temp;
                }
                return _resourceManager;
            }
        }
        
        /// <summary>
        ///   Returns the formatted resource string.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static string GetResourceString(string key, params string[] tokens)
		{
			var culture = CultureInfo.CurrentCulture;;
            var str = ResourceManager.GetString(key, culture);

			for(int i = 0; i < tokens.Length; i += 2)
				str = str.Replace(tokens[i], tokens[i+1]);
										
            return str;
        }
        
        /// <summary>
        ///   Returns the formatted resource string.
        /// </summary>
		/*
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        private static HtmlString GetResourceHtmlString(string key, params string[] tokens)
		{
			var str = GetResourceString(key, tokens);
							
			if(str.StartsWith("HTML:"))
				str = str.Substring(5);

			return new HtmlString(str);
        }*/
		
		public static string AcceptInviteVM_Description { get { return GetResourceString("AcceptInviteVM_Description"); } }
//Resources:UserAdminResources:AcceptInviteVM_Help

		public static string AcceptInviteVM_Help { get { return GetResourceString("AcceptInviteVM_Help"); } }
//Resources:UserAdminResources:AcceptInviteVM_Title

		public static string AcceptInviteVM_Title { get { return GetResourceString("AcceptInviteVM_Title"); } }
//Resources:UserAdminResources:Admin_Contact

		public static string Admin_Contact { get { return GetResourceString("Admin_Contact"); } }
//Resources:UserAdminResources:AppUser_ConfirmPassword

		public static string AppUser_ConfirmPassword { get { return GetResourceString("AppUser_ConfirmPassword"); } }
//Resources:UserAdminResources:AppUser_Description

		public static string AppUser_Description { get { return GetResourceString("AppUser_Description"); } }
//Resources:UserAdminResources:AppUser_Email

		public static string AppUser_Email { get { return GetResourceString("AppUser_Email"); } }
//Resources:UserAdminResources:AppUser_FirstName

		public static string AppUser_FirstName { get { return GetResourceString("AppUser_FirstName"); } }
//Resources:UserAdminResources:AppUser_Help

		public static string AppUser_Help { get { return GetResourceString("AppUser_Help"); } }
//Resources:UserAdminResources:AppUser_IsOrgAdmin

		public static string AppUser_IsOrgAdmin { get { return GetResourceString("AppUser_IsOrgAdmin"); } }
//Resources:UserAdminResources:AppUser_IsSystemAdmin

		public static string AppUser_IsSystemAdmin { get { return GetResourceString("AppUser_IsSystemAdmin"); } }
//Resources:UserAdminResources:AppUser_LastName

		public static string AppUser_LastName { get { return GetResourceString("AppUser_LastName"); } }
//Resources:UserAdminResources:AppUser_NewPassword

		public static string AppUser_NewPassword { get { return GetResourceString("AppUser_NewPassword"); } }
//Resources:UserAdminResources:AppUser_OldPassword

		public static string AppUser_OldPassword { get { return GetResourceString("AppUser_OldPassword"); } }
//Resources:UserAdminResources:AppUser_Password

		public static string AppUser_Password { get { return GetResourceString("AppUser_Password"); } }
//Resources:UserAdminResources:AppUser_PasswordConfirmPasswordMatch

		public static string AppUser_PasswordConfirmPasswordMatch { get { return GetResourceString("AppUser_PasswordConfirmPasswordMatch"); } }
//Resources:UserAdminResources:AppUser_PhoneNumber

		public static string AppUser_PhoneNumber { get { return GetResourceString("AppUser_PhoneNumber"); } }
//Resources:UserAdminResources:AppUser_PhoneVerificationCode

		public static string AppUser_PhoneVerificationCode { get { return GetResourceString("AppUser_PhoneVerificationCode"); } }
//Resources:UserAdminResources:AppUser_RememberMe

		public static string AppUser_RememberMe { get { return GetResourceString("AppUser_RememberMe"); } }
//Resources:UserAdminResources:AppUser_RememberMe_InThisBrowser

		public static string AppUser_RememberMe_InThisBrowser { get { return GetResourceString("AppUser_RememberMe_InThisBrowser"); } }
//Resources:UserAdminResources:AppUser_Title

		public static string AppUser_Title { get { return GetResourceString("AppUser_Title"); } }
//Resources:UserAdminResources:AssetSet_Description

		public static string AssetSet_Description { get { return GetResourceString("AssetSet_Description"); } }
//Resources:UserAdminResources:AssetSet_Help

		public static string AssetSet_Help { get { return GetResourceString("AssetSet_Help"); } }
//Resources:UserAdminResources:AssetSet_IsRestricted

		public static string AssetSet_IsRestricted { get { return GetResourceString("AssetSet_IsRestricted"); } }
//Resources:UserAdminResources:AssetSet_IsRestricted_Help

		public static string AssetSet_IsRestricted_Help { get { return GetResourceString("AssetSet_IsRestricted_Help"); } }
//Resources:UserAdminResources:AssetSet_Title

		public static string AssetSet_Title { get { return GetResourceString("AssetSet_Title"); } }
//Resources:UserAdminResources:AuthErr_AuthRequestNull

		public static string AuthErr_AuthRequestNull { get { return GetResourceString("AuthErr_AuthRequestNull"); } }
//Resources:UserAdminResources:AuthErr_CanNotRemoveSelfFromOrgAdmin

		public static string AuthErr_CanNotRemoveSelfFromOrgAdmin { get { return GetResourceString("AuthErr_CanNotRemoveSelfFromOrgAdmin"); } }
//Resources:UserAdminResources:AuthErr_CouldNotFindUser

		public static string AuthErr_CouldNotFindUser { get { return GetResourceString("AuthErr_CouldNotFindUser"); } }
//Resources:UserAdminResources:AuthErr_CouldNotFindUserAccount

		public static string AuthErr_CouldNotFindUserAccount { get { return GetResourceString("AuthErr_CouldNotFindUserAccount"); } }
//Resources:UserAdminResources:AuthErr_CurrentOrgAlreadySet

		public static string AuthErr_CurrentOrgAlreadySet { get { return GetResourceString("AuthErr_CurrentOrgAlreadySet"); } }
//Resources:UserAdminResources:AuthErr_EmailInvalidFormat

		public static string AuthErr_EmailInvalidFormat { get { return GetResourceString("AuthErr_EmailInvalidFormat"); } }
//Resources:UserAdminResources:AuthErr_ErrorUpdatingUser

		public static string AuthErr_ErrorUpdatingUser { get { return GetResourceString("AuthErr_ErrorUpdatingUser"); } }
//Resources:UserAdminResources:AuthErr_InvalidCredentials

		public static string AuthErr_InvalidCredentials { get { return GetResourceString("AuthErr_InvalidCredentials"); } }
//Resources:UserAdminResources:AuthErr_InvalidGrantType

		public static string AuthErr_InvalidGrantType { get { return GetResourceString("AuthErr_InvalidGrantType"); } }
//Resources:UserAdminResources:AuthErr_InvalidRefreshToken

		public static string AuthErr_InvalidRefreshToken { get { return GetResourceString("AuthErr_InvalidRefreshToken"); } }
//Resources:UserAdminResources:AuthErr_InviteNotActive

		public static string AuthErr_InviteNotActive { get { return GetResourceString("AuthErr_InviteNotActive"); } }
//Resources:UserAdminResources:AuthErr_MissingAppId

		public static string AuthErr_MissingAppId { get { return GetResourceString("AuthErr_MissingAppId"); } }
//Resources:UserAdminResources:AuthErr_MissingAppInstanceid

		public static string AuthErr_MissingAppInstanceid { get { return GetResourceString("AuthErr_MissingAppInstanceid"); } }
//Resources:UserAdminResources:AuthErr_MissingClientType

		public static string AuthErr_MissingClientType { get { return GetResourceString("AuthErr_MissingClientType"); } }
//Resources:UserAdminResources:AuthErr_MissingDeviceId

		public static string AuthErr_MissingDeviceId { get { return GetResourceString("AuthErr_MissingDeviceId"); } }
//Resources:UserAdminResources:AuthErr_MissingEmailAddress

		public static string AuthErr_MissingEmailAddress { get { return GetResourceString("AuthErr_MissingEmailAddress"); } }
//Resources:UserAdminResources:AuthErr_MissingPassword

		public static string AuthErr_MissingPassword { get { return GetResourceString("AuthErr_MissingPassword"); } }
//Resources:UserAdminResources:AuthErr_MissingRefreshToken

		public static string AuthErr_MissingRefreshToken { get { return GetResourceString("AuthErr_MissingRefreshToken"); } }
//Resources:UserAdminResources:AuthErr_NotOrgAdmin

		public static string AuthErr_NotOrgAdmin { get { return GetResourceString("AuthErr_NotOrgAdmin"); } }
//Resources:UserAdminResources:AuthErr_OrgNotAuthorized

		public static string AuthErr_OrgNotAuthorized { get { return GetResourceString("AuthErr_OrgNotAuthorized"); } }
//Resources:UserAdminResources:AuthErr_RefreshToken_InvalidFormat

		public static string AuthErr_RefreshToken_InvalidFormat { get { return GetResourceString("AuthErr_RefreshToken_InvalidFormat"); } }
//Resources:UserAdminResources:AuthErr_RefreshToken_NotFound

		public static string AuthErr_RefreshToken_NotFound { get { return GetResourceString("AuthErr_RefreshToken_NotFound"); } }
//Resources:UserAdminResources:AuthErr_RefreshTokenExpired

		public static string AuthErr_RefreshTokenExpired { get { return GetResourceString("AuthErr_RefreshTokenExpired"); } }
//Resources:UserAdminResources:AuthErr_RefreshTokenNotInStoraage

		public static string AuthErr_RefreshTokenNotInStoraage { get { return GetResourceString("AuthErr_RefreshTokenNotInStoraage"); } }
//Resources:UserAdminResources:AuthErr_UserIsNullForRefresh

		public static string AuthErr_UserIsNullForRefresh { get { return GetResourceString("AuthErr_UserIsNullForRefresh"); } }
//Resources:UserAdminResources:AuthErr_UserLockedOut

		public static string AuthErr_UserLockedOut { get { return GetResourceString("AuthErr_UserLockedOut"); } }
//Resources:UserAdminResources:AuthError_NotSysAdmin

		public static string AuthError_NotSysAdmin { get { return GetResourceString("AuthError_NotSysAdmin"); } }
//Resources:UserAdminResources:Billing_Contact

		public static string Billing_Contact { get { return GetResourceString("Billing_Contact"); } }
//Resources:UserAdminResources:ChangePasswordVM_Description

		public static string ChangePasswordVM_Description { get { return GetResourceString("ChangePasswordVM_Description"); } }
//Resources:UserAdminResources:ChangePasswordVM_Help

		public static string ChangePasswordVM_Help { get { return GetResourceString("ChangePasswordVM_Help"); } }
//Resources:UserAdminResources:ChangePasswordVM_Title

		public static string ChangePasswordVM_Title { get { return GetResourceString("ChangePasswordVM_Title"); } }
//Resources:UserAdminResources:Common_CreatedBy

		public static string Common_CreatedBy { get { return GetResourceString("Common_CreatedBy"); } }
//Resources:UserAdminResources:Common_CreationDate

		public static string Common_CreationDate { get { return GetResourceString("Common_CreationDate"); } }
//Resources:UserAdminResources:Common_Description

		public static string Common_Description { get { return GetResourceString("Common_Description"); } }
//Resources:UserAdminResources:Common_EmailAddress

		public static string Common_EmailAddress { get { return GetResourceString("Common_EmailAddress"); } }
//Resources:UserAdminResources:Common_Id

		public static string Common_Id { get { return GetResourceString("Common_Id"); } }
//Resources:UserAdminResources:Common_Key

		public static string Common_Key { get { return GetResourceString("Common_Key"); } }
//Resources:UserAdminResources:Common_Key_Help

		public static string Common_Key_Help { get { return GetResourceString("Common_Key_Help"); } }
//Resources:UserAdminResources:Common_Key_Validation

		public static string Common_Key_Validation { get { return GetResourceString("Common_Key_Validation"); } }
//Resources:UserAdminResources:Common_LastUpdatedBy

		public static string Common_LastUpdatedBy { get { return GetResourceString("Common_LastUpdatedBy"); } }
//Resources:UserAdminResources:Common_LastUpdatedDate

		public static string Common_LastUpdatedDate { get { return GetResourceString("Common_LastUpdatedDate"); } }
//Resources:UserAdminResources:Common_Name

		public static string Common_Name { get { return GetResourceString("Common_Name"); } }
//Resources:UserAdminResources:Common_Namespace

		public static string Common_Namespace { get { return GetResourceString("Common_Namespace"); } }
//Resources:UserAdminResources:Common_Notes

		public static string Common_Notes { get { return GetResourceString("Common_Notes"); } }
//Resources:UserAdminResources:Common_PhoneNumber

		public static string Common_PhoneNumber { get { return GetResourceString("Common_PhoneNumber"); } }
//Resources:UserAdminResources:Common_Role

		public static string Common_Role { get { return GetResourceString("Common_Role"); } }
//Resources:UserAdminResources:Common_Status

		public static string Common_Status { get { return GetResourceString("Common_Status"); } }
//Resources:UserAdminResources:CreateLocationVM_Description

		public static string CreateLocationVM_Description { get { return GetResourceString("CreateLocationVM_Description"); } }
//Resources:UserAdminResources:CreateLocationVM_Help

		public static string CreateLocationVM_Help { get { return GetResourceString("CreateLocationVM_Help"); } }
//Resources:UserAdminResources:CreateLocationVM_Title

		public static string CreateLocationVM_Title { get { return GetResourceString("CreateLocationVM_Title"); } }
//Resources:UserAdminResources:CreateOrganizationVM_Description

		public static string CreateOrganizationVM_Description { get { return GetResourceString("CreateOrganizationVM_Description"); } }
//Resources:UserAdminResources:CreateOrganizationVM_Help

		public static string CreateOrganizationVM_Help { get { return GetResourceString("CreateOrganizationVM_Help"); } }
//Resources:UserAdminResources:CreateOrganizationVM_Title

		public static string CreateOrganizationVM_Title { get { return GetResourceString("CreateOrganizationVM_Title"); } }
//Resources:UserAdminResources:Email_ResetPassword_Body

		public static string Email_ResetPassword_Body { get { return GetResourceString("Email_ResetPassword_Body"); } }
//Resources:UserAdminResources:Email_ResetPassword_Subject

		public static string Email_ResetPassword_Subject { get { return GetResourceString("Email_ResetPassword_Subject"); } }
//Resources:UserAdminResources:Email_RestPassword_ErrorSending

		public static string Email_RestPassword_ErrorSending { get { return GetResourceString("Email_RestPassword_ErrorSending"); } }
//Resources:UserAdminResources:Email_Verification_Body

		public static string Email_Verification_Body { get { return GetResourceString("Email_Verification_Body"); } }
//Resources:UserAdminResources:Email_Verification_Subject

		public static string Email_Verification_Subject { get { return GetResourceString("Email_Verification_Subject"); } }
//Resources:UserAdminResources:Err_PwdChange_CouldNotFindUser

		public static string Err_PwdChange_CouldNotFindUser { get { return GetResourceString("Err_PwdChange_CouldNotFindUser"); } }
//Resources:UserAdminResources:Err_PwdChange_MissingUserId

		public static string Err_PwdChange_MissingUserId { get { return GetResourceString("Err_PwdChange_MissingUserId"); } }
//Resources:UserAdminResources:Err_PwdChange_NewPassword_Missing

		public static string Err_PwdChange_NewPassword_Missing { get { return GetResourceString("Err_PwdChange_NewPassword_Missing"); } }
//Resources:UserAdminResources:Err_PwdChange_OldPassword_Missing

		public static string Err_PwdChange_OldPassword_Missing { get { return GetResourceString("Err_PwdChange_OldPassword_Missing"); } }
//Resources:UserAdminResources:Err_PwdChange_Token_Missing

		public static string Err_PwdChange_Token_Missing { get { return GetResourceString("Err_PwdChange_Token_Missing"); } }
//Resources:UserAdminResources:Err_PwdChange_UserId_Missing

		public static string Err_PwdChange_UserId_Missing { get { return GetResourceString("Err_PwdChange_UserId_Missing"); } }
//Resources:UserAdminResources:Err_PwdChange_UserIdMismatch

		public static string Err_PwdChange_UserIdMismatch { get { return GetResourceString("Err_PwdChange_UserIdMismatch"); } }
//Resources:UserAdminResources:Err_PwdReset_MissingNewPassword

		public static string Err_PwdReset_MissingNewPassword { get { return GetResourceString("Err_PwdReset_MissingNewPassword"); } }
//Resources:UserAdminResources:Err_PwdReset_MissingToken

		public static string Err_PwdReset_MissingToken { get { return GetResourceString("Err_PwdReset_MissingToken"); } }
//Resources:UserAdminResources:Err_PwdReset_MssingEmail

		public static string Err_PwdReset_MssingEmail { get { return GetResourceString("Err_PwdReset_MssingEmail"); } }
//Resources:UserAdminResources:Err_ResetPwd_CouldNotFindUser

		public static string Err_ResetPwd_CouldNotFindUser { get { return GetResourceString("Err_ResetPwd_CouldNotFindUser"); } }
//Resources:UserAdminResources:Err_UserId_DoesNotMatch

		public static string Err_UserId_DoesNotMatch { get { return GetResourceString("Err_UserId_DoesNotMatch"); } }
//Resources:UserAdminResources:ErrInvitation_AlreayAccepted

		public static string ErrInvitation_AlreayAccepted { get { return GetResourceString("ErrInvitation_AlreayAccepted"); } }
//Resources:UserAdminResources:ErrInvitation_CantFind

		public static string ErrInvitation_CantFind { get { return GetResourceString("ErrInvitation_CantFind"); } }
//Resources:UserAdminResources:ExternalLoginConfirmVM_Description

		public static string ExternalLoginConfirmVM_Description { get { return GetResourceString("ExternalLoginConfirmVM_Description"); } }
//Resources:UserAdminResources:ExternalLoginConfirmVM_Help

		public static string ExternalLoginConfirmVM_Help { get { return GetResourceString("ExternalLoginConfirmVM_Help"); } }
//Resources:UserAdminResources:ExternalLoginConfirmVM_Title

		public static string ExternalLoginConfirmVM_Title { get { return GetResourceString("ExternalLoginConfirmVM_Title"); } }
//Resources:UserAdminResources:ForgotPasswordVM_Description

		public static string ForgotPasswordVM_Description { get { return GetResourceString("ForgotPasswordVM_Description"); } }
//Resources:UserAdminResources:ForgotPasswordVM_Help

		public static string ForgotPasswordVM_Help { get { return GetResourceString("ForgotPasswordVM_Help"); } }
//Resources:UserAdminResources:ForgotPasswordVM_Title

		public static string ForgotPasswordVM_Title { get { return GetResourceString("ForgotPasswordVM_Title"); } }
//Resources:UserAdminResources:GeoLocation_Description

		public static string GeoLocation_Description { get { return GetResourceString("GeoLocation_Description"); } }
//Resources:UserAdminResources:GeoLocation_Help

		public static string GeoLocation_Help { get { return GetResourceString("GeoLocation_Help"); } }
//Resources:UserAdminResources:GeoLocation_Title

		public static string GeoLocation_Title { get { return GetResourceString("GeoLocation_Title"); } }
//Resources:UserAdminResources:ImageDetails_Description

		public static string ImageDetails_Description { get { return GetResourceString("ImageDetails_Description"); } }
//Resources:UserAdminResources:ImageDetails_Help

		public static string ImageDetails_Help { get { return GetResourceString("ImageDetails_Help"); } }
//Resources:UserAdminResources:ImageDetails_Title

		public static string ImageDetails_Title { get { return GetResourceString("ImageDetails_Title"); } }
//Resources:UserAdminResources:IndexVM_Description

		public static string IndexVM_Description { get { return GetResourceString("IndexVM_Description"); } }
//Resources:UserAdminResources:IndexVM_Help

		public static string IndexVM_Help { get { return GetResourceString("IndexVM_Help"); } }
//Resources:UserAdminResources:IndexVM_Title

		public static string IndexVM_Title { get { return GetResourceString("IndexVM_Title"); } }
//Resources:UserAdminResources:InuteUser_Status_Declined

		public static string InuteUser_Status_Declined { get { return GetResourceString("InuteUser_Status_Declined"); } }
//Resources:UserAdminResources:Invitation_Description

		public static string Invitation_Description { get { return GetResourceString("Invitation_Description"); } }
//Resources:UserAdminResources:Invitation_Help

		public static string Invitation_Help { get { return GetResourceString("Invitation_Help"); } }
//Resources:UserAdminResources:Invitation_Title

		public static string Invitation_Title { get { return GetResourceString("Invitation_Title"); } }
//Resources:UserAdminResources:Invite_Greeting_Subject

		public static string Invite_Greeting_Subject { get { return GetResourceString("Invite_Greeting_Subject"); } }
//Resources:UserAdminResources:InviteErr_EmailInvalid

		public static string InviteErr_EmailInvalid { get { return GetResourceString("InviteErr_EmailInvalid"); } }
//Resources:UserAdminResources:InviteErr_EmailIsEmpty

		public static string InviteErr_EmailIsEmpty { get { return GetResourceString("InviteErr_EmailIsEmpty"); } }
//Resources:UserAdminResources:InviteErr_InviteIsNull

		public static string InviteErr_InviteIsNull { get { return GetResourceString("InviteErr_InviteIsNull"); } }
//Resources:UserAdminResources:InviteErr_NameIsRequired

		public static string InviteErr_NameIsRequired { get { return GetResourceString("InviteErr_NameIsRequired"); } }
//Resources:UserAdminResources:InviteUser_AlreadyPartOfOrg

		public static string InviteUser_AlreadyPartOfOrg { get { return GetResourceString("InviteUser_AlreadyPartOfOrg"); } }
//Resources:UserAdminResources:InviteUser_ClickHere

		public static string InviteUser_ClickHere { get { return GetResourceString("InviteUser_ClickHere"); } }
//Resources:UserAdminResources:InviteUser_Greeting_Label

		public static string InviteUser_Greeting_Label { get { return GetResourceString("InviteUser_Greeting_Label"); } }
//Resources:UserAdminResources:InviteUser_Greeting_Message


		///<summary>
		///WIll replace [USERS_FULL_NAME] with the first/last name of current user, [ORG_NAME] as the name of the organization.
		///</summary>
		public static string InviteUser_Greeting_Message { get { return GetResourceString("InviteUser_Greeting_Message"); } }
//Resources:UserAdminResources:InviteUser_InvitedById


		///<summary>
		///No need to translate
		///</summary>
		public static string InviteUser_InvitedById { get { return GetResourceString("InviteUser_InvitedById"); } }
//Resources:UserAdminResources:InviteUser_InvitedByName

		public static string InviteUser_InvitedByName { get { return GetResourceString("InviteUser_InvitedByName"); } }
//Resources:UserAdminResources:InviteUser_Name

		public static string InviteUser_Name { get { return GetResourceString("InviteUser_Name"); } }
//Resources:UserAdminResources:InviteUser_Status

		public static string InviteUser_Status { get { return GetResourceString("InviteUser_Status"); } }
//Resources:UserAdminResources:InviteUser_Status_Accepted

		public static string InviteUser_Status_Accepted { get { return GetResourceString("InviteUser_Status_Accepted"); } }
//Resources:UserAdminResources:InviteUser_Status_Queued

		public static string InviteUser_Status_Queued { get { return GetResourceString("InviteUser_Status_Queued"); } }
//Resources:UserAdminResources:InviteUserVM_Description

		public static string InviteUserVM_Description { get { return GetResourceString("InviteUserVM_Description"); } }
//Resources:UserAdminResources:InviteUserVM_Help

		public static string InviteUserVM_Help { get { return GetResourceString("InviteUserVM_Help"); } }
//Resources:UserAdminResources:InviteUserVM_Title

		public static string InviteUserVM_Title { get { return GetResourceString("InviteUserVM_Title"); } }
//Resources:UserAdminResources:Location_Address1

		public static string Location_Address1 { get { return GetResourceString("Location_Address1"); } }
//Resources:UserAdminResources:Location_Address2

		public static string Location_Address2 { get { return GetResourceString("Location_Address2"); } }
//Resources:UserAdminResources:Location_Admin_Contact

		public static string Location_Admin_Contact { get { return GetResourceString("Location_Admin_Contact"); } }
//Resources:UserAdminResources:Location_City

		public static string Location_City { get { return GetResourceString("Location_City"); } }
//Resources:UserAdminResources:Location_Country

		public static string Location_Country { get { return GetResourceString("Location_Country"); } }
//Resources:UserAdminResources:Location_GeoLocation

		public static string Location_GeoLocation { get { return GetResourceString("Location_GeoLocation"); } }
//Resources:UserAdminResources:Location_LocationName

		public static string Location_LocationName { get { return GetResourceString("Location_LocationName"); } }
//Resources:UserAdminResources:Location_PostalCode

		public static string Location_PostalCode { get { return GetResourceString("Location_PostalCode"); } }
//Resources:UserAdminResources:Location_State

		public static string Location_State { get { return GetResourceString("Location_State"); } }
//Resources:UserAdminResources:LocationNamespace_Help

		public static string LocationNamespace_Help { get { return GetResourceString("LocationNamespace_Help"); } }
//Resources:UserAdminResources:LocationUser_Description

		public static string LocationUser_Description { get { return GetResourceString("LocationUser_Description"); } }
//Resources:UserAdminResources:LocationUser_Help

		public static string LocationUser_Help { get { return GetResourceString("LocationUser_Help"); } }
//Resources:UserAdminResources:LocationUser_Title

		public static string LocationUser_Title { get { return GetResourceString("LocationUser_Title"); } }
//Resources:UserAdminResources:LocationUserRole_Description

		public static string LocationUserRole_Description { get { return GetResourceString("LocationUserRole_Description"); } }
//Resources:UserAdminResources:LocationUserRole_Help

		public static string LocationUserRole_Help { get { return GetResourceString("LocationUserRole_Help"); } }
//Resources:UserAdminResources:LocationUserRole_Title

		public static string LocationUserRole_Title { get { return GetResourceString("LocationUserRole_Title"); } }
//Resources:UserAdminResources:LocationVM_Description

		public static string LocationVM_Description { get { return GetResourceString("LocationVM_Description"); } }
//Resources:UserAdminResources:LocationVM_Help

		public static string LocationVM_Help { get { return GetResourceString("LocationVM_Help"); } }
//Resources:UserAdminResources:LocationVM_Title

		public static string LocationVM_Title { get { return GetResourceString("LocationVM_Title"); } }
//Resources:UserAdminResources:LoginVM_Description

		public static string LoginVM_Description { get { return GetResourceString("LoginVM_Description"); } }
//Resources:UserAdminResources:LoginVM_Help

		public static string LoginVM_Help { get { return GetResourceString("LoginVM_Help"); } }
//Resources:UserAdminResources:LoginVM_Title

		public static string LoginVM_Title { get { return GetResourceString("LoginVM_Title"); } }
//Resources:UserAdminResources:Organization

		public static string Organization { get { return GetResourceString("Organization"); } }
//Resources:UserAdminResources:Organization_CantCreate

		public static string Organization_CantCreate { get { return GetResourceString("Organization_CantCreate"); } }
//Resources:UserAdminResources:Organization_Description

		public static string Organization_Description { get { return GetResourceString("Organization_Description"); } }
//Resources:UserAdminResources:Organization_Help

		public static string Organization_Help { get { return GetResourceString("Organization_Help"); } }
//Resources:UserAdminResources:Organization_Location

		public static string Organization_Location { get { return GetResourceString("Organization_Location"); } }
//Resources:UserAdminResources:Organization_Location_Description

		public static string Organization_Location_Description { get { return GetResourceString("Organization_Location_Description"); } }
//Resources:UserAdminResources:Organization_Location_Help

		public static string Organization_Location_Help { get { return GetResourceString("Organization_Location_Help"); } }
//Resources:UserAdminResources:Organization_Location_Title

		public static string Organization_Location_Title { get { return GetResourceString("Organization_Location_Title"); } }
//Resources:UserAdminResources:Organization_Locations

		public static string Organization_Locations { get { return GetResourceString("Organization_Locations"); } }
//Resources:UserAdminResources:Organization_Name

		public static string Organization_Name { get { return GetResourceString("Organization_Name"); } }
//Resources:UserAdminResources:Organization_NamespaceInUse

		public static string Organization_NamespaceInUse { get { return GetResourceString("Organization_NamespaceInUse"); } }
//Resources:UserAdminResources:Organization_Primary_Location

		public static string Organization_Primary_Location { get { return GetResourceString("Organization_Primary_Location"); } }
//Resources:UserAdminResources:Organization_Status_Active

		public static string Organization_Status_Active { get { return GetResourceString("Organization_Status_Active"); } }
//Resources:UserAdminResources:Organization_Status_Active_BehindPayments

		public static string Organization_Status_Active_BehindPayments { get { return GetResourceString("Organization_Status_Active_BehindPayments"); } }
//Resources:UserAdminResources:Organization_Status_Archived

		public static string Organization_Status_Archived { get { return GetResourceString("Organization_Status_Archived"); } }
//Resources:UserAdminResources:Organization_Title

		public static string Organization_Title { get { return GetResourceString("Organization_Title"); } }
//Resources:UserAdminResources:Organization_User_Description

		public static string Organization_User_Description { get { return GetResourceString("Organization_User_Description"); } }
//Resources:UserAdminResources:Organization_User_Help

		public static string Organization_User_Help { get { return GetResourceString("Organization_User_Help"); } }
//Resources:UserAdminResources:Organization_User_Title

		public static string Organization_User_Title { get { return GetResourceString("Organization_User_Title"); } }
//Resources:UserAdminResources:Organization_WebSite

		public static string Organization_WebSite { get { return GetResourceString("Organization_WebSite"); } }
//Resources:UserAdminResources:OrganizationDetailsVM_Description

		public static string OrganizationDetailsVM_Description { get { return GetResourceString("OrganizationDetailsVM_Description"); } }
//Resources:UserAdminResources:OrganizationDetailVM_Help

		public static string OrganizationDetailVM_Help { get { return GetResourceString("OrganizationDetailVM_Help"); } }
//Resources:UserAdminResources:OrganizationDetailVM_Title

		public static string OrganizationDetailVM_Title { get { return GetResourceString("OrganizationDetailVM_Title"); } }
//Resources:UserAdminResources:OrganizationLocation_NamespaceInUse

		public static string OrganizationLocation_NamespaceInUse { get { return GetResourceString("OrganizationLocation_NamespaceInUse"); } }
//Resources:UserAdminResources:OrganizationNamespace_Help

		public static string OrganizationNamespace_Help { get { return GetResourceString("OrganizationNamespace_Help"); } }
//Resources:UserAdminResources:OrganizationUser_CouldntAdd

		public static string OrganizationUser_CouldntAdd { get { return GetResourceString("OrganizationUser_CouldntAdd"); } }
//Resources:UserAdminResources:OrganizationUser_UserExists

		public static string OrganizationUser_UserExists { get { return GetResourceString("OrganizationUser_UserExists"); } }
//Resources:UserAdminResources:OrganizationUserRole_Description

		public static string OrganizationUserRole_Description { get { return GetResourceString("OrganizationUserRole_Description"); } }
//Resources:UserAdminResources:OrganizationUserRole_Help

		public static string OrganizationUserRole_Help { get { return GetResourceString("OrganizationUserRole_Help"); } }
//Resources:UserAdminResources:OrganizationUserRole_Title

		public static string OrganizationUserRole_Title { get { return GetResourceString("OrganizationUserRole_Title"); } }
//Resources:UserAdminResources:OrganizationVM_Description

		public static string OrganizationVM_Description { get { return GetResourceString("OrganizationVM_Description"); } }
//Resources:UserAdminResources:OrganizationVM_Help

		public static string OrganizationVM_Help { get { return GetResourceString("OrganizationVM_Help"); } }
//Resources:UserAdminResources:OrganizationVM_Title

		public static string OrganizationVM_Title { get { return GetResourceString("OrganizationVM_Title"); } }
//Resources:UserAdminResources:RegErr_ErrorSendingEmail

		public static string RegErr_ErrorSendingEmail { get { return GetResourceString("RegErr_ErrorSendingEmail"); } }
//Resources:UserAdminResources:RegErr_ErrorSendingPhoneNumber

		public static string RegErr_ErrorSendingPhoneNumber { get { return GetResourceString("RegErr_ErrorSendingPhoneNumber"); } }
//Resources:UserAdminResources:RegErr_InvalidEmailAddress

		public static string RegErr_InvalidEmailAddress { get { return GetResourceString("RegErr_InvalidEmailAddress"); } }
//Resources:UserAdminResources:RegErr_MissingFirstName

		public static string RegErr_MissingFirstName { get { return GetResourceString("RegErr_MissingFirstName"); } }
//Resources:UserAdminResources:RegErr_MissingLastName

		public static string RegErr_MissingLastName { get { return GetResourceString("RegErr_MissingLastName"); } }
//Resources:UserAdminResources:RegErr_MissingPassword

		public static string RegErr_MissingPassword { get { return GetResourceString("RegErr_MissingPassword"); } }
//Resources:UserAdminResources:RegErr_MissingPhoneNumber

		public static string RegErr_MissingPhoneNumber { get { return GetResourceString("RegErr_MissingPhoneNumber"); } }
//Resources:UserAdminResources:RegErr_UserAlreadyExists

		public static string RegErr_UserAlreadyExists { get { return GetResourceString("RegErr_UserAlreadyExists"); } }
//Resources:UserAdminResources:RegisterVM_Description

		public static string RegisterVM_Description { get { return GetResourceString("RegisterVM_Description"); } }
//Resources:UserAdminResources:RegisterVM_Help

		public static string RegisterVM_Help { get { return GetResourceString("RegisterVM_Help"); } }
//Resources:UserAdminResources:RegisterVM_Title

		public static string RegisterVM_Title { get { return GetResourceString("RegisterVM_Title"); } }
//Resources:UserAdminResources:ResetPassword_Description

		public static string ResetPassword_Description { get { return GetResourceString("ResetPassword_Description"); } }
//Resources:UserAdminResources:ResetPassword_Help

		public static string ResetPassword_Help { get { return GetResourceString("ResetPassword_Help"); } }
//Resources:UserAdminResources:ResetPassword_Title

		public static string ResetPassword_Title { get { return GetResourceString("ResetPassword_Title"); } }
//Resources:UserAdminResources:Role_Description

		public static string Role_Description { get { return GetResourceString("Role_Description"); } }
//Resources:UserAdminResources:Role_Help

		public static string Role_Help { get { return GetResourceString("Role_Help"); } }
//Resources:UserAdminResources:Role_Title

		public static string Role_Title { get { return GetResourceString("Role_Title"); } }
//Resources:UserAdminResources:SendCodeVM_Description

		public static string SendCodeVM_Description { get { return GetResourceString("SendCodeVM_Description"); } }
//Resources:UserAdminResources:SendCodeVM_Help

		public static string SendCodeVM_Help { get { return GetResourceString("SendCodeVM_Help"); } }
//Resources:UserAdminResources:SendCodeVM_Title

		public static string SendCodeVM_Title { get { return GetResourceString("SendCodeVM_Title"); } }
//Resources:UserAdminResources:SetPasswordVM_Description

		public static string SetPasswordVM_Description { get { return GetResourceString("SetPasswordVM_Description"); } }
//Resources:UserAdminResources:SetPasswordVM_Help

		public static string SetPasswordVM_Help { get { return GetResourceString("SetPasswordVM_Help"); } }
//Resources:UserAdminResources:SetPasswordVM_Title

		public static string SetPasswordVM_Title { get { return GetResourceString("SetPasswordVM_Title"); } }
//Resources:UserAdminResources:SMS_CouldNotVerify

		public static string SMS_CouldNotVerify { get { return GetResourceString("SMS_CouldNotVerify"); } }
//Resources:UserAdminResources:SMS_Verification_Body

		public static string SMS_Verification_Body { get { return GetResourceString("SMS_Verification_Body"); } }
//Resources:UserAdminResources:Subscription_Description

		public static string Subscription_Description { get { return GetResourceString("Subscription_Description"); } }
//Resources:UserAdminResources:Subscription_Help

		public static string Subscription_Help { get { return GetResourceString("Subscription_Help"); } }
//Resources:UserAdminResources:Subscription_Title

		public static string Subscription_Title { get { return GetResourceString("Subscription_Title"); } }
//Resources:UserAdminResources:Team_Description

		public static string Team_Description { get { return GetResourceString("Team_Description"); } }
//Resources:UserAdminResources:Team_Help

		public static string Team_Help { get { return GetResourceString("Team_Help"); } }
//Resources:UserAdminResources:Team_Title

		public static string Team_Title { get { return GetResourceString("Team_Title"); } }
//Resources:UserAdminResources:Technical_Contact

		public static string Technical_Contact { get { return GetResourceString("Technical_Contact"); } }
//Resources:UserAdminResources:UpdateLocationVM_Help

		public static string UpdateLocationVM_Help { get { return GetResourceString("UpdateLocationVM_Help"); } }
//Resources:UserAdminResources:UpdateLocationVM_Title

		public static string UpdateLocationVM_Title { get { return GetResourceString("UpdateLocationVM_Title"); } }
//Resources:UserAdminResources:UpdateLocatoinVM_Description

		public static string UpdateLocatoinVM_Description { get { return GetResourceString("UpdateLocatoinVM_Description"); } }
//Resources:UserAdminResources:UpdateOrganizationVM_Description

		public static string UpdateOrganizationVM_Description { get { return GetResourceString("UpdateOrganizationVM_Description"); } }
//Resources:UserAdminResources:UpdateOrganizationVM_Help

		public static string UpdateOrganizationVM_Help { get { return GetResourceString("UpdateOrganizationVM_Help"); } }
//Resources:UserAdminResources:UpdateOrganizationVM_Title

		public static string UpdateOrganizationVM_Title { get { return GetResourceString("UpdateOrganizationVM_Title"); } }
//Resources:UserAdminResources:User

		public static string User { get { return GetResourceString("User"); } }
//Resources:UserAdminResources:VerifyCodeVM_Description

		public static string VerifyCodeVM_Description { get { return GetResourceString("VerifyCodeVM_Description"); } }
//Resources:UserAdminResources:VerifyCodeVM_Help

		public static string VerifyCodeVM_Help { get { return GetResourceString("VerifyCodeVM_Help"); } }
//Resources:UserAdminResources:VerifyCodeVM_Title

		public static string VerifyCodeVM_Title { get { return GetResourceString("VerifyCodeVM_Title"); } }
//Resources:UserAdminResources:VerifyPhoneNumberVM_Description

		public static string VerifyPhoneNumberVM_Description { get { return GetResourceString("VerifyPhoneNumberVM_Description"); } }
//Resources:UserAdminResources:VerifyPhoneNumberVM_Help

		public static string VerifyPhoneNumberVM_Help { get { return GetResourceString("VerifyPhoneNumberVM_Help"); } }
//Resources:UserAdminResources:VerifyPhoneNumberVM_Title

		public static string VerifyPhoneNumberVM_Title { get { return GetResourceString("VerifyPhoneNumberVM_Title"); } }
//Resources:UserAdminResources:VerifyUser_BrowserRemembered

		public static string VerifyUser_BrowserRemembered { get { return GetResourceString("VerifyUser_BrowserRemembered"); } }
//Resources:UserAdminResources:VerifyUser_EmailConfirmed

		public static string VerifyUser_EmailConfirmed { get { return GetResourceString("VerifyUser_EmailConfirmed"); } }
//Resources:UserAdminResources:VerifyUser_ExistingPhoneNumber

		public static string VerifyUser_ExistingPhoneNumber { get { return GetResourceString("VerifyUser_ExistingPhoneNumber"); } }
//Resources:UserAdminResources:VerifyUser_PhoneConfirmed

		public static string VerifyUser_PhoneConfirmed { get { return GetResourceString("VerifyUser_PhoneConfirmed"); } }

		public static class Names
		{
			public const string AcceptInviteVM_Description = "AcceptInviteVM_Description";
			public const string AcceptInviteVM_Help = "AcceptInviteVM_Help";
			public const string AcceptInviteVM_Title = "AcceptInviteVM_Title";
			public const string Admin_Contact = "Admin_Contact";
			public const string AppUser_ConfirmPassword = "AppUser_ConfirmPassword";
			public const string AppUser_Description = "AppUser_Description";
			public const string AppUser_Email = "AppUser_Email";
			public const string AppUser_FirstName = "AppUser_FirstName";
			public const string AppUser_Help = "AppUser_Help";
			public const string AppUser_IsOrgAdmin = "AppUser_IsOrgAdmin";
			public const string AppUser_IsSystemAdmin = "AppUser_IsSystemAdmin";
			public const string AppUser_LastName = "AppUser_LastName";
			public const string AppUser_NewPassword = "AppUser_NewPassword";
			public const string AppUser_OldPassword = "AppUser_OldPassword";
			public const string AppUser_Password = "AppUser_Password";
			public const string AppUser_PasswordConfirmPasswordMatch = "AppUser_PasswordConfirmPasswordMatch";
			public const string AppUser_PhoneNumber = "AppUser_PhoneNumber";
			public const string AppUser_PhoneVerificationCode = "AppUser_PhoneVerificationCode";
			public const string AppUser_RememberMe = "AppUser_RememberMe";
			public const string AppUser_RememberMe_InThisBrowser = "AppUser_RememberMe_InThisBrowser";
			public const string AppUser_Title = "AppUser_Title";
			public const string AssetSet_Description = "AssetSet_Description";
			public const string AssetSet_Help = "AssetSet_Help";
			public const string AssetSet_IsRestricted = "AssetSet_IsRestricted";
			public const string AssetSet_IsRestricted_Help = "AssetSet_IsRestricted_Help";
			public const string AssetSet_Title = "AssetSet_Title";
			public const string AuthErr_AuthRequestNull = "AuthErr_AuthRequestNull";
			public const string AuthErr_CanNotRemoveSelfFromOrgAdmin = "AuthErr_CanNotRemoveSelfFromOrgAdmin";
			public const string AuthErr_CouldNotFindUser = "AuthErr_CouldNotFindUser";
			public const string AuthErr_CouldNotFindUserAccount = "AuthErr_CouldNotFindUserAccount";
			public const string AuthErr_CurrentOrgAlreadySet = "AuthErr_CurrentOrgAlreadySet";
			public const string AuthErr_EmailInvalidFormat = "AuthErr_EmailInvalidFormat";
			public const string AuthErr_ErrorUpdatingUser = "AuthErr_ErrorUpdatingUser";
			public const string AuthErr_InvalidCredentials = "AuthErr_InvalidCredentials";
			public const string AuthErr_InvalidGrantType = "AuthErr_InvalidGrantType";
			public const string AuthErr_InvalidRefreshToken = "AuthErr_InvalidRefreshToken";
			public const string AuthErr_InviteNotActive = "AuthErr_InviteNotActive";
			public const string AuthErr_MissingAppId = "AuthErr_MissingAppId";
			public const string AuthErr_MissingAppInstanceid = "AuthErr_MissingAppInstanceid";
			public const string AuthErr_MissingClientType = "AuthErr_MissingClientType";
			public const string AuthErr_MissingDeviceId = "AuthErr_MissingDeviceId";
			public const string AuthErr_MissingEmailAddress = "AuthErr_MissingEmailAddress";
			public const string AuthErr_MissingPassword = "AuthErr_MissingPassword";
			public const string AuthErr_MissingRefreshToken = "AuthErr_MissingRefreshToken";
			public const string AuthErr_NotOrgAdmin = "AuthErr_NotOrgAdmin";
			public const string AuthErr_OrgNotAuthorized = "AuthErr_OrgNotAuthorized";
			public const string AuthErr_RefreshToken_InvalidFormat = "AuthErr_RefreshToken_InvalidFormat";
			public const string AuthErr_RefreshToken_NotFound = "AuthErr_RefreshToken_NotFound";
			public const string AuthErr_RefreshTokenExpired = "AuthErr_RefreshTokenExpired";
			public const string AuthErr_RefreshTokenNotInStoraage = "AuthErr_RefreshTokenNotInStoraage";
			public const string AuthErr_UserIsNullForRefresh = "AuthErr_UserIsNullForRefresh";
			public const string AuthErr_UserLockedOut = "AuthErr_UserLockedOut";
			public const string AuthError_NotSysAdmin = "AuthError_NotSysAdmin";
			public const string Billing_Contact = "Billing_Contact";
			public const string ChangePasswordVM_Description = "ChangePasswordVM_Description";
			public const string ChangePasswordVM_Help = "ChangePasswordVM_Help";
			public const string ChangePasswordVM_Title = "ChangePasswordVM_Title";
			public const string Common_CreatedBy = "Common_CreatedBy";
			public const string Common_CreationDate = "Common_CreationDate";
			public const string Common_Description = "Common_Description";
			public const string Common_EmailAddress = "Common_EmailAddress";
			public const string Common_Id = "Common_Id";
			public const string Common_Key = "Common_Key";
			public const string Common_Key_Help = "Common_Key_Help";
			public const string Common_Key_Validation = "Common_Key_Validation";
			public const string Common_LastUpdatedBy = "Common_LastUpdatedBy";
			public const string Common_LastUpdatedDate = "Common_LastUpdatedDate";
			public const string Common_Name = "Common_Name";
			public const string Common_Namespace = "Common_Namespace";
			public const string Common_Notes = "Common_Notes";
			public const string Common_PhoneNumber = "Common_PhoneNumber";
			public const string Common_Role = "Common_Role";
			public const string Common_Status = "Common_Status";
			public const string CreateLocationVM_Description = "CreateLocationVM_Description";
			public const string CreateLocationVM_Help = "CreateLocationVM_Help";
			public const string CreateLocationVM_Title = "CreateLocationVM_Title";
			public const string CreateOrganizationVM_Description = "CreateOrganizationVM_Description";
			public const string CreateOrganizationVM_Help = "CreateOrganizationVM_Help";
			public const string CreateOrganizationVM_Title = "CreateOrganizationVM_Title";
			public const string Email_ResetPassword_Body = "Email_ResetPassword_Body";
			public const string Email_ResetPassword_Subject = "Email_ResetPassword_Subject";
			public const string Email_RestPassword_ErrorSending = "Email_RestPassword_ErrorSending";
			public const string Email_Verification_Body = "Email_Verification_Body";
			public const string Email_Verification_Subject = "Email_Verification_Subject";
			public const string Err_PwdChange_CouldNotFindUser = "Err_PwdChange_CouldNotFindUser";
			public const string Err_PwdChange_MissingUserId = "Err_PwdChange_MissingUserId";
			public const string Err_PwdChange_NewPassword_Missing = "Err_PwdChange_NewPassword_Missing";
			public const string Err_PwdChange_OldPassword_Missing = "Err_PwdChange_OldPassword_Missing";
			public const string Err_PwdChange_Token_Missing = "Err_PwdChange_Token_Missing";
			public const string Err_PwdChange_UserId_Missing = "Err_PwdChange_UserId_Missing";
			public const string Err_PwdChange_UserIdMismatch = "Err_PwdChange_UserIdMismatch";
			public const string Err_PwdReset_MissingNewPassword = "Err_PwdReset_MissingNewPassword";
			public const string Err_PwdReset_MissingToken = "Err_PwdReset_MissingToken";
			public const string Err_PwdReset_MssingEmail = "Err_PwdReset_MssingEmail";
			public const string Err_ResetPwd_CouldNotFindUser = "Err_ResetPwd_CouldNotFindUser";
			public const string Err_UserId_DoesNotMatch = "Err_UserId_DoesNotMatch";
			public const string ErrInvitation_AlreayAccepted = "ErrInvitation_AlreayAccepted";
			public const string ErrInvitation_CantFind = "ErrInvitation_CantFind";
			public const string ExternalLoginConfirmVM_Description = "ExternalLoginConfirmVM_Description";
			public const string ExternalLoginConfirmVM_Help = "ExternalLoginConfirmVM_Help";
			public const string ExternalLoginConfirmVM_Title = "ExternalLoginConfirmVM_Title";
			public const string ForgotPasswordVM_Description = "ForgotPasswordVM_Description";
			public const string ForgotPasswordVM_Help = "ForgotPasswordVM_Help";
			public const string ForgotPasswordVM_Title = "ForgotPasswordVM_Title";
			public const string GeoLocation_Description = "GeoLocation_Description";
			public const string GeoLocation_Help = "GeoLocation_Help";
			public const string GeoLocation_Title = "GeoLocation_Title";
			public const string ImageDetails_Description = "ImageDetails_Description";
			public const string ImageDetails_Help = "ImageDetails_Help";
			public const string ImageDetails_Title = "ImageDetails_Title";
			public const string IndexVM_Description = "IndexVM_Description";
			public const string IndexVM_Help = "IndexVM_Help";
			public const string IndexVM_Title = "IndexVM_Title";
			public const string InuteUser_Status_Declined = "InuteUser_Status_Declined";
			public const string Invitation_Description = "Invitation_Description";
			public const string Invitation_Help = "Invitation_Help";
			public const string Invitation_Title = "Invitation_Title";
			public const string Invite_Greeting_Subject = "Invite_Greeting_Subject";
			public const string InviteErr_EmailInvalid = "InviteErr_EmailInvalid";
			public const string InviteErr_EmailIsEmpty = "InviteErr_EmailIsEmpty";
			public const string InviteErr_InviteIsNull = "InviteErr_InviteIsNull";
			public const string InviteErr_NameIsRequired = "InviteErr_NameIsRequired";
			public const string InviteUser_AlreadyPartOfOrg = "InviteUser_AlreadyPartOfOrg";
			public const string InviteUser_ClickHere = "InviteUser_ClickHere";
			public const string InviteUser_Greeting_Label = "InviteUser_Greeting_Label";
			public const string InviteUser_Greeting_Message = "InviteUser_Greeting_Message";
			public const string InviteUser_InvitedById = "InviteUser_InvitedById";
			public const string InviteUser_InvitedByName = "InviteUser_InvitedByName";
			public const string InviteUser_Name = "InviteUser_Name";
			public const string InviteUser_Status = "InviteUser_Status";
			public const string InviteUser_Status_Accepted = "InviteUser_Status_Accepted";
			public const string InviteUser_Status_Queued = "InviteUser_Status_Queued";
			public const string InviteUserVM_Description = "InviteUserVM_Description";
			public const string InviteUserVM_Help = "InviteUserVM_Help";
			public const string InviteUserVM_Title = "InviteUserVM_Title";
			public const string Location_Address1 = "Location_Address1";
			public const string Location_Address2 = "Location_Address2";
			public const string Location_Admin_Contact = "Location_Admin_Contact";
			public const string Location_City = "Location_City";
			public const string Location_Country = "Location_Country";
			public const string Location_GeoLocation = "Location_GeoLocation";
			public const string Location_LocationName = "Location_LocationName";
			public const string Location_PostalCode = "Location_PostalCode";
			public const string Location_State = "Location_State";
			public const string LocationNamespace_Help = "LocationNamespace_Help";
			public const string LocationUser_Description = "LocationUser_Description";
			public const string LocationUser_Help = "LocationUser_Help";
			public const string LocationUser_Title = "LocationUser_Title";
			public const string LocationUserRole_Description = "LocationUserRole_Description";
			public const string LocationUserRole_Help = "LocationUserRole_Help";
			public const string LocationUserRole_Title = "LocationUserRole_Title";
			public const string LocationVM_Description = "LocationVM_Description";
			public const string LocationVM_Help = "LocationVM_Help";
			public const string LocationVM_Title = "LocationVM_Title";
			public const string LoginVM_Description = "LoginVM_Description";
			public const string LoginVM_Help = "LoginVM_Help";
			public const string LoginVM_Title = "LoginVM_Title";
			public const string Organization = "Organization";
			public const string Organization_CantCreate = "Organization_CantCreate";
			public const string Organization_Description = "Organization_Description";
			public const string Organization_Help = "Organization_Help";
			public const string Organization_Location = "Organization_Location";
			public const string Organization_Location_Description = "Organization_Location_Description";
			public const string Organization_Location_Help = "Organization_Location_Help";
			public const string Organization_Location_Title = "Organization_Location_Title";
			public const string Organization_Locations = "Organization_Locations";
			public const string Organization_Name = "Organization_Name";
			public const string Organization_NamespaceInUse = "Organization_NamespaceInUse";
			public const string Organization_Primary_Location = "Organization_Primary_Location";
			public const string Organization_Status_Active = "Organization_Status_Active";
			public const string Organization_Status_Active_BehindPayments = "Organization_Status_Active_BehindPayments";
			public const string Organization_Status_Archived = "Organization_Status_Archived";
			public const string Organization_Title = "Organization_Title";
			public const string Organization_User_Description = "Organization_User_Description";
			public const string Organization_User_Help = "Organization_User_Help";
			public const string Organization_User_Title = "Organization_User_Title";
			public const string Organization_WebSite = "Organization_WebSite";
			public const string OrganizationDetailsVM_Description = "OrganizationDetailsVM_Description";
			public const string OrganizationDetailVM_Help = "OrganizationDetailVM_Help";
			public const string OrganizationDetailVM_Title = "OrganizationDetailVM_Title";
			public const string OrganizationLocation_NamespaceInUse = "OrganizationLocation_NamespaceInUse";
			public const string OrganizationNamespace_Help = "OrganizationNamespace_Help";
			public const string OrganizationUser_CouldntAdd = "OrganizationUser_CouldntAdd";
			public const string OrganizationUser_UserExists = "OrganizationUser_UserExists";
			public const string OrganizationUserRole_Description = "OrganizationUserRole_Description";
			public const string OrganizationUserRole_Help = "OrganizationUserRole_Help";
			public const string OrganizationUserRole_Title = "OrganizationUserRole_Title";
			public const string OrganizationVM_Description = "OrganizationVM_Description";
			public const string OrganizationVM_Help = "OrganizationVM_Help";
			public const string OrganizationVM_Title = "OrganizationVM_Title";
			public const string RegErr_ErrorSendingEmail = "RegErr_ErrorSendingEmail";
			public const string RegErr_ErrorSendingPhoneNumber = "RegErr_ErrorSendingPhoneNumber";
			public const string RegErr_InvalidEmailAddress = "RegErr_InvalidEmailAddress";
			public const string RegErr_MissingFirstName = "RegErr_MissingFirstName";
			public const string RegErr_MissingLastName = "RegErr_MissingLastName";
			public const string RegErr_MissingPassword = "RegErr_MissingPassword";
			public const string RegErr_MissingPhoneNumber = "RegErr_MissingPhoneNumber";
			public const string RegErr_UserAlreadyExists = "RegErr_UserAlreadyExists";
			public const string RegisterVM_Description = "RegisterVM_Description";
			public const string RegisterVM_Help = "RegisterVM_Help";
			public const string RegisterVM_Title = "RegisterVM_Title";
			public const string ResetPassword_Description = "ResetPassword_Description";
			public const string ResetPassword_Help = "ResetPassword_Help";
			public const string ResetPassword_Title = "ResetPassword_Title";
			public const string Role_Description = "Role_Description";
			public const string Role_Help = "Role_Help";
			public const string Role_Title = "Role_Title";
			public const string SendCodeVM_Description = "SendCodeVM_Description";
			public const string SendCodeVM_Help = "SendCodeVM_Help";
			public const string SendCodeVM_Title = "SendCodeVM_Title";
			public const string SetPasswordVM_Description = "SetPasswordVM_Description";
			public const string SetPasswordVM_Help = "SetPasswordVM_Help";
			public const string SetPasswordVM_Title = "SetPasswordVM_Title";
			public const string SMS_CouldNotVerify = "SMS_CouldNotVerify";
			public const string SMS_Verification_Body = "SMS_Verification_Body";
			public const string Subscription_Description = "Subscription_Description";
			public const string Subscription_Help = "Subscription_Help";
			public const string Subscription_Title = "Subscription_Title";
			public const string Team_Description = "Team_Description";
			public const string Team_Help = "Team_Help";
			public const string Team_Title = "Team_Title";
			public const string Technical_Contact = "Technical_Contact";
			public const string UpdateLocationVM_Help = "UpdateLocationVM_Help";
			public const string UpdateLocationVM_Title = "UpdateLocationVM_Title";
			public const string UpdateLocatoinVM_Description = "UpdateLocatoinVM_Description";
			public const string UpdateOrganizationVM_Description = "UpdateOrganizationVM_Description";
			public const string UpdateOrganizationVM_Help = "UpdateOrganizationVM_Help";
			public const string UpdateOrganizationVM_Title = "UpdateOrganizationVM_Title";
			public const string User = "User";
			public const string VerifyCodeVM_Description = "VerifyCodeVM_Description";
			public const string VerifyCodeVM_Help = "VerifyCodeVM_Help";
			public const string VerifyCodeVM_Title = "VerifyCodeVM_Title";
			public const string VerifyPhoneNumberVM_Description = "VerifyPhoneNumberVM_Description";
			public const string VerifyPhoneNumberVM_Help = "VerifyPhoneNumberVM_Help";
			public const string VerifyPhoneNumberVM_Title = "VerifyPhoneNumberVM_Title";
			public const string VerifyUser_BrowserRemembered = "VerifyUser_BrowserRemembered";
			public const string VerifyUser_EmailConfirmed = "VerifyUser_EmailConfirmed";
			public const string VerifyUser_ExistingPhoneNumber = "VerifyUser_ExistingPhoneNumber";
			public const string VerifyUser_PhoneConfirmed = "VerifyUser_PhoneConfirmed";
		}
	}
 
}
