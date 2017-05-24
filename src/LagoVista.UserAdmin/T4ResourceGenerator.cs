using System.Globalization;
using System.Reflection;

//Resources:UserAdminResources:AcceptInviteVM_Description
namespace LagoVista.UserAdmin.Resources
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LagoVista.UserAdmin.Resources.UserAdminResources", typeof(UserAdminResources).GetTypeInfo().Assembly);
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
//Resources:UserAdminResources:LocationAccount_Description

		public static string LocationAccount_Description { get { return GetResourceString("LocationAccount_Description"); } }
//Resources:UserAdminResources:LocationAccount_Help

		public static string LocationAccount_Help { get { return GetResourceString("LocationAccount_Help"); } }
//Resources:UserAdminResources:LocationAccount_Title

		public static string LocationAccount_Title { get { return GetResourceString("LocationAccount_Title"); } }
//Resources:UserAdminResources:LocationAccountRole_Description

		public static string LocationAccountRole_Description { get { return GetResourceString("LocationAccountRole_Description"); } }
//Resources:UserAdminResources:LocationAccountRole_Help

		public static string LocationAccountRole_Help { get { return GetResourceString("LocationAccountRole_Help"); } }
//Resources:UserAdminResources:LocationAccountRole_Title

		public static string LocationAccountRole_Title { get { return GetResourceString("LocationAccountRole_Title"); } }
//Resources:UserAdminResources:LocationNamespace_Help

		public static string LocationNamespace_Help { get { return GetResourceString("LocationNamespace_Help"); } }
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
//Resources:UserAdminResources:Organization_Account_Description

		public static string Organization_Account_Description { get { return GetResourceString("Organization_Account_Description"); } }
//Resources:UserAdminResources:Organization_Account_Help

		public static string Organization_Account_Help { get { return GetResourceString("Organization_Account_Help"); } }
//Resources:UserAdminResources:Organization_Account_Title

		public static string Organization_Account_Title { get { return GetResourceString("Organization_Account_Title"); } }
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
//Resources:UserAdminResources:Organization_WebSite

		public static string Organization_WebSite { get { return GetResourceString("Organization_WebSite"); } }
//Resources:UserAdminResources:OrganizationAccount_CouldntAdd

		public static string OrganizationAccount_CouldntAdd { get { return GetResourceString("OrganizationAccount_CouldntAdd"); } }
//Resources:UserAdminResources:OrganizationAccount_UserExists

		public static string OrganizationAccount_UserExists { get { return GetResourceString("OrganizationAccount_UserExists"); } }
//Resources:UserAdminResources:OrganizationAccountRole_Description

		public static string OrganizationAccountRole_Description { get { return GetResourceString("OrganizationAccountRole_Description"); } }
//Resources:UserAdminResources:OrganizationAccountRole_Help

		public static string OrganizationAccountRole_Help { get { return GetResourceString("OrganizationAccountRole_Help"); } }
//Resources:UserAdminResources:OrganizationAccountRole_Title

		public static string OrganizationAccountRole_Title { get { return GetResourceString("OrganizationAccountRole_Title"); } }
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
//Resources:UserAdminResources:OrganizationVM_Description

		public static string OrganizationVM_Description { get { return GetResourceString("OrganizationVM_Description"); } }
//Resources:UserAdminResources:OrganizationVM_Help

		public static string OrganizationVM_Help { get { return GetResourceString("OrganizationVM_Help"); } }
//Resources:UserAdminResources:OrganizationVM_Title

		public static string OrganizationVM_Title { get { return GetResourceString("OrganizationVM_Title"); } }
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
//Resources:UserAdminResources:UserInfo_Description

		public static string UserInfo_Description { get { return GetResourceString("UserInfo_Description"); } }
//Resources:UserAdminResources:UserInfo_Help

		public static string UserInfo_Help { get { return GetResourceString("UserInfo_Help"); } }
//Resources:UserAdminResources:UserInfo_Title

		public static string UserInfo_Title { get { return GetResourceString("UserInfo_Title"); } }
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
			public const string LocationAccount_Description = "LocationAccount_Description";
			public const string LocationAccount_Help = "LocationAccount_Help";
			public const string LocationAccount_Title = "LocationAccount_Title";
			public const string LocationAccountRole_Description = "LocationAccountRole_Description";
			public const string LocationAccountRole_Help = "LocationAccountRole_Help";
			public const string LocationAccountRole_Title = "LocationAccountRole_Title";
			public const string LocationNamespace_Help = "LocationNamespace_Help";
			public const string LocationVM_Description = "LocationVM_Description";
			public const string LocationVM_Help = "LocationVM_Help";
			public const string LocationVM_Title = "LocationVM_Title";
			public const string LoginVM_Description = "LoginVM_Description";
			public const string LoginVM_Help = "LoginVM_Help";
			public const string LoginVM_Title = "LoginVM_Title";
			public const string Organization = "Organization";
			public const string Organization_Account_Description = "Organization_Account_Description";
			public const string Organization_Account_Help = "Organization_Account_Help";
			public const string Organization_Account_Title = "Organization_Account_Title";
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
			public const string Organization_WebSite = "Organization_WebSite";
			public const string OrganizationAccount_CouldntAdd = "OrganizationAccount_CouldntAdd";
			public const string OrganizationAccount_UserExists = "OrganizationAccount_UserExists";
			public const string OrganizationAccountRole_Description = "OrganizationAccountRole_Description";
			public const string OrganizationAccountRole_Help = "OrganizationAccountRole_Help";
			public const string OrganizationAccountRole_Title = "OrganizationAccountRole_Title";
			public const string OrganizationDetailsVM_Description = "OrganizationDetailsVM_Description";
			public const string OrganizationDetailVM_Help = "OrganizationDetailVM_Help";
			public const string OrganizationDetailVM_Title = "OrganizationDetailVM_Title";
			public const string OrganizationLocation_NamespaceInUse = "OrganizationLocation_NamespaceInUse";
			public const string OrganizationNamespace_Help = "OrganizationNamespace_Help";
			public const string OrganizationVM_Description = "OrganizationVM_Description";
			public const string OrganizationVM_Help = "OrganizationVM_Help";
			public const string OrganizationVM_Title = "OrganizationVM_Title";
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
			public const string UserInfo_Description = "UserInfo_Description";
			public const string UserInfo_Help = "UserInfo_Help";
			public const string UserInfo_Title = "UserInfo_Title";
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
