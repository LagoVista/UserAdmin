/*2/12/2026 10:53:05 AM*/
using System.Globalization;
using System.Reflection;

//Resources:UserAdminResources:AcceptInviteResponse_Description
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LagoVista.UserAdmin.Models.Resources.UserAdminResources", typeof(UserAdminResources).GetTypeInfo().Assembly);
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
		
		public static string AcceptInviteResponse_Description { get { return GetResourceString("AcceptInviteResponse_Description"); } }
//Resources:UserAdminResources:AcceptInviteResponse_Help

		public static string AcceptInviteResponse_Help { get { return GetResourceString("AcceptInviteResponse_Help"); } }
//Resources:UserAdminResources:AcceptInviteResponse_Name

		public static string AcceptInviteResponse_Name { get { return GetResourceString("AcceptInviteResponse_Name"); } }
//Resources:UserAdminResources:AcceptInviteVM_Description

		public static string AcceptInviteVM_Description { get { return GetResourceString("AcceptInviteVM_Description"); } }
//Resources:UserAdminResources:AcceptInviteVM_Help

		public static string AcceptInviteVM_Help { get { return GetResourceString("AcceptInviteVM_Help"); } }
//Resources:UserAdminResources:AcceptInviteVM_Title

		public static string AcceptInviteVM_Title { get { return GetResourceString("AcceptInviteVM_Title"); } }
//Resources:UserAdminResources:AccessLog_Description

		public static string AccessLog_Description { get { return GetResourceString("AccessLog_Description"); } }
//Resources:UserAdminResources:AccessLog_Help

		public static string AccessLog_Help { get { return GetResourceString("AccessLog_Help"); } }
//Resources:UserAdminResources:AccessLog_Name

		public static string AccessLog_Name { get { return GetResourceString("AccessLog_Name"); } }
//Resources:UserAdminResources:Admin_Contact

		public static string Admin_Contact { get { return GetResourceString("Admin_Contact"); } }
//Resources:UserAdminResources:AppUser_Bio

		public static string AppUser_Bio { get { return GetResourceString("AppUser_Bio"); } }
//Resources:UserAdminResources:AppUser_City

		public static string AppUser_City { get { return GetResourceString("AppUser_City"); } }
//Resources:UserAdminResources:AppUser_ConfirmPassword

		public static string AppUser_ConfirmPassword { get { return GetResourceString("AppUser_ConfirmPassword"); } }
//Resources:UserAdminResources:AppUser_Country

		public static string AppUser_Country { get { return GetResourceString("AppUser_Country"); } }
//Resources:UserAdminResources:AppUser_Description

		public static string AppUser_Description { get { return GetResourceString("AppUser_Description"); } }
//Resources:UserAdminResources:AppUser_Email

		public static string AppUser_Email { get { return GetResourceString("AppUser_Email"); } }
//Resources:UserAdminResources:AppUser_EmailSignature

		public static string AppUser_EmailSignature { get { return GetResourceString("AppUser_EmailSignature"); } }
//Resources:UserAdminResources:AppUser_FirstName

		public static string AppUser_FirstName { get { return GetResourceString("AppUser_FirstName"); } }
//Resources:UserAdminResources:AppUser_Help

		public static string AppUser_Help { get { return GetResourceString("AppUser_Help"); } }
//Resources:UserAdminResources:Appuser_IsAccountDisabled

		public static string Appuser_IsAccountDisabled { get { return GetResourceString("Appuser_IsAccountDisabled"); } }
//Resources:UserAdminResources:AppUser_IsAppBuilder

		public static string AppUser_IsAppBuilder { get { return GetResourceString("AppUser_IsAppBuilder"); } }
//Resources:UserAdminResources:AppUser_IsOrgAdmin

		public static string AppUser_IsOrgAdmin { get { return GetResourceString("AppUser_IsOrgAdmin"); } }
//Resources:UserAdminResources:Appuser_IsRuntimeUser

		public static string Appuser_IsRuntimeUser { get { return GetResourceString("Appuser_IsRuntimeUser"); } }
//Resources:UserAdminResources:AppUser_IsSystemAdmin

		public static string AppUser_IsSystemAdmin { get { return GetResourceString("AppUser_IsSystemAdmin"); } }
//Resources:UserAdminResources:AppUser_IsUserDevice

		public static string AppUser_IsUserDevice { get { return GetResourceString("AppUser_IsUserDevice"); } }
//Resources:UserAdminResources:AppUser_Language

		public static string AppUser_Language { get { return GetResourceString("AppUser_Language"); } }
//Resources:UserAdminResources:AppUser_Language_Select

		public static string AppUser_Language_Select { get { return GetResourceString("AppUser_Language_Select"); } }
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
//Resources:UserAdminResources:AppUser_PrimaryOrganization

		public static string AppUser_PrimaryOrganization { get { return GetResourceString("AppUser_PrimaryOrganization"); } }
//Resources:UserAdminResources:AppUser_PrimaryOrganization_Help

		public static string AppUser_PrimaryOrganization_Help { get { return GetResourceString("AppUser_PrimaryOrganization_Help"); } }
//Resources:UserAdminResources:AppUser_RememberMe

		public static string AppUser_RememberMe { get { return GetResourceString("AppUser_RememberMe"); } }
//Resources:UserAdminResources:AppUser_RememberMe_InThisBrowser

		public static string AppUser_RememberMe_InThisBrowser { get { return GetResourceString("AppUser_RememberMe_InThisBrowser"); } }
//Resources:UserAdminResources:AppUser_SSN

		public static string AppUser_SSN { get { return GetResourceString("AppUser_SSN"); } }
//Resources:UserAdminResources:AppUser_SSN_Help

		public static string AppUser_SSN_Help { get { return GetResourceString("AppUser_SSN_Help"); } }
//Resources:UserAdminResources:AppUser_StateProvince

		public static string AppUser_StateProvince { get { return GetResourceString("AppUser_StateProvince"); } }
//Resources:UserAdminResources:AppUser_TeamsAccountName

		public static string AppUser_TeamsAccountName { get { return GetResourceString("AppUser_TeamsAccountName"); } }
//Resources:UserAdminResources:AppUser_Title

		public static string AppUser_Title { get { return GetResourceString("AppUser_Title"); } }
//Resources:UserAdminResources:AppUser_UserName

		public static string AppUser_UserName { get { return GetResourceString("AppUser_UserName"); } }
//Resources:UserAdminResources:AppUser_UserTitle

		public static string AppUser_UserTitle { get { return GetResourceString("AppUser_UserTitle"); } }
//Resources:UserAdminResources:AppUserInboxItem_Description

		public static string AppUserInboxItem_Description { get { return GetResourceString("AppUserInboxItem_Description"); } }
//Resources:UserAdminResources:AppUserInboxItem_Help

		public static string AppUserInboxItem_Help { get { return GetResourceString("AppUserInboxItem_Help"); } }
//Resources:UserAdminResources:AppUserInboxItem_Name

		public static string AppUserInboxItem_Name { get { return GetResourceString("AppUserInboxItem_Name"); } }
//Resources:UserAdminResources:AppUserTestingDSL_Action

		public static string AppUserTestingDSL_Action { get { return GetResourceString("AppUserTestingDSL_Action"); } }
//Resources:UserAdminResources:AppUserTestingDSL_Action_Help

		public static string AppUserTestingDSL_Action_Help { get { return GetResourceString("AppUserTestingDSL_Action_Help"); } }
//Resources:UserAdminResources:AppUserTestingDSL_AuthView

		public static string AppUserTestingDSL_AuthView { get { return GetResourceString("AppUserTestingDSL_AuthView"); } }
//Resources:UserAdminResources:AppUserTestingDSL_AuthView_Help

		public static string AppUserTestingDSL_AuthView_Help { get { return GetResourceString("AppUserTestingDSL_AuthView_Help"); } }
//Resources:UserAdminResources:AppUserTestingDSL_Expected

		public static string AppUserTestingDSL_Expected { get { return GetResourceString("AppUserTestingDSL_Expected"); } }
//Resources:UserAdminResources:AppUserTestingDSL_Expected_Help

		public static string AppUserTestingDSL_Expected_Help { get { return GetResourceString("AppUserTestingDSL_Expected_Help"); } }
//Resources:UserAdminResources:AppUserTestingDSL_Inputs

		public static string AppUserTestingDSL_Inputs { get { return GetResourceString("AppUserTestingDSL_Inputs"); } }
//Resources:UserAdminResources:AppUserTestingDSL_Inputs_Help

		public static string AppUserTestingDSL_Inputs_Help { get { return GetResourceString("AppUserTestingDSL_Inputs_Help"); } }
//Resources:UserAdminResources:AppUserTestingDSL_Preconditions

		public static string AppUserTestingDSL_Preconditions { get { return GetResourceString("AppUserTestingDSL_Preconditions"); } }
//Resources:UserAdminResources:AppUserTestingDSL_Preconditions_Help

		public static string AppUserTestingDSL_Preconditions_Help { get { return GetResourceString("AppUserTestingDSL_Preconditions_Help"); } }
//Resources:UserAdminResources:AppUserTestingExpectedOutcome_Description

		public static string AppUserTestingExpectedOutcome_Description { get { return GetResourceString("AppUserTestingExpectedOutcome_Description"); } }
//Resources:UserAdminResources:AppUserTestingExpectedOutcome_ExpectedAuthLogEvents

		public static string AppUserTestingExpectedOutcome_ExpectedAuthLogEvents { get { return GetResourceString("AppUserTestingExpectedOutcome_ExpectedAuthLogEvents"); } }
//Resources:UserAdminResources:AppUserTestingExpectedOutcome_ExpectedAuthLogEvents_Help

		public static string AppUserTestingExpectedOutcome_ExpectedAuthLogEvents_Help { get { return GetResourceString("AppUserTestingExpectedOutcome_ExpectedAuthLogEvents_Help"); } }
//Resources:UserAdminResources:AppUserTestingExpectedOutcome_ExpectedLanding

		public static string AppUserTestingExpectedOutcome_ExpectedLanding { get { return GetResourceString("AppUserTestingExpectedOutcome_ExpectedLanding"); } }
//Resources:UserAdminResources:AppUserTestingExpectedOutcome_ExpectedLanding_Help

		public static string AppUserTestingExpectedOutcome_ExpectedLanding_Help { get { return GetResourceString("AppUserTestingExpectedOutcome_ExpectedLanding_Help"); } }
//Resources:UserAdminResources:AppUserTestingExpectedOutcome_Help

		public static string AppUserTestingExpectedOutcome_Help { get { return GetResourceString("AppUserTestingExpectedOutcome_Help"); } }
//Resources:UserAdminResources:AppUserTestingExpectedOutcome_Postconditions

		public static string AppUserTestingExpectedOutcome_Postconditions { get { return GetResourceString("AppUserTestingExpectedOutcome_Postconditions"); } }
//Resources:UserAdminResources:AppUserTestingExpectedOutcome_Postconditions_Help

		public static string AppUserTestingExpectedOutcome_Postconditions_Help { get { return GetResourceString("AppUserTestingExpectedOutcome_Postconditions_Help"); } }
//Resources:UserAdminResources:AppUserTestingExpectedOutcome_Title

		public static string AppUserTestingExpectedOutcome_Title { get { return GetResourceString("AppUserTestingExpectedOutcome_Title"); } }
//Resources:UserAdminResources:AppUserTestRun_Description

		public static string AppUserTestRun_Description { get { return GetResourceString("AppUserTestRun_Description"); } }
//Resources:UserAdminResources:AppUserTestRun_Help

		public static string AppUserTestRun_Help { get { return GetResourceString("AppUserTestRun_Help"); } }
//Resources:UserAdminResources:AppUserTestRun_Name

		public static string AppUserTestRun_Name { get { return GetResourceString("AppUserTestRun_Name"); } }
//Resources:UserAdminResources:AppUserTestRunEvent_Description

		public static string AppUserTestRunEvent_Description { get { return GetResourceString("AppUserTestRunEvent_Description"); } }
//Resources:UserAdminResources:AppUserTestRunEvent_Help

		public static string AppUserTestRunEvent_Help { get { return GetResourceString("AppUserTestRunEvent_Help"); } }
//Resources:UserAdminResources:AppUserTestRunEvent_Name

		public static string AppUserTestRunEvent_Name { get { return GetResourceString("AppUserTestRunEvent_Name"); } }
//Resources:UserAdminResources:AppUserTestRunSummary_Description

		public static string AppUserTestRunSummary_Description { get { return GetResourceString("AppUserTestRunSummary_Description"); } }
//Resources:UserAdminResources:AppUserTestRunSummary_Help

		public static string AppUserTestRunSummary_Help { get { return GetResourceString("AppUserTestRunSummary_Help"); } }
//Resources:UserAdminResources:AppUserTestRunSummary_Name

		public static string AppUserTestRunSummary_Name { get { return GetResourceString("AppUserTestRunSummary_Name"); } }
//Resources:UserAdminResources:AppUserTestScenario_AuthView

		public static string AppUserTestScenario_AuthView { get { return GetResourceString("AppUserTestScenario_AuthView"); } }
//Resources:UserAdminResources:AppUserTestScenario_AuthView_Help

		public static string AppUserTestScenario_AuthView_Help { get { return GetResourceString("AppUserTestScenario_AuthView_Help"); } }
//Resources:UserAdminResources:Area_Help

		public static string Area_Help { get { return GetResourceString("Area_Help"); } }
//Resources:UserAdminResources:Area_IsForProductLine

		public static string Area_IsForProductLine { get { return GetResourceString("Area_IsForProductLine"); } }
//Resources:UserAdminResources:Area_PageCategories

		public static string Area_PageCategories { get { return GetResourceString("Area_PageCategories"); } }
//Resources:UserAdminResources:Area_Title

		public static string Area_Title { get { return GetResourceString("Area_Title"); } }
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
//Resources:UserAdminResources:AuthDSL_AuthView

		public static string AuthDSL_AuthView { get { return GetResourceString("AuthDSL_AuthView"); } }
//Resources:UserAdminResources:AuthDSL_Description

		public static string AuthDSL_Description { get { return GetResourceString("AuthDSL_Description"); } }
//Resources:UserAdminResources:AuthDSL_Help

		public static string AuthDSL_Help { get { return GetResourceString("AuthDSL_Help"); } }
//Resources:UserAdminResources:AuthDSL_Title

		public static string AuthDSL_Title { get { return GetResourceString("AuthDSL_Title"); } }
//Resources:UserAdminResources:AuthenticationLogs_Description

		public static string AuthenticationLogs_Description { get { return GetResourceString("AuthenticationLogs_Description"); } }
//Resources:UserAdminResources:AuthenticationLogs_Title

		public static string AuthenticationLogs_Title { get { return GetResourceString("AuthenticationLogs_Title"); } }
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
//Resources:UserAdminResources:AuthFailureRecord_Description

		public static string AuthFailureRecord_Description { get { return GetResourceString("AuthFailureRecord_Description"); } }
//Resources:UserAdminResources:AuthFailureRecord_Help

		public static string AuthFailureRecord_Help { get { return GetResourceString("AuthFailureRecord_Help"); } }
//Resources:UserAdminResources:AuthFailureRecord_Name

		public static string AuthFailureRecord_Name { get { return GetResourceString("AuthFailureRecord_Name"); } }
//Resources:UserAdminResources:AuthFieldAction_Description

		public static string AuthFieldAction_Description { get { return GetResourceString("AuthFieldAction_Description"); } }
//Resources:UserAdminResources:AuthFieldAction_Finder

		public static string AuthFieldAction_Finder { get { return GetResourceString("AuthFieldAction_Finder"); } }
//Resources:UserAdminResources:AuthFieldAction_Finder_Help

		public static string AuthFieldAction_Finder_Help { get { return GetResourceString("AuthFieldAction_Finder_Help"); } }
//Resources:UserAdminResources:AuthFieldAction_Help

		public static string AuthFieldAction_Help { get { return GetResourceString("AuthFieldAction_Help"); } }
//Resources:UserAdminResources:AuthFieldAction_Name

		public static string AuthFieldAction_Name { get { return GetResourceString("AuthFieldAction_Name"); } }
//Resources:UserAdminResources:AuthFieldAction_Name_Help

		public static string AuthFieldAction_Name_Help { get { return GetResourceString("AuthFieldAction_Name_Help"); } }
//Resources:UserAdminResources:AuthFieldAction_Title

		public static string AuthFieldAction_Title { get { return GetResourceString("AuthFieldAction_Title"); } }
//Resources:UserAdminResources:AuthLoginRequest_Description

		public static string AuthLoginRequest_Description { get { return GetResourceString("AuthLoginRequest_Description"); } }
//Resources:UserAdminResources:AuthLoginRequest_Help

		public static string AuthLoginRequest_Help { get { return GetResourceString("AuthLoginRequest_Help"); } }
//Resources:UserAdminResources:AuthLoginRequest_Name

		public static string AuthLoginRequest_Name { get { return GetResourceString("AuthLoginRequest_Name"); } }
//Resources:UserAdminResources:AuthLogReviewSummary_Description

		public static string AuthLogReviewSummary_Description { get { return GetResourceString("AuthLogReviewSummary_Description"); } }
//Resources:UserAdminResources:AuthLogReviewSummary_Help

		public static string AuthLogReviewSummary_Help { get { return GetResourceString("AuthLogReviewSummary_Help"); } }
//Resources:UserAdminResources:AuthLogReviewSummary_Name

		public static string AuthLogReviewSummary_Name { get { return GetResourceString("AuthLogReviewSummary_Name"); } }
//Resources:UserAdminResources:AuthResponse_Description

		public static string AuthResponse_Description { get { return GetResourceString("AuthResponse_Description"); } }
//Resources:UserAdminResources:AuthResponse_Help

		public static string AuthResponse_Help { get { return GetResourceString("AuthResponse_Help"); } }
//Resources:UserAdminResources:AuthResponse_Name

		public static string AuthResponse_Name { get { return GetResourceString("AuthResponse_Name"); } }
//Resources:UserAdminResources:AuthRunnerAction_Description

		public static string AuthRunnerAction_Description { get { return GetResourceString("AuthRunnerAction_Description"); } }
//Resources:UserAdminResources:AuthRunnerAction_Help

		public static string AuthRunnerAction_Help { get { return GetResourceString("AuthRunnerAction_Help"); } }
//Resources:UserAdminResources:AuthRunnerAction_Name

		public static string AuthRunnerAction_Name { get { return GetResourceString("AuthRunnerAction_Name"); } }
//Resources:UserAdminResources:AuthRunnerArtifact_Description

		public static string AuthRunnerArtifact_Description { get { return GetResourceString("AuthRunnerArtifact_Description"); } }
//Resources:UserAdminResources:AuthRunnerArtifact_Help

		public static string AuthRunnerArtifact_Help { get { return GetResourceString("AuthRunnerArtifact_Help"); } }
//Resources:UserAdminResources:AuthRunnerArtifact_Name

		public static string AuthRunnerArtifact_Name { get { return GetResourceString("AuthRunnerArtifact_Name"); } }
//Resources:UserAdminResources:AuthRunnerInput_Description

		public static string AuthRunnerInput_Description { get { return GetResourceString("AuthRunnerInput_Description"); } }
//Resources:UserAdminResources:AuthRunnerInput_Help

		public static string AuthRunnerInput_Help { get { return GetResourceString("AuthRunnerInput_Help"); } }
//Resources:UserAdminResources:AuthRunnerInput_Name

		public static string AuthRunnerInput_Name { get { return GetResourceString("AuthRunnerInput_Name"); } }
//Resources:UserAdminResources:AuthRunnerObservations_Description

		public static string AuthRunnerObservations_Description { get { return GetResourceString("AuthRunnerObservations_Description"); } }
//Resources:UserAdminResources:AuthRunnerObservations_Help

		public static string AuthRunnerObservations_Help { get { return GetResourceString("AuthRunnerObservations_Help"); } }
//Resources:UserAdminResources:AuthRunnerObservations_Name

		public static string AuthRunnerObservations_Name { get { return GetResourceString("AuthRunnerObservations_Name"); } }
//Resources:UserAdminResources:AuthRunnerOptions_Description

		public static string AuthRunnerOptions_Description { get { return GetResourceString("AuthRunnerOptions_Description"); } }
//Resources:UserAdminResources:AuthRunnerOptions_Help

		public static string AuthRunnerOptions_Help { get { return GetResourceString("AuthRunnerOptions_Help"); } }
//Resources:UserAdminResources:AuthRunnerOptions_Name

		public static string AuthRunnerOptions_Name { get { return GetResourceString("AuthRunnerOptions_Name"); } }
//Resources:UserAdminResources:AuthRunnerPlan_Description

		public static string AuthRunnerPlan_Description { get { return GetResourceString("AuthRunnerPlan_Description"); } }
//Resources:UserAdminResources:AuthRunnerPlan_Help

		public static string AuthRunnerPlan_Help { get { return GetResourceString("AuthRunnerPlan_Help"); } }
//Resources:UserAdminResources:AuthRunnerPlan_Name

		public static string AuthRunnerPlan_Name { get { return GetResourceString("AuthRunnerPlan_Name"); } }
//Resources:UserAdminResources:AuthRunnerResult_Description

		public static string AuthRunnerResult_Description { get { return GetResourceString("AuthRunnerResult_Description"); } }
//Resources:UserAdminResources:AuthRunnerResult_Help

		public static string AuthRunnerResult_Help { get { return GetResourceString("AuthRunnerResult_Help"); } }
//Resources:UserAdminResources:AuthRunnerResult_Name

		public static string AuthRunnerResult_Name { get { return GetResourceString("AuthRunnerResult_Name"); } }
//Resources:UserAdminResources:AuthSingleuseToken_TokenNotFound

		public static string AuthSingleuseToken_TokenNotFound { get { return GetResourceString("AuthSingleuseToken_TokenNotFound"); } }
//Resources:UserAdminResources:AuthSingleuseToken_UserNotFound

		public static string AuthSingleuseToken_UserNotFound { get { return GetResourceString("AuthSingleuseToken_UserNotFound"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_BelongsToOrg

		public static string AuthTenantStateSnapshot_BelongsToOrg { get { return GetResourceString("AuthTenantStateSnapshot_BelongsToOrg"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_BelongsToOrg_Help

		public static string AuthTenantStateSnapshot_BelongsToOrg_Help { get { return GetResourceString("AuthTenantStateSnapshot_BelongsToOrg_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_Description

		public static string AuthTenantStateSnapshot_Description { get { return GetResourceString("AuthTenantStateSnapshot_Description"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_EmailConfirmed

		public static string AuthTenantStateSnapshot_EmailConfirmed { get { return GetResourceString("AuthTenantStateSnapshot_EmailConfirmed"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_EmailConfirmed_Help

		public static string AuthTenantStateSnapshot_EmailConfirmed_Help { get { return GetResourceString("AuthTenantStateSnapshot_EmailConfirmed_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_EnsureUserDoesNotExist

		public static string AuthTenantStateSnapshot_EnsureUserDoesNotExist { get { return GetResourceString("AuthTenantStateSnapshot_EnsureUserDoesNotExist"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_EnsureUserDoesNotExist_Help

		public static string AuthTenantStateSnapshot_EnsureUserDoesNotExist_Help { get { return GetResourceString("AuthTenantStateSnapshot_EnsureUserDoesNotExist_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_EnsureUserExists

		public static string AuthTenantStateSnapshot_EnsureUserExists { get { return GetResourceString("AuthTenantStateSnapshot_EnsureUserExists"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_EnsureUserExists_Help

		public static string AuthTenantStateSnapshot_EnsureUserExists_Help { get { return GetResourceString("AuthTenantStateSnapshot_EnsureUserExists_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_ExternalLoginProviders

		public static string AuthTenantStateSnapshot_ExternalLoginProviders { get { return GetResourceString("AuthTenantStateSnapshot_ExternalLoginProviders"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_ExternalLoginProviders_Help

		public static string AuthTenantStateSnapshot_ExternalLoginProviders_Help { get { return GetResourceString("AuthTenantStateSnapshot_ExternalLoginProviders_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_GenerateEmailConfirm

		public static string AuthTenantStateSnapshot_GenerateEmailConfirm { get { return GetResourceString("AuthTenantStateSnapshot_GenerateEmailConfirm"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_HasPasskey

		public static string AuthTenantStateSnapshot_HasPasskey { get { return GetResourceString("AuthTenantStateSnapshot_HasPasskey"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_HasPassword

		public static string AuthTenantStateSnapshot_HasPassword { get { return GetResourceString("AuthTenantStateSnapshot_HasPassword"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_HasPendingIdentity

		public static string AuthTenantStateSnapshot_HasPendingIdentity { get { return GetResourceString("AuthTenantStateSnapshot_HasPendingIdentity"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_HasTotp

		public static string AuthTenantStateSnapshot_HasTotp { get { return GetResourceString("AuthTenantStateSnapshot_HasTotp"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_Help

		public static string AuthTenantStateSnapshot_Help { get { return GetResourceString("AuthTenantStateSnapshot_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_IsAccountDisabled

		public static string AuthTenantStateSnapshot_IsAccountDisabled { get { return GetResourceString("AuthTenantStateSnapshot_IsAccountDisabled"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_IsAccountDisabled_Help

		public static string AuthTenantStateSnapshot_IsAccountDisabled_Help { get { return GetResourceString("AuthTenantStateSnapshot_IsAccountDisabled_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_IsAnonymous

		public static string AuthTenantStateSnapshot_IsAnonymous { get { return GetResourceString("AuthTenantStateSnapshot_IsAnonymous"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_IsAnonymous_Help

		public static string AuthTenantStateSnapshot_IsAnonymous_Help { get { return GetResourceString("AuthTenantStateSnapshot_IsAnonymous_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_IsLoggedIn

		public static string AuthTenantStateSnapshot_IsLoggedIn { get { return GetResourceString("AuthTenantStateSnapshot_IsLoggedIn"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_IsLoggedIn_Help

		public static string AuthTenantStateSnapshot_IsLoggedIn_Help { get { return GetResourceString("AuthTenantStateSnapshot_IsLoggedIn_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_IsOrgAdmin

		public static string AuthTenantStateSnapshot_IsOrgAdmin { get { return GetResourceString("AuthTenantStateSnapshot_IsOrgAdmin"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_LastMfaDateTimeUtc

		public static string AuthTenantStateSnapshot_LastMfaDateTimeUtc { get { return GetResourceString("AuthTenantStateSnapshot_LastMfaDateTimeUtc"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_LastMfaDateTimeUtc_Help

		public static string AuthTenantStateSnapshot_LastMfaDateTimeUtc_Help { get { return GetResourceString("AuthTenantStateSnapshot_LastMfaDateTimeUtc_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_PhoneNumberConfirmed

		public static string AuthTenantStateSnapshot_PhoneNumberConfirmed { get { return GetResourceString("AuthTenantStateSnapshot_PhoneNumberConfirmed"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_PhoneNumberConfirmed_Help

		public static string AuthTenantStateSnapshot_PhoneNumberConfirmed_Help { get { return GetResourceString("AuthTenantStateSnapshot_PhoneNumberConfirmed_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_SendMagicLink

		public static string AuthTenantStateSnapshot_SendMagicLink { get { return GetResourceString("AuthTenantStateSnapshot_SendMagicLink"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_ShouldInviteToOrg

		public static string AuthTenantStateSnapshot_ShouldInviteToOrg { get { return GetResourceString("AuthTenantStateSnapshot_ShouldInviteToOrg"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_ShouldSendConfirmationEmail

		public static string AuthTenantStateSnapshot_ShouldSendConfirmationEmail { get { return GetResourceString("AuthTenantStateSnapshot_ShouldSendConfirmationEmail"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_ShowWelcome

		public static string AuthTenantStateSnapshot_ShowWelcome { get { return GetResourceString("AuthTenantStateSnapshot_ShowWelcome"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_ShowWelcome_Help

		public static string AuthTenantStateSnapshot_ShowWelcome_Help { get { return GetResourceString("AuthTenantStateSnapshot_ShowWelcome_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_Title

		public static string AuthTenantStateSnapshot_Title { get { return GetResourceString("AuthTenantStateSnapshot_Title"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_TwoFactorEnabled

		public static string AuthTenantStateSnapshot_TwoFactorEnabled { get { return GetResourceString("AuthTenantStateSnapshot_TwoFactorEnabled"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_TwoFactorEnabled_Help

		public static string AuthTenantStateSnapshot_TwoFactorEnabled_Help { get { return GetResourceString("AuthTenantStateSnapshot_TwoFactorEnabled_Help"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_UserHasEmail

		public static string AuthTenantStateSnapshot_UserHasEmail { get { return GetResourceString("AuthTenantStateSnapshot_UserHasEmail"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_UserHasFirstName

		public static string AuthTenantStateSnapshot_UserHasFirstName { get { return GetResourceString("AuthTenantStateSnapshot_UserHasFirstName"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_UserHasLastName

		public static string AuthTenantStateSnapshot_UserHasLastName { get { return GetResourceString("AuthTenantStateSnapshot_UserHasLastName"); } }
//Resources:UserAdminResources:AuthTenantStateSnapshot_UserHasOAuthGitHub

		public static string AuthTenantStateSnapshot_UserHasOAuthGitHub { get { return GetResourceString("AuthTenantStateSnapshot_UserHasOAuthGitHub"); } }
//Resources:UserAdminResources:AuthView_Actions

		public static string AuthView_Actions { get { return GetResourceString("AuthView_Actions"); } }
//Resources:UserAdminResources:AuthView_Actions_Help

		public static string AuthView_Actions_Help { get { return GetResourceString("AuthView_Actions_Help"); } }
//Resources:UserAdminResources:AuthView_Description

		public static string AuthView_Description { get { return GetResourceString("AuthView_Description"); } }
//Resources:UserAdminResources:AuthView_Fields

		public static string AuthView_Fields { get { return GetResourceString("AuthView_Fields"); } }
//Resources:UserAdminResources:AuthView_FormFiield_Description

		public static string AuthView_FormFiield_Description { get { return GetResourceString("AuthView_FormFiield_Description"); } }
//Resources:UserAdminResources:AuthView_Help

		public static string AuthView_Help { get { return GetResourceString("AuthView_Help"); } }
//Resources:UserAdminResources:AuthView_Route

		public static string AuthView_Route { get { return GetResourceString("AuthView_Route"); } }
//Resources:UserAdminResources:AuthView_Route_Help

		public static string AuthView_Route_Help { get { return GetResourceString("AuthView_Route_Help"); } }
//Resources:UserAdminResources:AuthView_RouteHelp

		public static string AuthView_RouteHelp { get { return GetResourceString("AuthView_RouteHelp"); } }
//Resources:UserAdminResources:AuthView_Title

		public static string AuthView_Title { get { return GetResourceString("AuthView_Title"); } }
//Resources:UserAdminResources:AuthView_ViewId

		public static string AuthView_ViewId { get { return GetResourceString("AuthView_ViewId"); } }
//Resources:UserAdminResources:AuthView_Views

		public static string AuthView_Views { get { return GetResourceString("AuthView_Views"); } }
//Resources:UserAdminResources:AuthView_Views_Help

		public static string AuthView_Views_Help { get { return GetResourceString("AuthView_Views_Help"); } }
//Resources:UserAdminResources:AuthViewAction_Description

		public static string AuthViewAction_Description { get { return GetResourceString("AuthViewAction_Description"); } }
//Resources:UserAdminResources:AuthViewAction_Finder

		public static string AuthViewAction_Finder { get { return GetResourceString("AuthViewAction_Finder"); } }
//Resources:UserAdminResources:AuthViewField_Description

		public static string AuthViewField_Description { get { return GetResourceString("AuthViewField_Description"); } }
//Resources:UserAdminResources:AuthViewField_FieldName

		public static string AuthViewField_FieldName { get { return GetResourceString("AuthViewField_FieldName"); } }
//Resources:UserAdminResources:AuthViewField_FieldType

		public static string AuthViewField_FieldType { get { return GetResourceString("AuthViewField_FieldType"); } }
//Resources:UserAdminResources:AuthViewField_Finder

		public static string AuthViewField_Finder { get { return GetResourceString("AuthViewField_Finder"); } }
//Resources:UserAdminResources:AuthViewField_Finder_Help

		public static string AuthViewField_Finder_Help { get { return GetResourceString("AuthViewField_Finder_Help"); } }
//Resources:UserAdminResources:AuthViewField_Help

		public static string AuthViewField_Help { get { return GetResourceString("AuthViewField_Help"); } }
//Resources:UserAdminResources:AuthViewField_Name

		public static string AuthViewField_Name { get { return GetResourceString("AuthViewField_Name"); } }
//Resources:UserAdminResources:AuthViewField_Name_Help

		public static string AuthViewField_Name_Help { get { return GetResourceString("AuthViewField_Name_Help"); } }
//Resources:UserAdminResources:AuthViewField_Title

		public static string AuthViewField_Title { get { return GetResourceString("AuthViewField_Title"); } }
//Resources:UserAdminResources:AuthViewFormField_Title

		public static string AuthViewFormField_Title { get { return GetResourceString("AuthViewFormField_Title"); } }
//Resources:UserAdminResources:AuthViews_Title

		public static string AuthViews_Title { get { return GetResourceString("AuthViews_Title"); } }
//Resources:UserAdminResources:BasicTheme_Description

		public static string BasicTheme_Description { get { return GetResourceString("BasicTheme_Description"); } }
//Resources:UserAdminResources:BasicTheme_Help

		public static string BasicTheme_Help { get { return GetResourceString("BasicTheme_Help"); } }
//Resources:UserAdminResources:BasicTheme_Name

		public static string BasicTheme_Name { get { return GetResourceString("BasicTheme_Name"); } }
//Resources:UserAdminResources:Billing_Contact

		public static string Billing_Contact { get { return GetResourceString("Billing_Contact"); } }
//Resources:UserAdminResources:Calendar_AllDay

		public static string Calendar_AllDay { get { return GetResourceString("Calendar_AllDay"); } }
//Resources:UserAdminResources:Calendar_Date

		public static string Calendar_Date { get { return GetResourceString("Calendar_Date"); } }
//Resources:UserAdminResources:Calendar_Description

		public static string Calendar_Description { get { return GetResourceString("Calendar_Description"); } }
//Resources:UserAdminResources:Calendar_End

		public static string Calendar_End { get { return GetResourceString("Calendar_End"); } }
//Resources:UserAdminResources:Calendar_EventLink

		public static string Calendar_EventLink { get { return GetResourceString("Calendar_EventLink"); } }
//Resources:UserAdminResources:Calendar_EventType

		public static string Calendar_EventType { get { return GetResourceString("Calendar_EventType"); } }
//Resources:UserAdminResources:Calendar_EventType_Select

		public static string Calendar_EventType_Select { get { return GetResourceString("Calendar_EventType_Select"); } }
//Resources:UserAdminResources:Calendar_ObjectDescription

		public static string Calendar_ObjectDescription { get { return GetResourceString("Calendar_ObjectDescription"); } }
//Resources:UserAdminResources:Calendar_ObjectTitle

		public static string Calendar_ObjectTitle { get { return GetResourceString("Calendar_ObjectTitle"); } }
//Resources:UserAdminResources:Calendar_Start

		public static string Calendar_Start { get { return GetResourceString("Calendar_Start"); } }
//Resources:UserAdminResources:CalendarEvent_Title

		public static string CalendarEvent_Title { get { return GetResourceString("CalendarEvent_Title"); } }
//Resources:UserAdminResources:CalendarEventType_Holiday

		public static string CalendarEventType_Holiday { get { return GetResourceString("CalendarEventType_Holiday"); } }
//Resources:UserAdminResources:CalendarEventType_Networking

		public static string CalendarEventType_Networking { get { return GetResourceString("CalendarEventType_Networking"); } }
//Resources:UserAdminResources:CalendarEventType_Other

		public static string CalendarEventType_Other { get { return GetResourceString("CalendarEventType_Other"); } }
//Resources:UserAdminResources:CalendarEventType_OutOfOffice

		public static string CalendarEventType_OutOfOffice { get { return GetResourceString("CalendarEventType_OutOfOffice"); } }
//Resources:UserAdminResources:CalendarEventType_UserGroup

		public static string CalendarEventType_UserGroup { get { return GetResourceString("CalendarEventType_UserGroup"); } }
//Resources:UserAdminResources:CallLog_Description

		public static string CallLog_Description { get { return GetResourceString("CallLog_Description"); } }
//Resources:UserAdminResources:CallLog_Help

		public static string CallLog_Help { get { return GetResourceString("CallLog_Help"); } }
//Resources:UserAdminResources:CallLog_Name

		public static string CallLog_Name { get { return GetResourceString("CallLog_Name"); } }
//Resources:UserAdminResources:ChangePassword_Description

		public static string ChangePassword_Description { get { return GetResourceString("ChangePassword_Description"); } }
//Resources:UserAdminResources:ChangePassword_Help

		public static string ChangePassword_Help { get { return GetResourceString("ChangePassword_Help"); } }
//Resources:UserAdminResources:ChangePassword_Name

		public static string ChangePassword_Name { get { return GetResourceString("ChangePassword_Name"); } }
//Resources:UserAdminResources:ChangePasswordVM_Description

		public static string ChangePasswordVM_Description { get { return GetResourceString("ChangePasswordVM_Description"); } }
//Resources:UserAdminResources:ChangePasswordVM_Help

		public static string ChangePasswordVM_Help { get { return GetResourceString("ChangePasswordVM_Help"); } }
//Resources:UserAdminResources:ChangePasswordVM_Title

		public static string ChangePasswordVM_Title { get { return GetResourceString("ChangePasswordVM_Title"); } }
//Resources:UserAdminResources:ClientApp_AppAuthKey1

		public static string ClientApp_AppAuthKey1 { get { return GetResourceString("ClientApp_AppAuthKey1"); } }
//Resources:UserAdminResources:ClientApp_AppAuthKey2

		public static string ClientApp_AppAuthKey2 { get { return GetResourceString("ClientApp_AppAuthKey2"); } }
//Resources:UserAdminResources:ClientApp_Description

		public static string ClientApp_Description { get { return GetResourceString("ClientApp_Description"); } }
//Resources:UserAdminResources:ClientApp_DeviceConfigs

		public static string ClientApp_DeviceConfigs { get { return GetResourceString("ClientApp_DeviceConfigs"); } }
//Resources:UserAdminResources:ClientApp_DeviceTypes

		public static string ClientApp_DeviceTypes { get { return GetResourceString("ClientApp_DeviceTypes"); } }
//Resources:UserAdminResources:ClientApp_Help

		public static string ClientApp_Help { get { return GetResourceString("ClientApp_Help"); } }
//Resources:UserAdminResources:ClientApp_Instance

		public static string ClientApp_Instance { get { return GetResourceString("ClientApp_Instance"); } }
//Resources:UserAdminResources:ClientApp_SelectInstance

		public static string ClientApp_SelectInstance { get { return GetResourceString("ClientApp_SelectInstance"); } }
//Resources:UserAdminResources:ClientApp_Title

		public static string ClientApp_Title { get { return GetResourceString("ClientApp_Title"); } }
//Resources:UserAdminResources:Common_Address1

		public static string Common_Address1 { get { return GetResourceString("Common_Address1"); } }
//Resources:UserAdminResources:Common_Address2

		public static string Common_Address2 { get { return GetResourceString("Common_Address2"); } }
//Resources:UserAdminResources:Common_Category

		public static string Common_Category { get { return GetResourceString("Common_Category"); } }
//Resources:UserAdminResources:Common_Category_Select

		public static string Common_Category_Select { get { return GetResourceString("Common_Category_Select"); } }
//Resources:UserAdminResources:Common_City

		public static string Common_City { get { return GetResourceString("Common_City"); } }
//Resources:UserAdminResources:Common_Country

		public static string Common_Country { get { return GetResourceString("Common_Country"); } }
//Resources:UserAdminResources:Common_CreatedBy

		public static string Common_CreatedBy { get { return GetResourceString("Common_CreatedBy"); } }
//Resources:UserAdminResources:Common_CreationDate

		public static string Common_CreationDate { get { return GetResourceString("Common_CreationDate"); } }
//Resources:UserAdminResources:Common_Customer

		public static string Common_Customer { get { return GetResourceString("Common_Customer"); } }
//Resources:UserAdminResources:Common_Description

		public static string Common_Description { get { return GetResourceString("Common_Description"); } }
//Resources:UserAdminResources:Common_DesktopSupport

		public static string Common_DesktopSupport { get { return GetResourceString("Common_DesktopSupport"); } }
//Resources:UserAdminResources:Common_EmailAddress

		public static string Common_EmailAddress { get { return GetResourceString("Common_EmailAddress"); } }
//Resources:UserAdminResources:Common_FillColor

		public static string Common_FillColor { get { return GetResourceString("Common_FillColor"); } }
//Resources:UserAdminResources:Common_HorizontalAlign

		public static string Common_HorizontalAlign { get { return GetResourceString("Common_HorizontalAlign"); } }
//Resources:UserAdminResources:Common_HorizontalAlign_Select

		public static string Common_HorizontalAlign_Select { get { return GetResourceString("Common_HorizontalAlign_Select"); } }
//Resources:UserAdminResources:Common_Icon

		public static string Common_Icon { get { return GetResourceString("Common_Icon"); } }
//Resources:UserAdminResources:Common_Icon_Size_ExtraLarge

		public static string Common_Icon_Size_ExtraLarge { get { return GetResourceString("Common_Icon_Size_ExtraLarge"); } }
//Resources:UserAdminResources:Common_Icon_Size_Large

		public static string Common_Icon_Size_Large { get { return GetResourceString("Common_Icon_Size_Large"); } }
//Resources:UserAdminResources:Common_Icon_Size_Medium

		public static string Common_Icon_Size_Medium { get { return GetResourceString("Common_Icon_Size_Medium"); } }
//Resources:UserAdminResources:Common_Icon_Size_Small

		public static string Common_Icon_Size_Small { get { return GetResourceString("Common_Icon_Size_Small"); } }
//Resources:UserAdminResources:Common_IconSize

		public static string Common_IconSize { get { return GetResourceString("Common_IconSize"); } }
//Resources:UserAdminResources:Common_IconSize_Select

		public static string Common_IconSize_Select { get { return GetResourceString("Common_IconSize_Select"); } }
//Resources:UserAdminResources:Common_Id

		public static string Common_Id { get { return GetResourceString("Common_Id"); } }
//Resources:UserAdminResources:Common_IsPublic

		public static string Common_IsPublic { get { return GetResourceString("Common_IsPublic"); } }
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
//Resources:UserAdminResources:Common_PhoneSupport

		public static string Common_PhoneSupport { get { return GetResourceString("Common_PhoneSupport"); } }
//Resources:UserAdminResources:Common_PostalCode

		public static string Common_PostalCode { get { return GetResourceString("Common_PostalCode"); } }
//Resources:UserAdminResources:Common_PrimaryBgColor

		public static string Common_PrimaryBgColor { get { return GetResourceString("Common_PrimaryBgColor"); } }
//Resources:UserAdminResources:Common_PrimaryTextColor

		public static string Common_PrimaryTextColor { get { return GetResourceString("Common_PrimaryTextColor"); } }
//Resources:UserAdminResources:Common_Role

		public static string Common_Role { get { return GetResourceString("Common_Role"); } }
//Resources:UserAdminResources:Common_SelectDevice

		public static string Common_SelectDevice { get { return GetResourceString("Common_SelectDevice"); } }
//Resources:UserAdminResources:Common_SelectLocation

		public static string Common_SelectLocation { get { return GetResourceString("Common_SelectLocation"); } }
//Resources:UserAdminResources:Common_State

		public static string Common_State { get { return GetResourceString("Common_State"); } }
//Resources:UserAdminResources:Common_Status

		public static string Common_Status { get { return GetResourceString("Common_Status"); } }
//Resources:UserAdminResources:Common_StrokeColor

		public static string Common_StrokeColor { get { return GetResourceString("Common_StrokeColor"); } }
//Resources:UserAdminResources:Common_TabletSupport

		public static string Common_TabletSupport { get { return GetResourceString("Common_TabletSupport"); } }
//Resources:UserAdminResources:Common_TextColor

		public static string Common_TextColor { get { return GetResourceString("Common_TextColor"); } }
//Resources:UserAdminResources:Common_TimeZome

		public static string Common_TimeZome { get { return GetResourceString("Common_TimeZome"); } }
//Resources:UserAdminResources:Common_TimeZome_Picker

		public static string Common_TimeZome_Picker { get { return GetResourceString("Common_TimeZome_Picker"); } }
//Resources:UserAdminResources:Common_VerticalAlign

		public static string Common_VerticalAlign { get { return GetResourceString("Common_VerticalAlign"); } }
//Resources:UserAdminResources:Common_VerticalAlign_Select

		public static string Common_VerticalAlign_Select { get { return GetResourceString("Common_VerticalAlign_Select"); } }
//Resources:UserAdminResources:ConfirmEmail_Description

		public static string ConfirmEmail_Description { get { return GetResourceString("ConfirmEmail_Description"); } }
//Resources:UserAdminResources:ConfirmEmail_Help

		public static string ConfirmEmail_Help { get { return GetResourceString("ConfirmEmail_Help"); } }
//Resources:UserAdminResources:ConfirmEmail_Name

		public static string ConfirmEmail_Name { get { return GetResourceString("ConfirmEmail_Name"); } }
//Resources:UserAdminResources:ContactList_Description

		public static string ContactList_Description { get { return GetResourceString("ContactList_Description"); } }
//Resources:UserAdminResources:ContactList_Title

		public static string ContactList_Title { get { return GetResourceString("ContactList_Title"); } }
//Resources:UserAdminResources:CoreUserInfo_Description

		public static string CoreUserInfo_Description { get { return GetResourceString("CoreUserInfo_Description"); } }
//Resources:UserAdminResources:CoreUserInfo_Help

		public static string CoreUserInfo_Help { get { return GetResourceString("CoreUserInfo_Help"); } }
//Resources:UserAdminResources:CoreUserInfo_Name

		public static string CoreUserInfo_Name { get { return GetResourceString("CoreUserInfo_Name"); } }
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
//Resources:UserAdminResources:CreateUserResponse_Description

		public static string CreateUserResponse_Description { get { return GetResourceString("CreateUserResponse_Description"); } }
//Resources:UserAdminResources:CreateUserResponse_Help

		public static string CreateUserResponse_Help { get { return GetResourceString("CreateUserResponse_Help"); } }
//Resources:UserAdminResources:CreateUserResponse_Name

		public static string CreateUserResponse_Name { get { return GetResourceString("CreateUserResponse_Name"); } }
//Resources:UserAdminResources:DayOfWeek_Friday

		public static string DayOfWeek_Friday { get { return GetResourceString("DayOfWeek_Friday"); } }
//Resources:UserAdminResources:DayOfWeek_Monday

		public static string DayOfWeek_Monday { get { return GetResourceString("DayOfWeek_Monday"); } }
//Resources:UserAdminResources:DayOfWeek_Saturday

		public static string DayOfWeek_Saturday { get { return GetResourceString("DayOfWeek_Saturday"); } }
//Resources:UserAdminResources:DayOfWeek_Select

		public static string DayOfWeek_Select { get { return GetResourceString("DayOfWeek_Select"); } }
//Resources:UserAdminResources:DayOfWeek_Sunday

		public static string DayOfWeek_Sunday { get { return GetResourceString("DayOfWeek_Sunday"); } }
//Resources:UserAdminResources:DayOfWeek_Thursday

		public static string DayOfWeek_Thursday { get { return GetResourceString("DayOfWeek_Thursday"); } }
//Resources:UserAdminResources:DayOfWeek_Tuesday

		public static string DayOfWeek_Tuesday { get { return GetResourceString("DayOfWeek_Tuesday"); } }
//Resources:UserAdminResources:DayOfWeek_Wednesday

		public static string DayOfWeek_Wednesday { get { return GetResourceString("DayOfWeek_Wednesday"); } }
//Resources:UserAdminResources:DeviceOwner_Description

		public static string DeviceOwner_Description { get { return GetResourceString("DeviceOwner_Description"); } }
//Resources:UserAdminResources:DeviceOwner_Devices

		public static string DeviceOwner_Devices { get { return GetResourceString("DeviceOwner_Devices"); } }
//Resources:UserAdminResources:DeviceOwner_EmailAddress

		public static string DeviceOwner_EmailAddress { get { return GetResourceString("DeviceOwner_EmailAddress"); } }
//Resources:UserAdminResources:DeviceOwner_FirstName

		public static string DeviceOwner_FirstName { get { return GetResourceString("DeviceOwner_FirstName"); } }
//Resources:UserAdminResources:DeviceOwner_LastName

		public static string DeviceOwner_LastName { get { return GetResourceString("DeviceOwner_LastName"); } }
//Resources:UserAdminResources:DeviceOwner_Password

		public static string DeviceOwner_Password { get { return GetResourceString("DeviceOwner_Password"); } }
//Resources:UserAdminResources:DeviceOwner_Password_Confirm

		public static string DeviceOwner_Password_Confirm { get { return GetResourceString("DeviceOwner_Password_Confirm"); } }
//Resources:UserAdminResources:DeviceOwner_PhoneNumber

		public static string DeviceOwner_PhoneNumber { get { return GetResourceString("DeviceOwner_PhoneNumber"); } }
//Resources:UserAdminResources:DeviceOwner_Title

		public static string DeviceOwner_Title { get { return GetResourceString("DeviceOwner_Title"); } }
//Resources:UserAdminResources:DeviceOwners_Title

		public static string DeviceOwners_Title { get { return GetResourceString("DeviceOwners_Title"); } }
//Resources:UserAdminResources:DeviceOwnersDevices_Description

		public static string DeviceOwnersDevices_Description { get { return GetResourceString("DeviceOwnersDevices_Description"); } }
//Resources:UserAdminResources:DeviceOwnersDevices_Device

		public static string DeviceOwnersDevices_Device { get { return GetResourceString("DeviceOwnersDevices_Device"); } }
//Resources:UserAdminResources:DeviceOwnersDevices_DeviceId

		public static string DeviceOwnersDevices_DeviceId { get { return GetResourceString("DeviceOwnersDevices_DeviceId"); } }
//Resources:UserAdminResources:DeviceOwnersDevices_DeviceRepository

		public static string DeviceOwnersDevices_DeviceRepository { get { return GetResourceString("DeviceOwnersDevices_DeviceRepository"); } }
//Resources:UserAdminResources:DeviceOwnersDevices_Organization

		public static string DeviceOwnersDevices_Organization { get { return GetResourceString("DeviceOwnersDevices_Organization"); } }
//Resources:UserAdminResources:DeviceOwnersDevices_Product

		public static string DeviceOwnersDevices_Product { get { return GetResourceString("DeviceOwnersDevices_Product"); } }
//Resources:UserAdminResources:DeviceOwnersDevices_Title

		public static string DeviceOwnersDevices_Title { get { return GetResourceString("DeviceOwnersDevices_Title"); } }
//Resources:UserAdminResources:Diagram_Units

		public static string Diagram_Units { get { return GetResourceString("Diagram_Units"); } }
//Resources:UserAdminResources:Diagram_Units_Select

		public static string Diagram_Units_Select { get { return GetResourceString("Diagram_Units_Select"); } }
//Resources:UserAdminResources:DiagramUnits_Feet

		public static string DiagramUnits_Feet { get { return GetResourceString("DiagramUnits_Feet"); } }
//Resources:UserAdminResources:DiagramUnits_Meters

		public static string DiagramUnits_Meters { get { return GetResourceString("DiagramUnits_Meters"); } }
//Resources:UserAdminResources:DistributionList_ExternalContacts

		public static string DistributionList_ExternalContacts { get { return GetResourceString("DistributionList_ExternalContacts"); } }
//Resources:UserAdminResources:DistroList_Description

		public static string DistroList_Description { get { return GetResourceString("DistroList_Description"); } }
//Resources:UserAdminResources:DistroList_Help

		public static string DistroList_Help { get { return GetResourceString("DistroList_Help"); } }
//Resources:UserAdminResources:DistroList_Name

		public static string DistroList_Name { get { return GetResourceString("DistroList_Name"); } }
//Resources:UserAdminResources:DistroList_ParentList

		public static string DistroList_ParentList { get { return GetResourceString("DistroList_ParentList"); } }
//Resources:UserAdminResources:DistroList_ParentList_Help

		public static string DistroList_ParentList_Help { get { return GetResourceString("DistroList_ParentList_Help"); } }
//Resources:UserAdminResources:DistroLists_Name

		public static string DistroLists_Name { get { return GetResourceString("DistroLists_Name"); } }
//Resources:UserAdminResources:DownTimeType_Admin

		public static string DownTimeType_Admin { get { return GetResourceString("DownTimeType_Admin"); } }
//Resources:UserAdminResources:DownTimeType_Holiday

		public static string DownTimeType_Holiday { get { return GetResourceString("DownTimeType_Holiday"); } }
//Resources:UserAdminResources:DownTimeType_Other

		public static string DownTimeType_Other { get { return GetResourceString("DownTimeType_Other"); } }
//Resources:UserAdminResources:DownTimeType_ScheduledMaintenance

		public static string DownTimeType_ScheduledMaintenance { get { return GetResourceString("DownTimeType_ScheduledMaintenance"); } }
//Resources:UserAdminResources:DownTimeType_Select

		public static string DownTimeType_Select { get { return GetResourceString("DownTimeType_Select"); } }
//Resources:UserAdminResources:DownTimeType_WorkWeek

		public static string DownTimeType_WorkWeek { get { return GetResourceString("DownTimeType_WorkWeek"); } }
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
//Resources:UserAdminResources:EmailSender_Description

		public static string EmailSender_Description { get { return GetResourceString("EmailSender_Description"); } }
//Resources:UserAdminResources:EmailSender_From

		public static string EmailSender_From { get { return GetResourceString("EmailSender_From"); } }
//Resources:UserAdminResources:EmailSender_FromAddress

		public static string EmailSender_FromAddress { get { return GetResourceString("EmailSender_FromAddress"); } }
//Resources:UserAdminResources:EmailSender_FromName

		public static string EmailSender_FromName { get { return GetResourceString("EmailSender_FromName"); } }
//Resources:UserAdminResources:EmailSender_ReplyTo

		public static string EmailSender_ReplyTo { get { return GetResourceString("EmailSender_ReplyTo"); } }
//Resources:UserAdminResources:EmailSender_ReplyToAddress

		public static string EmailSender_ReplyToAddress { get { return GetResourceString("EmailSender_ReplyToAddress"); } }
//Resources:UserAdminResources:EmailSender_ReplyToName

		public static string EmailSender_ReplyToName { get { return GetResourceString("EmailSender_ReplyToName"); } }
//Resources:UserAdminResources:EmailSender_Title

		public static string EmailSender_Title { get { return GetResourceString("EmailSender_Title"); } }
//Resources:UserAdminResources:EmailSenders_Title

		public static string EmailSenders_Title { get { return GetResourceString("EmailSenders_Title"); } }
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
//Resources:UserAdminResources:ExternalContact_Description

		public static string ExternalContact_Description { get { return GetResourceString("ExternalContact_Description"); } }
//Resources:UserAdminResources:ExternalContact_SendEmail

		public static string ExternalContact_SendEmail { get { return GetResourceString("ExternalContact_SendEmail"); } }
//Resources:UserAdminResources:ExternalContact_SendSMS

		public static string ExternalContact_SendSMS { get { return GetResourceString("ExternalContact_SendSMS"); } }
//Resources:UserAdminResources:ExternalContact_Title

		public static string ExternalContact_Title { get { return GetResourceString("ExternalContact_Title"); } }
//Resources:UserAdminResources:ExternalLoginConfirmVM_Description

		public static string ExternalLoginConfirmVM_Description { get { return GetResourceString("ExternalLoginConfirmVM_Description"); } }
//Resources:UserAdminResources:ExternalLoginConfirmVM_Help

		public static string ExternalLoginConfirmVM_Help { get { return GetResourceString("ExternalLoginConfirmVM_Help"); } }
//Resources:UserAdminResources:ExternalLoginConfirmVM_Title

		public static string ExternalLoginConfirmVM_Title { get { return GetResourceString("ExternalLoginConfirmVM_Title"); } }
//Resources:UserAdminResources:FavoritesByModule_Description

		public static string FavoritesByModule_Description { get { return GetResourceString("FavoritesByModule_Description"); } }
//Resources:UserAdminResources:FavoritesByModule_Help

		public static string FavoritesByModule_Help { get { return GetResourceString("FavoritesByModule_Help"); } }
//Resources:UserAdminResources:FavoritesByModule_Name

		public static string FavoritesByModule_Name { get { return GetResourceString("FavoritesByModule_Name"); } }
//Resources:UserAdminResources:Feature_Help

		public static string Feature_Help { get { return GetResourceString("Feature_Help"); } }
//Resources:UserAdminResources:Feature_Icon

		public static string Feature_Icon { get { return GetResourceString("Feature_Icon"); } }
//Resources:UserAdminResources:Feature_MenuTitle

		public static string Feature_MenuTitle { get { return GetResourceString("Feature_MenuTitle"); } }
//Resources:UserAdminResources:Feature_Summary

		public static string Feature_Summary { get { return GetResourceString("Feature_Summary"); } }
//Resources:UserAdminResources:Feature_TItle

		public static string Feature_TItle { get { return GetResourceString("Feature_TItle"); } }
//Resources:UserAdminResources:ForgotPasswordVM_Description

		public static string ForgotPasswordVM_Description { get { return GetResourceString("ForgotPasswordVM_Description"); } }
//Resources:UserAdminResources:ForgotPasswordVM_Help

		public static string ForgotPasswordVM_Help { get { return GetResourceString("ForgotPasswordVM_Help"); } }
//Resources:UserAdminResources:ForgotPasswordVM_Title

		public static string ForgotPasswordVM_Title { get { return GetResourceString("ForgotPasswordVM_Title"); } }
//Resources:UserAdminResources:FunctionMap_Description

		public static string FunctionMap_Description { get { return GetResourceString("FunctionMap_Description"); } }
//Resources:UserAdminResources:FunctionMap_Functions

		public static string FunctionMap_Functions { get { return GetResourceString("FunctionMap_Functions"); } }
//Resources:UserAdminResources:FunctionMap_Title

		public static string FunctionMap_Title { get { return GetResourceString("FunctionMap_Title"); } }
//Resources:UserAdminResources:FunctionMapFunction_ChildFunctionMap

		public static string FunctionMapFunction_ChildFunctionMap { get { return GetResourceString("FunctionMapFunction_ChildFunctionMap"); } }
//Resources:UserAdminResources:FunctionMapFunction_ChildFunctionMap_Select

		public static string FunctionMapFunction_ChildFunctionMap_Select { get { return GetResourceString("FunctionMapFunction_ChildFunctionMap_Select"); } }
//Resources:UserAdminResources:FunctionMapFunction_Description

		public static string FunctionMapFunction_Description { get { return GetResourceString("FunctionMapFunction_Description"); } }
//Resources:UserAdminResources:FunctionMapFunction_FuctionView

		public static string FunctionMapFunction_FuctionView { get { return GetResourceString("FunctionMapFunction_FuctionView"); } }
//Resources:UserAdminResources:FunctionMapFunction_FuctionView_SelectView

		public static string FunctionMapFunction_FuctionView_SelectView { get { return GetResourceString("FunctionMapFunction_FuctionView_SelectView"); } }
//Resources:UserAdminResources:FunctionMapFunction_IconShape

		public static string FunctionMapFunction_IconShape { get { return GetResourceString("FunctionMapFunction_IconShape"); } }
//Resources:UserAdminResources:FunctionMapFunction_IconShape_Landscape

		public static string FunctionMapFunction_IconShape_Landscape { get { return GetResourceString("FunctionMapFunction_IconShape_Landscape"); } }
//Resources:UserAdminResources:FunctionMapFunction_IconShape_Portrait

		public static string FunctionMapFunction_IconShape_Portrait { get { return GetResourceString("FunctionMapFunction_IconShape_Portrait"); } }
//Resources:UserAdminResources:FunctionMapFunction_IconShape_Select

		public static string FunctionMapFunction_IconShape_Select { get { return GetResourceString("FunctionMapFunction_IconShape_Select"); } }
//Resources:UserAdminResources:FunctionMapFunction_IconShape_Square

		public static string FunctionMapFunction_IconShape_Square { get { return GetResourceString("FunctionMapFunction_IconShape_Square"); } }
//Resources:UserAdminResources:FunctionMapFunction_ImageIcon

		public static string FunctionMapFunction_ImageIcon { get { return GetResourceString("FunctionMapFunction_ImageIcon"); } }
//Resources:UserAdminResources:FunctionMapFunction_Title

		public static string FunctionMapFunction_Title { get { return GetResourceString("FunctionMapFunction_Title"); } }
//Resources:UserAdminResources:FunctionMapFunction_Type

		public static string FunctionMapFunction_Type { get { return GetResourceString("FunctionMapFunction_Type"); } }
//Resources:UserAdminResources:FunctionMapFunction_Type_ChildFunctionMap

		public static string FunctionMapFunction_Type_ChildFunctionMap { get { return GetResourceString("FunctionMapFunction_Type_ChildFunctionMap"); } }
//Resources:UserAdminResources:FunctionMapFunction_Type_FunctionView

		public static string FunctionMapFunction_Type_FunctionView { get { return GetResourceString("FunctionMapFunction_Type_FunctionView"); } }
//Resources:UserAdminResources:FunctionMapFunction_Type_Select

		public static string FunctionMapFunction_Type_Select { get { return GetResourceString("FunctionMapFunction_Type_Select"); } }
//Resources:UserAdminResources:GeoLocation_Description

		public static string GeoLocation_Description { get { return GetResourceString("GeoLocation_Description"); } }
//Resources:UserAdminResources:GeoLocation_Help

		public static string GeoLocation_Help { get { return GetResourceString("GeoLocation_Help"); } }
//Resources:UserAdminResources:GeoLocation_Title

		public static string GeoLocation_Title { get { return GetResourceString("GeoLocation_Title"); } }
//Resources:UserAdminResources:HelpResource_Description

		public static string HelpResource_Description { get { return GetResourceString("HelpResource_Description"); } }
//Resources:UserAdminResources:HelpResource_DisplayInline

		public static string HelpResource_DisplayInline { get { return GetResourceString("HelpResource_DisplayInline"); } }
//Resources:UserAdminResources:HelpResource_Guide

		public static string HelpResource_Guide { get { return GetResourceString("HelpResource_Guide"); } }
//Resources:UserAdminResources:HelpResource_Link

		public static string HelpResource_Link { get { return GetResourceString("HelpResource_Link"); } }
//Resources:UserAdminResources:HelpResource_SelectGuide

		public static string HelpResource_SelectGuide { get { return GetResourceString("HelpResource_SelectGuide"); } }
//Resources:UserAdminResources:HelpResource_Summary

		public static string HelpResource_Summary { get { return GetResourceString("HelpResource_Summary"); } }
//Resources:UserAdminResources:HelpResource_Title

		public static string HelpResource_Title { get { return GetResourceString("HelpResource_Title"); } }
//Resources:UserAdminResources:HolidaySet_Culture_Or_Country

		public static string HolidaySet_Culture_Or_Country { get { return GetResourceString("HolidaySet_Culture_Or_Country"); } }
//Resources:UserAdminResources:HolidaySet_Description

		public static string HolidaySet_Description { get { return GetResourceString("HolidaySet_Description"); } }
//Resources:UserAdminResources:HolidaySet_Help

		public static string HolidaySet_Help { get { return GetResourceString("HolidaySet_Help"); } }
//Resources:UserAdminResources:HolidaySet_Holidays

		public static string HolidaySet_Holidays { get { return GetResourceString("HolidaySet_Holidays"); } }
//Resources:UserAdminResources:HolidaySet_Title

		public static string HolidaySet_Title { get { return GetResourceString("HolidaySet_Title"); } }
//Resources:UserAdminResources:HolidaySets_Title

		public static string HolidaySets_Title { get { return GetResourceString("HolidaySets_Title"); } }
//Resources:UserAdminResources:HorizontalAlign_Center

		public static string HorizontalAlign_Center { get { return GetResourceString("HorizontalAlign_Center"); } }
//Resources:UserAdminResources:HorizontalAlign_Left

		public static string HorizontalAlign_Left { get { return GetResourceString("HorizontalAlign_Left"); } }
//Resources:UserAdminResources:HorizontalAlign_Right

		public static string HorizontalAlign_Right { get { return GetResourceString("HorizontalAlign_Right"); } }
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
//Resources:UserAdminResources:Invite_Customer

		public static string Invite_Customer { get { return GetResourceString("Invite_Customer"); } }
//Resources:UserAdminResources:Invite_CustomerContact

		public static string Invite_CustomerContact { get { return GetResourceString("Invite_CustomerContact"); } }
//Resources:UserAdminResources:Invite_CustomerContactId

		public static string Invite_CustomerContactId { get { return GetResourceString("Invite_CustomerContactId"); } }
//Resources:UserAdminResources:Invite_CustomerId

		public static string Invite_CustomerId { get { return GetResourceString("Invite_CustomerId"); } }
//Resources:UserAdminResources:Invite_EndUserAppOrg

		public static string Invite_EndUserAppOrg { get { return GetResourceString("Invite_EndUserAppOrg"); } }
//Resources:UserAdminResources:Invite_EndUserAppOrgId

		public static string Invite_EndUserAppOrgId { get { return GetResourceString("Invite_EndUserAppOrgId"); } }
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
//Resources:UserAdminResources:InviteUser_Description

		public static string InviteUser_Description { get { return GetResourceString("InviteUser_Description"); } }
//Resources:UserAdminResources:InviteUser_Greeting_Label

		public static string InviteUser_Greeting_Label { get { return GetResourceString("InviteUser_Greeting_Label"); } }
//Resources:UserAdminResources:InviteUser_Greeting_Message


		///<summary>
		///WIll replace [USERS_FULL_NAME] with the first/last name of current user, [ORG_NAME] as the name of the organization.
		///</summary>
		public static string InviteUser_Greeting_Message { get { return GetResourceString("InviteUser_Greeting_Message"); } }
//Resources:UserAdminResources:InviteUser_Help

		public static string InviteUser_Help { get { return GetResourceString("InviteUser_Help"); } }
//Resources:UserAdminResources:InviteUser_InvitedByEmail

		public static string InviteUser_InvitedByEmail { get { return GetResourceString("InviteUser_InvitedByEmail"); } }
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
//Resources:UserAdminResources:KioskLoginViewModel_Description

		public static string KioskLoginViewModel_Description { get { return GetResourceString("KioskLoginViewModel_Description"); } }
//Resources:UserAdminResources:KioskLoginViewModel_Help

		public static string KioskLoginViewModel_Help { get { return GetResourceString("KioskLoginViewModel_Help"); } }
//Resources:UserAdminResources:KioskLoginViewModel_Name

		public static string KioskLoginViewModel_Name { get { return GetResourceString("KioskLoginViewModel_Name"); } }
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
//Resources:UserAdminResources:Location_RoomNumber

		public static string Location_RoomNumber { get { return GetResourceString("Location_RoomNumber"); } }
//Resources:UserAdminResources:Location_State

		public static string Location_State { get { return GetResourceString("Location_State"); } }
//Resources:UserAdminResources:LocationDevice_Description

		public static string LocationDevice_Description { get { return GetResourceString("LocationDevice_Description"); } }
//Resources:UserAdminResources:LocationDevice_Help

		public static string LocationDevice_Help { get { return GetResourceString("LocationDevice_Help"); } }
//Resources:UserAdminResources:LocationDevice_Name

		public static string LocationDevice_Name { get { return GetResourceString("LocationDevice_Name"); } }
//Resources:UserAdminResources:LocationDiagram_BaseLocation

		public static string LocationDiagram_BaseLocation { get { return GetResourceString("LocationDiagram_BaseLocation"); } }
//Resources:UserAdminResources:LocationDiagram_BaseLocation_Help

		public static string LocationDiagram_BaseLocation_Help { get { return GetResourceString("LocationDiagram_BaseLocation_Help"); } }
//Resources:UserAdminResources:LocationDiagram_Groups

		public static string LocationDiagram_Groups { get { return GetResourceString("LocationDiagram_Groups"); } }
//Resources:UserAdminResources:LocationDiagram_Height

		public static string LocationDiagram_Height { get { return GetResourceString("LocationDiagram_Height"); } }
//Resources:UserAdminResources:LocationDiagram_Layers

		public static string LocationDiagram_Layers { get { return GetResourceString("LocationDiagram_Layers"); } }
//Resources:UserAdminResources:LocationDiagram_Left

		public static string LocationDiagram_Left { get { return GetResourceString("LocationDiagram_Left"); } }
//Resources:UserAdminResources:LocationDiagram_Shape

		public static string LocationDiagram_Shape { get { return GetResourceString("LocationDiagram_Shape"); } }
//Resources:UserAdminResources:LocationDiagram_Shape_CustomerLocation

		public static string LocationDiagram_Shape_CustomerLocation { get { return GetResourceString("LocationDiagram_Shape_CustomerLocation"); } }
//Resources:UserAdminResources:LocationDiagram_Shape_CustomerLocation_Help

		public static string LocationDiagram_Shape_CustomerLocation_Help { get { return GetResourceString("LocationDiagram_Shape_CustomerLocation_Help"); } }
//Resources:UserAdminResources:LocationDiagram_Shape_Help

		public static string LocationDiagram_Shape_Help { get { return GetResourceString("LocationDiagram_Shape_Help"); } }
//Resources:UserAdminResources:LocationDiagram_Shape_OrgLocation

		public static string LocationDiagram_Shape_OrgLocation { get { return GetResourceString("LocationDiagram_Shape_OrgLocation"); } }
//Resources:UserAdminResources:LocationDiagram_Shape_OrgLocation_Help

		public static string LocationDiagram_Shape_OrgLocation_Help { get { return GetResourceString("LocationDiagram_Shape_OrgLocation_Help"); } }
//Resources:UserAdminResources:LocationDiagram_Shapes

		public static string LocationDiagram_Shapes { get { return GetResourceString("LocationDiagram_Shapes"); } }
//Resources:UserAdminResources:LocationDiagram_Title

		public static string LocationDiagram_Title { get { return GetResourceString("LocationDiagram_Title"); } }
//Resources:UserAdminResources:LocationDiagram_Top

		public static string LocationDiagram_Top { get { return GetResourceString("LocationDiagram_Top"); } }
//Resources:UserAdminResources:LocationDiagram_Width

		public static string LocationDiagram_Width { get { return GetResourceString("LocationDiagram_Width"); } }
//Resources:UserAdminResources:LocationDiagramLayer_Description

		public static string LocationDiagramLayer_Description { get { return GetResourceString("LocationDiagramLayer_Description"); } }
//Resources:UserAdminResources:LocationDiagramLayer_Groups

		public static string LocationDiagramLayer_Groups { get { return GetResourceString("LocationDiagramLayer_Groups"); } }
//Resources:UserAdminResources:LocationDiagramLayer_Locked

		public static string LocationDiagramLayer_Locked { get { return GetResourceString("LocationDiagramLayer_Locked"); } }
//Resources:UserAdminResources:LocationDiagramLayer_Shapes

		public static string LocationDiagramLayer_Shapes { get { return GetResourceString("LocationDiagramLayer_Shapes"); } }
//Resources:UserAdminResources:LocationDiagramLayer_Title

		public static string LocationDiagramLayer_Title { get { return GetResourceString("LocationDiagramLayer_Title"); } }
//Resources:UserAdminResources:LocationDiagramLayer_Visible

		public static string LocationDiagramLayer_Visible { get { return GetResourceString("LocationDiagramLayer_Visible"); } }
//Resources:UserAdminResources:LocationDiagrams_Description

		public static string LocationDiagrams_Description { get { return GetResourceString("LocationDiagrams_Description"); } }
//Resources:UserAdminResources:LocationDiagrams_Title

		public static string LocationDiagrams_Title { get { return GetResourceString("LocationDiagrams_Title"); } }
//Resources:UserAdminResources:LocationDiagramShapeGroup_Description

		public static string LocationDiagramShapeGroup_Description { get { return GetResourceString("LocationDiagramShapeGroup_Description"); } }
//Resources:UserAdminResources:LocationDiagramShapeGroup_Help

		public static string LocationDiagramShapeGroup_Help { get { return GetResourceString("LocationDiagramShapeGroup_Help"); } }
//Resources:UserAdminResources:LocationDiagramShapeGroup_Shapes

		public static string LocationDiagramShapeGroup_Shapes { get { return GetResourceString("LocationDiagramShapeGroup_Shapes"); } }
//Resources:UserAdminResources:LocationDiagramShapeGroup_Title

		public static string LocationDiagramShapeGroup_Title { get { return GetResourceString("LocationDiagramShapeGroup_Title"); } }
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
//Resources:UserAdminResources:MagicLinkAttempt_Description

		public static string MagicLinkAttempt_Description { get { return GetResourceString("MagicLinkAttempt_Description"); } }
//Resources:UserAdminResources:MagicLinkAttempt_Help

		public static string MagicLinkAttempt_Help { get { return GetResourceString("MagicLinkAttempt_Help"); } }
//Resources:UserAdminResources:MagicLinkAttempt_Title

		public static string MagicLinkAttempt_Title { get { return GetResourceString("MagicLinkAttempt_Title"); } }
//Resources:UserAdminResources:MagicLinkConsumeContext_Description

		public static string MagicLinkConsumeContext_Description { get { return GetResourceString("MagicLinkConsumeContext_Description"); } }
//Resources:UserAdminResources:MagicLinkConsumeContext_Help

		public static string MagicLinkConsumeContext_Help { get { return GetResourceString("MagicLinkConsumeContext_Help"); } }
//Resources:UserAdminResources:MagicLinkConsumeContext_Title

		public static string MagicLinkConsumeContext_Title { get { return GetResourceString("MagicLinkConsumeContext_Title"); } }
//Resources:UserAdminResources:MagicLinkConsumeResponse_Description

		public static string MagicLinkConsumeResponse_Description { get { return GetResourceString("MagicLinkConsumeResponse_Description"); } }
//Resources:UserAdminResources:MagicLinkConsumeResponse_Help

		public static string MagicLinkConsumeResponse_Help { get { return GetResourceString("MagicLinkConsumeResponse_Help"); } }
//Resources:UserAdminResources:MagicLinkConsumeResponse_Title

		public static string MagicLinkConsumeResponse_Title { get { return GetResourceString("MagicLinkConsumeResponse_Title"); } }
//Resources:UserAdminResources:MagicLinkExchangeContext_Description

		public static string MagicLinkExchangeContext_Description { get { return GetResourceString("MagicLinkExchangeContext_Description"); } }
//Resources:UserAdminResources:MagicLinkExchangeContext_Help

		public static string MagicLinkExchangeContext_Help { get { return GetResourceString("MagicLinkExchangeContext_Help"); } }
//Resources:UserAdminResources:MagicLinkExchangeContext_Title

		public static string MagicLinkExchangeContext_Title { get { return GetResourceString("MagicLinkExchangeContext_Title"); } }
//Resources:UserAdminResources:MagicLinkExchangeResponse_Description

		public static string MagicLinkExchangeResponse_Description { get { return GetResourceString("MagicLinkExchangeResponse_Description"); } }
//Resources:UserAdminResources:MagicLinkExchangeResponse_Help

		public static string MagicLinkExchangeResponse_Help { get { return GetResourceString("MagicLinkExchangeResponse_Help"); } }
//Resources:UserAdminResources:MagicLinkExchangeResponse_Title

		public static string MagicLinkExchangeResponse_Title { get { return GetResourceString("MagicLinkExchangeResponse_Title"); } }
//Resources:UserAdminResources:MagicLinkRequest_Description

		public static string MagicLinkRequest_Description { get { return GetResourceString("MagicLinkRequest_Description"); } }
//Resources:UserAdminResources:MagicLinkRequest_Help

		public static string MagicLinkRequest_Help { get { return GetResourceString("MagicLinkRequest_Help"); } }
//Resources:UserAdminResources:MagicLinkRequest_Title

		public static string MagicLinkRequest_Title { get { return GetResourceString("MagicLinkRequest_Title"); } }
//Resources:UserAdminResources:MagicLinkRequestContext_Description

		public static string MagicLinkRequestContext_Description { get { return GetResourceString("MagicLinkRequestContext_Description"); } }
//Resources:UserAdminResources:MagicLinkRequestContext_Help

		public static string MagicLinkRequestContext_Help { get { return GetResourceString("MagicLinkRequestContext_Help"); } }
//Resources:UserAdminResources:MagicLinkRequestContext_Title

		public static string MagicLinkRequestContext_Title { get { return GetResourceString("MagicLinkRequestContext_Title"); } }
//Resources:UserAdminResources:ManagedAsset_Description

		public static string ManagedAsset_Description { get { return GetResourceString("ManagedAsset_Description"); } }
//Resources:UserAdminResources:ManagedAsset_Help

		public static string ManagedAsset_Help( string assetSetId, string assetId) { return GetResourceString("ManagedAsset_Help", "{assetSetId}", assetSetId, "{assetId}", assetId); }
//Resources:UserAdminResources:ManagedAsset_Name

		public static string ManagedAsset_Name { get { return GetResourceString("ManagedAsset_Name"); } }
//Resources:UserAdminResources:ManagedAssetSummary_Description

		public static string ManagedAssetSummary_Description { get { return GetResourceString("ManagedAssetSummary_Description"); } }
//Resources:UserAdminResources:ManagedAssetSummary_Help

		public static string ManagedAssetSummary_Help { get { return GetResourceString("ManagedAssetSummary_Help"); } }
//Resources:UserAdminResources:ManagedAssetSummary_Name

		public static string ManagedAssetSummary_Name { get { return GetResourceString("ManagedAssetSummary_Name"); } }
//Resources:UserAdminResources:Menu_DoNotDisplay

		public static string Menu_DoNotDisplay { get { return GetResourceString("Menu_DoNotDisplay"); } }
//Resources:UserAdminResources:Menu_DoNotDisplay_Help

		public static string Menu_DoNotDisplay_Help { get { return GetResourceString("Menu_DoNotDisplay_Help"); } }
//Resources:UserAdminResources:MobileOAuthPendingAuth_Description

		public static string MobileOAuthPendingAuth_Description { get { return GetResourceString("MobileOAuthPendingAuth_Description"); } }
//Resources:UserAdminResources:MobileOAuthPendingAuth_Help

		public static string MobileOAuthPendingAuth_Help { get { return GetResourceString("MobileOAuthPendingAuth_Help"); } }
//Resources:UserAdminResources:MobileOAuthPendingAuth_Name

		public static string MobileOAuthPendingAuth_Name { get { return GetResourceString("MobileOAuthPendingAuth_Name"); } }
//Resources:UserAdminResources:Module_AreaCategories

		public static string Module_AreaCategories { get { return GetResourceString("Module_AreaCategories"); } }
//Resources:UserAdminResources:Module_CardIcon

		public static string Module_CardIcon { get { return GetResourceString("Module_CardIcon"); } }
//Resources:UserAdminResources:Module_CardSummary

		public static string Module_CardSummary { get { return GetResourceString("Module_CardSummary"); } }
//Resources:UserAdminResources:Module_CardTitle

		public static string Module_CardTitle { get { return GetResourceString("Module_CardTitle"); } }
//Resources:UserAdminResources:Module_Help

		public static string Module_Help { get { return GetResourceString("Module_Help"); } }
//Resources:UserAdminResources:Module_HelpResources

		public static string Module_HelpResources { get { return GetResourceString("Module_HelpResources"); } }
//Resources:UserAdminResources:Module_IsExternalLink

		public static string Module_IsExternalLink { get { return GetResourceString("Module_IsExternalLink"); } }
//Resources:UserAdminResources:Module_IsForProductLine

		public static string Module_IsForProductLine { get { return GetResourceString("Module_IsForProductLine"); } }
//Resources:UserAdminResources:Module_IsLegacyNGX

		public static string Module_IsLegacyNGX { get { return GetResourceString("Module_IsLegacyNGX"); } }
//Resources:UserAdminResources:Module_Link

		public static string Module_Link { get { return GetResourceString("Module_Link"); } }
//Resources:UserAdminResources:Module_OpenInNewTab

		public static string Module_OpenInNewTab { get { return GetResourceString("Module_OpenInNewTab"); } }
//Resources:UserAdminResources:Module_RestrictByDefault

		public static string Module_RestrictByDefault { get { return GetResourceString("Module_RestrictByDefault"); } }
//Resources:UserAdminResources:Module_RestrictByDefault_Help

		public static string Module_RestrictByDefault_Help { get { return GetResourceString("Module_RestrictByDefault_Help"); } }
//Resources:UserAdminResources:Module_RootPath

		public static string Module_RootPath { get { return GetResourceString("Module_RootPath"); } }
//Resources:UserAdminResources:Module_RootPath_Help

		public static string Module_RootPath_Help { get { return GetResourceString("Module_RootPath_Help"); } }
//Resources:UserAdminResources:Module_SortOrder

		public static string Module_SortOrder { get { return GetResourceString("Module_SortOrder"); } }
//Resources:UserAdminResources:Module_Title

		public static string Module_Title { get { return GetResourceString("Module_Title"); } }
//Resources:UserAdminResources:ModuleCategory_Admin

		public static string ModuleCategory_Admin { get { return GetResourceString("ModuleCategory_Admin"); } }
//Resources:UserAdminResources:ModuleCategory_Admin_Summary

		public static string ModuleCategory_Admin_Summary { get { return GetResourceString("ModuleCategory_Admin_Summary"); } }
//Resources:UserAdminResources:ModuleCategory_IoT

		public static string ModuleCategory_IoT { get { return GetResourceString("ModuleCategory_IoT"); } }
//Resources:UserAdminResources:ModuleCategory_IoT_Summary

		public static string ModuleCategory_IoT_Summary { get { return GetResourceString("ModuleCategory_IoT_Summary"); } }
//Resources:UserAdminResources:ModuleCategory_Support

		public static string ModuleCategory_Support { get { return GetResourceString("ModuleCategory_Support"); } }
//Resources:UserAdminResources:ModuleCategory_Support_Summary

		public static string ModuleCategory_Support_Summary { get { return GetResourceString("ModuleCategory_Support_Summary"); } }
//Resources:UserAdminResources:ModuleCategory_Tools

		public static string ModuleCategory_Tools { get { return GetResourceString("ModuleCategory_Tools"); } }
//Resources:UserAdminResources:ModuleCategory_Tools_Summary

		public static string ModuleCategory_Tools_Summary { get { return GetResourceString("ModuleCategory_Tools_Summary"); } }
//Resources:UserAdminResources:ModuleCateogry_EndUser

		public static string ModuleCateogry_EndUser { get { return GetResourceString("ModuleCateogry_EndUser"); } }
//Resources:UserAdminResources:ModuleCateogry_EndUser_Summary

		public static string ModuleCateogry_EndUser_Summary { get { return GetResourceString("ModuleCateogry_EndUser_Summary"); } }
//Resources:UserAdminResources:ModuleCateogry_Other

		public static string ModuleCateogry_Other { get { return GetResourceString("ModuleCateogry_Other"); } }
//Resources:UserAdminResources:Modules_Title

		public static string Modules_Title { get { return GetResourceString("Modules_Title"); } }
//Resources:UserAdminResources:ModuleStatus_Alpha

		public static string ModuleStatus_Alpha { get { return GetResourceString("ModuleStatus_Alpha"); } }
//Resources:UserAdminResources:ModuleStatus_Beta

		public static string ModuleStatus_Beta { get { return GetResourceString("ModuleStatus_Beta"); } }
//Resources:UserAdminResources:ModuleStatus_Development

		public static string ModuleStatus_Development { get { return GetResourceString("ModuleStatus_Development"); } }
//Resources:UserAdminResources:ModuleStatus_Live

		public static string ModuleStatus_Live { get { return GetResourceString("ModuleStatus_Live"); } }
//Resources:UserAdminResources:ModuleStatus_Preview

		public static string ModuleStatus_Preview { get { return GetResourceString("ModuleStatus_Preview"); } }
//Resources:UserAdminResources:ModuleStatus_Retired

		public static string ModuleStatus_Retired { get { return GetResourceString("ModuleStatus_Retired"); } }
//Resources:UserAdminResources:ModuleStatus_Select

		public static string ModuleStatus_Select { get { return GetResourceString("ModuleStatus_Select"); } }
//Resources:UserAdminResources:Month_April

		public static string Month_April { get { return GetResourceString("Month_April"); } }
//Resources:UserAdminResources:Month_August

		public static string Month_August { get { return GetResourceString("Month_August"); } }
//Resources:UserAdminResources:Month_December

		public static string Month_December { get { return GetResourceString("Month_December"); } }
//Resources:UserAdminResources:Month_February

		public static string Month_February { get { return GetResourceString("Month_February"); } }
//Resources:UserAdminResources:Month_January

		public static string Month_January { get { return GetResourceString("Month_January"); } }
//Resources:UserAdminResources:Month_July

		public static string Month_July { get { return GetResourceString("Month_July"); } }
//Resources:UserAdminResources:Month_June

		public static string Month_June { get { return GetResourceString("Month_June"); } }
//Resources:UserAdminResources:Month_March

		public static string Month_March { get { return GetResourceString("Month_March"); } }
//Resources:UserAdminResources:Month_May

		public static string Month_May { get { return GetResourceString("Month_May"); } }
//Resources:UserAdminResources:Month_November

		public static string Month_November { get { return GetResourceString("Month_November"); } }
//Resources:UserAdminResources:Month_October

		public static string Month_October { get { return GetResourceString("Month_October"); } }
//Resources:UserAdminResources:Month_Select

		public static string Month_Select { get { return GetResourceString("Month_Select"); } }
//Resources:UserAdminResources:Month_September

		public static string Month_September { get { return GetResourceString("Month_September"); } }
//Resources:UserAdminResources:MostRecentlyUsed_Description

		public static string MostRecentlyUsed_Description { get { return GetResourceString("MostRecentlyUsed_Description"); } }
//Resources:UserAdminResources:MostRecentlyUsed_Help

		public static string MostRecentlyUsed_Help { get { return GetResourceString("MostRecentlyUsed_Help"); } }
//Resources:UserAdminResources:MostRecentlyUsed_Name

		public static string MostRecentlyUsed_Name { get { return GetResourceString("MostRecentlyUsed_Name"); } }
//Resources:UserAdminResources:MostRecentlyUsedItem_Description

		public static string MostRecentlyUsedItem_Description { get { return GetResourceString("MostRecentlyUsedItem_Description"); } }
//Resources:UserAdminResources:MostRecentlyUsedItem_Help

		public static string MostRecentlyUsedItem_Help { get { return GetResourceString("MostRecentlyUsedItem_Help"); } }
//Resources:UserAdminResources:MostRecentlyUsedItem_Name

		public static string MostRecentlyUsedItem_Name { get { return GetResourceString("MostRecentlyUsedItem_Name"); } }
//Resources:UserAdminResources:MostRecentlyUsedModule_Description

		public static string MostRecentlyUsedModule_Description { get { return GetResourceString("MostRecentlyUsedModule_Description"); } }
//Resources:UserAdminResources:MostRecentlyUsedModule_Help

		public static string MostRecentlyUsedModule_Help { get { return GetResourceString("MostRecentlyUsedModule_Help"); } }
//Resources:UserAdminResources:MostRecentlyUsedModule_Name

		public static string MostRecentlyUsedModule_Name { get { return GetResourceString("MostRecentlyUsedModule_Name"); } }
//Resources:UserAdminResources:NewAppUserInboxItemItem_Description

		public static string NewAppUserInboxItemItem_Description { get { return GetResourceString("NewAppUserInboxItemItem_Description"); } }
//Resources:UserAdminResources:NewAppUserInboxItemItem_Help

		public static string NewAppUserInboxItemItem_Help { get { return GetResourceString("NewAppUserInboxItemItem_Help"); } }
//Resources:UserAdminResources:NewAppUserInboxItemItem_Name

		public static string NewAppUserInboxItemItem_Name { get { return GetResourceString("NewAppUserInboxItemItem_Name"); } }
//Resources:UserAdminResources:NotificationContact_Description

		public static string NotificationContact_Description { get { return GetResourceString("NotificationContact_Description"); } }
//Resources:UserAdminResources:NotificationContact_Help

		public static string NotificationContact_Help { get { return GetResourceString("NotificationContact_Help"); } }
//Resources:UserAdminResources:NotificationContact_Name

		public static string NotificationContact_Name { get { return GetResourceString("NotificationContact_Name"); } }
//Resources:UserAdminResources:Organization

		public static string Organization { get { return GetResourceString("Organization"); } }
//Resources:UserAdminResources:Organization_AccentColor

		public static string Organization_AccentColor { get { return GetResourceString("Organization_AccentColor"); } }
//Resources:UserAdminResources:Organization_ApEmailAddress

		public static string Organization_ApEmailAddress { get { return GetResourceString("Organization_ApEmailAddress"); } }
//Resources:UserAdminResources:Organization_ApEmailAddress_Help

		public static string Organization_ApEmailAddress_Help { get { return GetResourceString("Organization_ApEmailAddress_Help"); } }
//Resources:UserAdminResources:Organization_AppUrl

		public static string Organization_AppUrl { get { return GetResourceString("Organization_AppUrl"); } }
//Resources:UserAdminResources:Organization_AppUrl_Help

		public static string Organization_AppUrl_Help { get { return GetResourceString("Organization_AppUrl_Help"); } }
//Resources:UserAdminResources:Organization_ArEmailAddress

		public static string Organization_ArEmailAddress { get { return GetResourceString("Organization_ArEmailAddress"); } }
//Resources:UserAdminResources:Organization_ArEmailAddress_Help

		public static string Organization_ArEmailAddress_Help { get { return GetResourceString("Organization_ArEmailAddress_Help"); } }
//Resources:UserAdminResources:Organization_BillingLocation

		public static string Organization_BillingLocation { get { return GetResourceString("Organization_BillingLocation"); } }
//Resources:UserAdminResources:Organization_BillingLocation_Select

		public static string Organization_BillingLocation_Select { get { return GetResourceString("Organization_BillingLocation_Select"); } }
//Resources:UserAdminResources:Organization_CantCreate

		public static string Organization_CantCreate { get { return GetResourceString("Organization_CantCreate"); } }
//Resources:UserAdminResources:Organization_CreateGettingStartedData

		public static string Organization_CreateGettingStartedData { get { return GetResourceString("Organization_CreateGettingStartedData"); } }
//Resources:UserAdminResources:Organization_CreateGettingStartedData_Help

		public static string Organization_CreateGettingStartedData_Help { get { return GetResourceString("Organization_CreateGettingStartedData_Help"); } }
//Resources:UserAdminResources:Organization_DefaultAgentContext

		public static string Organization_DefaultAgentContext { get { return GetResourceString("Organization_DefaultAgentContext"); } }
//Resources:UserAdminResources:Organization_DefaultAgentContext_Help

		public static string Organization_DefaultAgentContext_Help { get { return GetResourceString("Organization_DefaultAgentContext_Help"); } }
//Resources:UserAdminResources:Organization_DefaultAgentContext_Select

		public static string Organization_DefaultAgentContext_Select { get { return GetResourceString("Organization_DefaultAgentContext_Select"); } }
//Resources:UserAdminResources:Organization_DefaultBusinessDevelopmentRep

		public static string Organization_DefaultBusinessDevelopmentRep { get { return GetResourceString("Organization_DefaultBusinessDevelopmentRep"); } }
//Resources:UserAdminResources:Organization_DefaultBusinessDevelopmentRep_Help

		public static string Organization_DefaultBusinessDevelopmentRep_Help { get { return GetResourceString("Organization_DefaultBusinessDevelopmentRep_Help"); } }
//Resources:UserAdminResources:Organization_DefaultContributor

		public static string Organization_DefaultContributor { get { return GetResourceString("Organization_DefaultContributor"); } }
//Resources:UserAdminResources:Organization_DefaultContributor_Help

		public static string Organization_DefaultContributor_Help { get { return GetResourceString("Organization_DefaultContributor_Help"); } }
//Resources:UserAdminResources:Organization_DefaultDemoInstance

		public static string Organization_DefaultDemoInstance { get { return GetResourceString("Organization_DefaultDemoInstance"); } }
//Resources:UserAdminResources:Organization_DefaultDevelopmentInstance

		public static string Organization_DefaultDevelopmentInstance { get { return GetResourceString("Organization_DefaultDevelopmentInstance"); } }
//Resources:UserAdminResources:Organization_DefaultIndustry

		public static string Organization_DefaultIndustry { get { return GetResourceString("Organization_DefaultIndustry"); } }
//Resources:UserAdminResources:Organization_DefaultIndustry_Help

		public static string Organization_DefaultIndustry_Help { get { return GetResourceString("Organization_DefaultIndustry_Help"); } }
//Resources:UserAdminResources:Organization_DefaultIndustry_Select

		public static string Organization_DefaultIndustry_Select { get { return GetResourceString("Organization_DefaultIndustry_Select"); } }
//Resources:UserAdminResources:Organization_DefaultInstance

		public static string Organization_DefaultInstance { get { return GetResourceString("Organization_DefaultInstance"); } }
//Resources:UserAdminResources:Organization_DefaultInstance_Select

		public static string Organization_DefaultInstance_Select { get { return GetResourceString("Organization_DefaultInstance_Select"); } }
//Resources:UserAdminResources:Organization_DefaultLandingPage

		public static string Organization_DefaultLandingPage { get { return GetResourceString("Organization_DefaultLandingPage"); } }
//Resources:UserAdminResources:Organization_DefaultLandingPage_Select

		public static string Organization_DefaultLandingPage_Select { get { return GetResourceString("Organization_DefaultLandingPage_Select"); } }
//Resources:UserAdminResources:Organization_DefaultProjectAdminLead

		public static string Organization_DefaultProjectAdminLead { get { return GetResourceString("Organization_DefaultProjectAdminLead"); } }
//Resources:UserAdminResources:Organization_DefaultProjectAdminLead_Help

		public static string Organization_DefaultProjectAdminLead_Help { get { return GetResourceString("Organization_DefaultProjectAdminLead_Help"); } }
//Resources:UserAdminResources:Organization_DefaultProjectLead

		public static string Organization_DefaultProjectLead { get { return GetResourceString("Organization_DefaultProjectLead"); } }
//Resources:UserAdminResources:Organization_DefaultProjectLead_Help

		public static string Organization_DefaultProjectLead_Help { get { return GetResourceString("Organization_DefaultProjectLead_Help"); } }
//Resources:UserAdminResources:Organization_DefaultQAResource

		public static string Organization_DefaultQAResource { get { return GetResourceString("Organization_DefaultQAResource"); } }
//Resources:UserAdminResources:Organization_DefaultQAResource_Help

		public static string Organization_DefaultQAResource_Help { get { return GetResourceString("Organization_DefaultQAResource_Help"); } }
//Resources:UserAdminResources:Organization_DefaultRepo

		public static string Organization_DefaultRepo { get { return GetResourceString("Organization_DefaultRepo"); } }
//Resources:UserAdminResources:Organization_DefaultRepo_Select

		public static string Organization_DefaultRepo_Select { get { return GetResourceString("Organization_DefaultRepo_Select"); } }
//Resources:UserAdminResources:Organization_DefaultResource_Watermark

		public static string Organization_DefaultResource_Watermark { get { return GetResourceString("Organization_DefaultResource_Watermark"); } }
//Resources:UserAdminResources:Organization_DefaultTeamsWebHook

		public static string Organization_DefaultTeamsWebHook { get { return GetResourceString("Organization_DefaultTeamsWebHook"); } }
//Resources:UserAdminResources:Organization_DefaultTeamsWebHook_Help

		public static string Organization_DefaultTeamsWebHook_Help { get { return GetResourceString("Organization_DefaultTeamsWebHook_Help"); } }
//Resources:UserAdminResources:Organization_DefaultTestInstance

		public static string Organization_DefaultTestInstance { get { return GetResourceString("Organization_DefaultTestInstance"); } }
//Resources:UserAdminResources:Organization_DefaultTheme

		public static string Organization_DefaultTheme { get { return GetResourceString("Organization_DefaultTheme"); } }
//Resources:UserAdminResources:Organization_Description

		public static string Organization_Description { get { return GetResourceString("Organization_Description"); } }
//Resources:UserAdminResources:Organization_Details

		public static string Organization_Details { get { return GetResourceString("Organization_Details"); } }
//Resources:UserAdminResources:Organization_EmailConfirmBody

		public static string Organization_EmailConfirmBody { get { return GetResourceString("Organization_EmailConfirmBody"); } }
//Resources:UserAdminResources:Organization_EmailConfirmBody_Help

		public static string Organization_EmailConfirmBody_Help { get { return GetResourceString("Organization_EmailConfirmBody_Help"); } }
//Resources:UserAdminResources:Organization_EmailConfirmSubject

		public static string Organization_EmailConfirmSubject { get { return GetResourceString("Organization_EmailConfirmSubject"); } }
//Resources:UserAdminResources:Organization_EmailConfirmSubject_Help

		public static string Organization_EmailConfirmSubject_Help { get { return GetResourceString("Organization_EmailConfirmSubject_Help"); } }
//Resources:UserAdminResources:Organization_EndUserHomePage

		public static string Organization_EndUserHomePage { get { return GetResourceString("Organization_EndUserHomePage"); } }
//Resources:UserAdminResources:Organization_EndUserHomePage_Help

		public static string Organization_EndUserHomePage_Help { get { return GetResourceString("Organization_EndUserHomePage_Help"); } }
//Resources:UserAdminResources:Organization_FacebookUrl

		public static string Organization_FacebookUrl { get { return GetResourceString("Organization_FacebookUrl"); } }
//Resources:UserAdminResources:Organization_Help

		public static string Organization_Help { get { return GetResourceString("Organization_Help"); } }
//Resources:UserAdminResources:Organization_HeroBackground

		public static string Organization_HeroBackground { get { return GetResourceString("Organization_HeroBackground"); } }
//Resources:UserAdminResources:Organization_HeroTitle

		public static string Organization_HeroTitle { get { return GetResourceString("Organization_HeroTitle"); } }
//Resources:UserAdminResources:Organization_HomePage

		public static string Organization_HomePage { get { return GetResourceString("Organization_HomePage"); } }
//Resources:UserAdminResources:Organization_HomePage_Help

		public static string Organization_HomePage_Help { get { return GetResourceString("Organization_HomePage_Help"); } }
//Resources:UserAdminResources:Organization_HrEmailAddress

		public static string Organization_HrEmailAddress { get { return GetResourceString("Organization_HrEmailAddress"); } }
//Resources:UserAdminResources:Organization_HrEmailAddress_Help

		public static string Organization_HrEmailAddress_Help { get { return GetResourceString("Organization_HrEmailAddress_Help"); } }
//Resources:UserAdminResources:Organization_IsForProductLine

		public static string Organization_IsForProductLine { get { return GetResourceString("Organization_IsForProductLine"); } }
//Resources:UserAdminResources:Organization_LandingPageHostName

		public static string Organization_LandingPageHostName { get { return GetResourceString("Organization_LandingPageHostName"); } }
//Resources:UserAdminResources:Organization_LandingPageHostName_Help

		public static string Organization_LandingPageHostName_Help { get { return GetResourceString("Organization_LandingPageHostName_Help"); } }
//Resources:UserAdminResources:Organization_LinkedInUrl

		public static string Organization_LinkedInUrl { get { return GetResourceString("Organization_LinkedInUrl"); } }
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
//Resources:UserAdminResources:Organization_Locations_Title

		public static string Organization_Locations_Title { get { return GetResourceString("Organization_Locations_Title"); } }
//Resources:UserAdminResources:Organization_Logo_DarkColor

		public static string Organization_Logo_DarkColor { get { return GetResourceString("Organization_Logo_DarkColor"); } }
//Resources:UserAdminResources:Organization_Logo_DarkColor_Help

		public static string Organization_Logo_DarkColor_Help { get { return GetResourceString("Organization_Logo_DarkColor_Help"); } }
//Resources:UserAdminResources:Organization_Logo_Light

		public static string Organization_Logo_Light { get { return GetResourceString("Organization_Logo_Light"); } }
//Resources:UserAdminResources:Organization_Logo_LightColor_Help

		public static string Organization_Logo_LightColor_Help { get { return GetResourceString("Organization_Logo_LightColor_Help"); } }
//Resources:UserAdminResources:Organization_Name

		public static string Organization_Name { get { return GetResourceString("Organization_Name"); } }
//Resources:UserAdminResources:Organization_NamespaceInUse

		public static string Organization_NamespaceInUse { get { return GetResourceString("Organization_NamespaceInUse"); } }
//Resources:UserAdminResources:Organization_OrgStatuses_Active

		public static string Organization_OrgStatuses_Active { get { return GetResourceString("Organization_OrgStatuses_Active"); } }
//Resources:UserAdminResources:Organization_OrgStatuses_Deactivated

		public static string Organization_OrgStatuses_Deactivated { get { return GetResourceString("Organization_OrgStatuses_Deactivated"); } }
//Resources:UserAdminResources:Organization_OrgStatuses_Spam

		public static string Organization_OrgStatuses_Spam { get { return GetResourceString("Organization_OrgStatuses_Spam"); } }
//Resources:UserAdminResources:Organization_Owner

		public static string Organization_Owner { get { return GetResourceString("Organization_Owner"); } }
//Resources:UserAdminResources:Organization_Primary_Location

		public static string Organization_Primary_Location { get { return GetResourceString("Organization_Primary_Location"); } }
//Resources:UserAdminResources:Organization_SalesContact

		public static string Organization_SalesContact { get { return GetResourceString("Organization_SalesContact"); } }
//Resources:UserAdminResources:Organization_Status_Active

		public static string Organization_Status_Active { get { return GetResourceString("Organization_Status_Active"); } }
//Resources:UserAdminResources:Organization_Status_Active_BehindPayments

		public static string Organization_Status_Active_BehindPayments { get { return GetResourceString("Organization_Status_Active_BehindPayments"); } }
//Resources:UserAdminResources:Organization_Status_Archived

		public static string Organization_Status_Archived { get { return GetResourceString("Organization_Status_Archived"); } }
//Resources:UserAdminResources:Organization_SubLocation

		public static string Organization_SubLocation { get { return GetResourceString("Organization_SubLocation"); } }
//Resources:UserAdminResources:Organization_SubLocation_Details

		public static string Organization_SubLocation_Details { get { return GetResourceString("Organization_SubLocation_Details"); } }
//Resources:UserAdminResources:Organization_SubLocations

		public static string Organization_SubLocations { get { return GetResourceString("Organization_SubLocations"); } }
//Resources:UserAdminResources:Organization_SupportEmailAddress

		public static string Organization_SupportEmailAddress { get { return GetResourceString("Organization_SupportEmailAddress"); } }
//Resources:UserAdminResources:Organization_SupportEmailAddress_Help

		public static string Organization_SupportEmailAddress_Help { get { return GetResourceString("Organization_SupportEmailAddress_Help"); } }
//Resources:UserAdminResources:Organization_TagLine

		public static string Organization_TagLine { get { return GetResourceString("Organization_TagLine"); } }
//Resources:UserAdminResources:Organization_TestingIndustry

		public static string Organization_TestingIndustry { get { return GetResourceString("Organization_TestingIndustry"); } }
//Resources:UserAdminResources:Organization_TestingIndustry_Help

		public static string Organization_TestingIndustry_Help { get { return GetResourceString("Organization_TestingIndustry_Help"); } }
//Resources:UserAdminResources:Organization_TestingIndustry_Select

		public static string Organization_TestingIndustry_Select { get { return GetResourceString("Organization_TestingIndustry_Select"); } }
//Resources:UserAdminResources:Organization_TestingIndustryNiche

		public static string Organization_TestingIndustryNiche { get { return GetResourceString("Organization_TestingIndustryNiche"); } }
//Resources:UserAdminResources:Organization_TestingIndustryNiche_Select

		public static string Organization_TestingIndustryNiche_Select { get { return GetResourceString("Organization_TestingIndustryNiche_Select"); } }
//Resources:UserAdminResources:Organization_Title

		public static string Organization_Title { get { return GetResourceString("Organization_Title"); } }
//Resources:UserAdminResources:Organization_User_Description

		public static string Organization_User_Description { get { return GetResourceString("Organization_User_Description"); } }
//Resources:UserAdminResources:Organization_User_Help

		public static string Organization_User_Help { get { return GetResourceString("Organization_User_Help"); } }
//Resources:UserAdminResources:Organization_User_Title

		public static string Organization_User_Title { get { return GetResourceString("Organization_User_Title"); } }
//Resources:UserAdminResources:Organization_VimeoUrl

		public static string Organization_VimeoUrl { get { return GetResourceString("Organization_VimeoUrl"); } }
//Resources:UserAdminResources:Organization_WebSite

		public static string Organization_WebSite { get { return GetResourceString("Organization_WebSite"); } }
//Resources:UserAdminResources:Organization_XUrl

		public static string Organization_XUrl { get { return GetResourceString("Organization_XUrl"); } }
//Resources:UserAdminResources:Organization_YouTubeUrl

		public static string Organization_YouTubeUrl { get { return GetResourceString("Organization_YouTubeUrl"); } }
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
//Resources:UserAdminResources:Organizations_Title

		public static string Organizations_Title { get { return GetResourceString("Organizations_Title"); } }
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
//Resources:UserAdminResources:OrgLocation_AdminContact_Select

		public static string OrgLocation_AdminContact_Select { get { return GetResourceString("OrgLocation_AdminContact_Select"); } }
//Resources:UserAdminResources:OrgLocation_DeviceRepository

		public static string OrgLocation_DeviceRepository { get { return GetResourceString("OrgLocation_DeviceRepository"); } }
//Resources:UserAdminResources:OrgLocation_DeviceRepository_Select

		public static string OrgLocation_DeviceRepository_Select { get { return GetResourceString("OrgLocation_DeviceRepository_Select"); } }
//Resources:UserAdminResources:OrgLocation_Diagram

		public static string OrgLocation_Diagram { get { return GetResourceString("OrgLocation_Diagram"); } }
//Resources:UserAdminResources:OrgLocation_Diagram_Select

		public static string OrgLocation_Diagram_Select { get { return GetResourceString("OrgLocation_Diagram_Select"); } }
//Resources:UserAdminResources:OrgLocation_DistributionList

		public static string OrgLocation_DistributionList { get { return GetResourceString("OrgLocation_DistributionList"); } }
//Resources:UserAdminResources:OrgLocation_DistributionList_Select

		public static string OrgLocation_DistributionList_Select { get { return GetResourceString("OrgLocation_DistributionList_Select"); } }
//Resources:UserAdminResources:OrgLocation_GeoBoundingBox

		public static string OrgLocation_GeoBoundingBox { get { return GetResourceString("OrgLocation_GeoBoundingBox"); } }
//Resources:UserAdminResources:OrgLocation_GeoBoundingBox_Help

		public static string OrgLocation_GeoBoundingBox_Help { get { return GetResourceString("OrgLocation_GeoBoundingBox_Help"); } }
//Resources:UserAdminResources:OrgLocation_PhoneNumber

		public static string OrgLocation_PhoneNumber { get { return GetResourceString("OrgLocation_PhoneNumber"); } }
//Resources:UserAdminResources:OrgLocation_PrimaryDevice

		public static string OrgLocation_PrimaryDevice { get { return GetResourceString("OrgLocation_PrimaryDevice"); } }
//Resources:UserAdminResources:OrgLocation_PrimaryDevice_Select

		public static string OrgLocation_PrimaryDevice_Select { get { return GetResourceString("OrgLocation_PrimaryDevice_Select"); } }
//Resources:UserAdminResources:OrgLocation_TechnicalContact_Select

		public static string OrgLocation_TechnicalContact_Select { get { return GetResourceString("OrgLocation_TechnicalContact_Select"); } }
//Resources:UserAdminResources:Page_Help

		public static string Page_Help { get { return GetResourceString("Page_Help"); } }
//Resources:UserAdminResources:Page_IsForProductLine

		public static string Page_IsForProductLine { get { return GetResourceString("Page_IsForProductLine"); } }
//Resources:UserAdminResources:Page_Title

		public static string Page_Title { get { return GetResourceString("Page_Title"); } }
//Resources:UserAdminResources:PasskeyAuthenticationCompleteRequest_Description

		public static string PasskeyAuthenticationCompleteRequest_Description { get { return GetResourceString("PasskeyAuthenticationCompleteRequest_Description"); } }
//Resources:UserAdminResources:PasskeyAuthenticationCompleteRequest_Help

		public static string PasskeyAuthenticationCompleteRequest_Help { get { return GetResourceString("PasskeyAuthenticationCompleteRequest_Help"); } }
//Resources:UserAdminResources:PasskeyAuthenticationCompleteRequest_Name

		public static string PasskeyAuthenticationCompleteRequest_Name { get { return GetResourceString("PasskeyAuthenticationCompleteRequest_Name"); } }
//Resources:UserAdminResources:PasskeyAuthenticationCompleteRequestWire_Description

		public static string PasskeyAuthenticationCompleteRequestWire_Description { get { return GetResourceString("PasskeyAuthenticationCompleteRequestWire_Description"); } }
//Resources:UserAdminResources:PasskeyAuthenticationCompleteRequestWire_Help

		public static string PasskeyAuthenticationCompleteRequestWire_Help { get { return GetResourceString("PasskeyAuthenticationCompleteRequestWire_Help"); } }
//Resources:UserAdminResources:PasskeyAuthenticationCompleteRequestWire_Name

		public static string PasskeyAuthenticationCompleteRequestWire_Name { get { return GetResourceString("PasskeyAuthenticationCompleteRequestWire_Name"); } }
//Resources:UserAdminResources:PasskeyBeginOptionsResponse_Description

		public static string PasskeyBeginOptionsResponse_Description { get { return GetResourceString("PasskeyBeginOptionsResponse_Description"); } }
//Resources:UserAdminResources:PasskeyBeginOptionsResponse_Help

		public static string PasskeyBeginOptionsResponse_Help { get { return GetResourceString("PasskeyBeginOptionsResponse_Help"); } }
//Resources:UserAdminResources:PasskeyBeginOptionsResponse_Name

		public static string PasskeyBeginOptionsResponse_Name { get { return GetResourceString("PasskeyBeginOptionsResponse_Name"); } }
//Resources:UserAdminResources:PasskeyChallenge_Description

		public static string PasskeyChallenge_Description { get { return GetResourceString("PasskeyChallenge_Description"); } }
//Resources:UserAdminResources:PasskeyChallenge_Help

		public static string PasskeyChallenge_Help { get { return GetResourceString("PasskeyChallenge_Help"); } }
//Resources:UserAdminResources:PasskeyChallenge_Name

		public static string PasskeyChallenge_Name { get { return GetResourceString("PasskeyChallenge_Name"); } }
//Resources:UserAdminResources:PasskeyChallengePacket_Description

		public static string PasskeyChallengePacket_Description { get { return GetResourceString("PasskeyChallengePacket_Description"); } }
//Resources:UserAdminResources:PasskeyChallengePacket_Help

		public static string PasskeyChallengePacket_Help { get { return GetResourceString("PasskeyChallengePacket_Help"); } }
//Resources:UserAdminResources:PasskeyChallengePacket_Name

		public static string PasskeyChallengePacket_Name { get { return GetResourceString("PasskeyChallengePacket_Name"); } }
//Resources:UserAdminResources:PasskeyCredential_Description

		public static string PasskeyCredential_Description { get { return GetResourceString("PasskeyCredential_Description"); } }
//Resources:UserAdminResources:PasskeyCredential_Help

		public static string PasskeyCredential_Help { get { return GetResourceString("PasskeyCredential_Help"); } }
//Resources:UserAdminResources:PasskeyCredential_Name

		public static string PasskeyCredential_Name { get { return GetResourceString("PasskeyCredential_Name"); } }
//Resources:UserAdminResources:PasskeyCredentialIndex_Description

		public static string PasskeyCredentialIndex_Description { get { return GetResourceString("PasskeyCredentialIndex_Description"); } }
//Resources:UserAdminResources:PasskeyCredentialIndex_Help

		public static string PasskeyCredentialIndex_Help { get { return GetResourceString("PasskeyCredentialIndex_Help"); } }
//Resources:UserAdminResources:PasskeyCredentialIndex_Name

		public static string PasskeyCredentialIndex_Name { get { return GetResourceString("PasskeyCredentialIndex_Name"); } }
//Resources:UserAdminResources:PasskeyCredentialSummary_Description

		public static string PasskeyCredentialSummary_Description { get { return GetResourceString("PasskeyCredentialSummary_Description"); } }
//Resources:UserAdminResources:PasskeyCredentialSummary_Help

		public static string PasskeyCredentialSummary_Help { get { return GetResourceString("PasskeyCredentialSummary_Help"); } }
//Resources:UserAdminResources:PasskeyCredentialSummary_Name

		public static string PasskeyCredentialSummary_Name { get { return GetResourceString("PasskeyCredentialSummary_Name"); } }
//Resources:UserAdminResources:PasskeyRegistrationCompleteRequest_Description

		public static string PasskeyRegistrationCompleteRequest_Description { get { return GetResourceString("PasskeyRegistrationCompleteRequest_Description"); } }
//Resources:UserAdminResources:PasskeyRegistrationCompleteRequest_Help

		public static string PasskeyRegistrationCompleteRequest_Help { get { return GetResourceString("PasskeyRegistrationCompleteRequest_Help"); } }
//Resources:UserAdminResources:PasskeyRegistrationCompleteRequest_Name

		public static string PasskeyRegistrationCompleteRequest_Name { get { return GetResourceString("PasskeyRegistrationCompleteRequest_Name"); } }
//Resources:UserAdminResources:PasskeyRegistrationCompleteRequestWire_Description

		public static string PasskeyRegistrationCompleteRequestWire_Description { get { return GetResourceString("PasskeyRegistrationCompleteRequestWire_Description"); } }
//Resources:UserAdminResources:PasskeyRegistrationCompleteRequestWire_Help

		public static string PasskeyRegistrationCompleteRequestWire_Help { get { return GetResourceString("PasskeyRegistrationCompleteRequestWire_Help"); } }
//Resources:UserAdminResources:PasskeyRegistrationCompleteRequestWire_Name

		public static string PasskeyRegistrationCompleteRequestWire_Name { get { return GetResourceString("PasskeyRegistrationCompleteRequestWire_Name"); } }
//Resources:UserAdminResources:PasskeySignInResult_Description

		public static string PasskeySignInResult_Description { get { return GetResourceString("PasskeySignInResult_Description"); } }
//Resources:UserAdminResources:PasskeySignInResult_Help

		public static string PasskeySignInResult_Help { get { return GetResourceString("PasskeySignInResult_Help"); } }
//Resources:UserAdminResources:PasskeySignInResult_Name

		public static string PasskeySignInResult_Name { get { return GetResourceString("PasskeySignInResult_Name"); } }
//Resources:UserAdminResources:PaymentAccounts_Description

		public static string PaymentAccounts_Description { get { return GetResourceString("PaymentAccounts_Description"); } }
//Resources:UserAdminResources:PaymentAccounts_Help

		public static string PaymentAccounts_Help { get { return GetResourceString("PaymentAccounts_Help"); } }
//Resources:UserAdminResources:PaymentAccounts_Name

		public static string PaymentAccounts_Name { get { return GetResourceString("PaymentAccounts_Name"); } }
//Resources:UserAdminResources:PendingIdentity_Description

		public static string PendingIdentity_Description { get { return GetResourceString("PendingIdentity_Description"); } }
//Resources:UserAdminResources:PendingIdentity_Help

		public static string PendingIdentity_Help { get { return GetResourceString("PendingIdentity_Help"); } }
//Resources:UserAdminResources:PendingIdentity_Name

		public static string PendingIdentity_Name { get { return GetResourceString("PendingIdentity_Name"); } }
//Resources:UserAdminResources:PushNotificationChannel_Description

		public static string PushNotificationChannel_Description { get { return GetResourceString("PushNotificationChannel_Description"); } }
//Resources:UserAdminResources:PushNotificationChannel_Help

		public static string PushNotificationChannel_Help { get { return GetResourceString("PushNotificationChannel_Help"); } }
//Resources:UserAdminResources:PushNotificationChannel_Name

		public static string PushNotificationChannel_Name { get { return GetResourceString("PushNotificationChannel_Name"); } }
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
//Resources:UserAdminResources:RegisterUser_Description

		public static string RegisterUser_Description { get { return GetResourceString("RegisterUser_Description"); } }
//Resources:UserAdminResources:RegisterUser_Help

		public static string RegisterUser_Help { get { return GetResourceString("RegisterUser_Help"); } }
//Resources:UserAdminResources:RegisterUser_Name

		public static string RegisterUser_Name { get { return GetResourceString("RegisterUser_Name"); } }
//Resources:UserAdminResources:RegisterUserExists_3rdParty

		public static string RegisterUserExists_3rdParty { get { return GetResourceString("RegisterUserExists_3rdParty"); } }
//Resources:UserAdminResources:RegisterVM_Description

		public static string RegisterVM_Description { get { return GetResourceString("RegisterVM_Description"); } }
//Resources:UserAdminResources:RegisterVM_Help

		public static string RegisterVM_Help { get { return GetResourceString("RegisterVM_Help"); } }
//Resources:UserAdminResources:RegisterVM_Title

		public static string RegisterVM_Title { get { return GetResourceString("RegisterVM_Title"); } }
//Resources:UserAdminResources:RenamePasskeyRequest_Description

		public static string RenamePasskeyRequest_Description { get { return GetResourceString("RenamePasskeyRequest_Description"); } }
//Resources:UserAdminResources:RenamePasskeyRequest_Help

		public static string RenamePasskeyRequest_Help { get { return GetResourceString("RenamePasskeyRequest_Help"); } }
//Resources:UserAdminResources:RenamePasskeyRequest_Name

		public static string RenamePasskeyRequest_Name { get { return GetResourceString("RenamePasskeyRequest_Name"); } }
//Resources:UserAdminResources:ResetPassword_Description

		public static string ResetPassword_Description { get { return GetResourceString("ResetPassword_Description"); } }
//Resources:UserAdminResources:ResetPassword_Help

		public static string ResetPassword_Help { get { return GetResourceString("ResetPassword_Help"); } }
//Resources:UserAdminResources:ResetPassword_Name

		public static string ResetPassword_Name { get { return GetResourceString("ResetPassword_Name"); } }
//Resources:UserAdminResources:ResetPassword_Title

		public static string ResetPassword_Title { get { return GetResourceString("ResetPassword_Title"); } }
//Resources:UserAdminResources:Role_Description

		public static string Role_Description { get { return GetResourceString("Role_Description"); } }
//Resources:UserAdminResources:Role_Help

		public static string Role_Help { get { return GetResourceString("Role_Help"); } }
//Resources:UserAdminResources:Role_IsSystemRole

		public static string Role_IsSystemRole { get { return GetResourceString("Role_IsSystemRole"); } }
//Resources:UserAdminResources:Role_Title

		public static string Role_Title { get { return GetResourceString("Role_Title"); } }
//Resources:UserAdminResources:RoleAccess_Description

		public static string RoleAccess_Description { get { return GetResourceString("RoleAccess_Description"); } }
//Resources:UserAdminResources:RoleAccess_Help

		public static string RoleAccess_Help { get { return GetResourceString("RoleAccess_Help"); } }
//Resources:UserAdminResources:RoleAccess_Name

		public static string RoleAccess_Name { get { return GetResourceString("RoleAccess_Name"); } }
//Resources:UserAdminResources:RoleAccessDTO_Description

		public static string RoleAccessDTO_Description { get { return GetResourceString("RoleAccessDTO_Description"); } }
//Resources:UserAdminResources:RoleAccessDTO_Help

		public static string RoleAccessDTO_Help { get { return GetResourceString("RoleAccessDTO_Help"); } }
//Resources:UserAdminResources:RoleAccessDTO_Name

		public static string RoleAccessDTO_Name { get { return GetResourceString("RoleAccessDTO_Name"); } }
//Resources:UserAdminResources:ScheduledDowntime_AllDay

		public static string ScheduledDowntime_AllDay { get { return GetResourceString("ScheduledDowntime_AllDay"); } }
//Resources:UserAdminResources:ScheduledDowntime_Day

		public static string ScheduledDowntime_Day { get { return GetResourceString("ScheduledDowntime_Day"); } }
//Resources:UserAdminResources:ScheduledDowntime_DayOfWeek

		public static string ScheduledDowntime_DayOfWeek { get { return GetResourceString("ScheduledDowntime_DayOfWeek"); } }
//Resources:UserAdminResources:ScheduledDowntime_DayOfWeek_Select

		public static string ScheduledDowntime_DayOfWeek_Select { get { return GetResourceString("ScheduledDowntime_DayOfWeek_Select"); } }
//Resources:UserAdminResources:ScheduledDowntime_Description

		public static string ScheduledDowntime_Description { get { return GetResourceString("ScheduledDowntime_Description"); } }
//Resources:UserAdminResources:ScheduledDowntime_DowntimeType

		public static string ScheduledDowntime_DowntimeType { get { return GetResourceString("ScheduledDowntime_DowntimeType"); } }
//Resources:UserAdminResources:ScheduledDowntime_Help

		public static string ScheduledDowntime_Help { get { return GetResourceString("ScheduledDowntime_Help"); } }
//Resources:UserAdminResources:ScheduledDowntime_Month

		public static string ScheduledDowntime_Month { get { return GetResourceString("ScheduledDowntime_Month"); } }
//Resources:UserAdminResources:ScheduledDowntime_Periods

		public static string ScheduledDowntime_Periods { get { return GetResourceString("ScheduledDowntime_Periods"); } }
//Resources:UserAdminResources:ScheduledDowntime_ScheduleType

		public static string ScheduledDowntime_ScheduleType { get { return GetResourceString("ScheduledDowntime_ScheduleType"); } }
//Resources:UserAdminResources:ScheduledDowntime_ScheduleType_DayInMonth

		public static string ScheduledDowntime_ScheduleType_DayInMonth { get { return GetResourceString("ScheduledDowntime_ScheduleType_DayInMonth"); } }
//Resources:UserAdminResources:ScheduledDowntime_ScheduleType_DayOfWeek

		public static string ScheduledDowntime_ScheduleType_DayOfWeek { get { return GetResourceString("ScheduledDowntime_ScheduleType_DayOfWeek"); } }
//Resources:UserAdminResources:ScheduledDowntime_ScheduleType_DayOfWeekOfWeekInMonth

		public static string ScheduledDowntime_ScheduleType_DayOfWeekOfWeekInMonth { get { return GetResourceString("ScheduledDowntime_ScheduleType_DayOfWeekOfWeekInMonth"); } }
//Resources:UserAdminResources:ScheduledDowntime_ScheduleType_Select

		public static string ScheduledDowntime_ScheduleType_Select { get { return GetResourceString("ScheduledDowntime_ScheduleType_Select"); } }
//Resources:UserAdminResources:ScheduledDowntime_ScheduleType_SpecificDate

		public static string ScheduledDowntime_ScheduleType_SpecificDate { get { return GetResourceString("ScheduledDowntime_ScheduleType_SpecificDate"); } }
//Resources:UserAdminResources:ScheduledDowntime_Title

		public static string ScheduledDowntime_Title { get { return GetResourceString("ScheduledDowntime_Title"); } }
//Resources:UserAdminResources:ScheduledDowntime_Type

		public static string ScheduledDowntime_Type { get { return GetResourceString("ScheduledDowntime_Type"); } }
//Resources:UserAdminResources:ScheduledDowntime_Year

		public static string ScheduledDowntime_Year { get { return GetResourceString("ScheduledDowntime_Year"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_Description

		public static string ScheduledDowntimePeriod_Description { get { return GetResourceString("ScheduledDowntimePeriod_Description"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_End

		public static string ScheduledDowntimePeriod_End { get { return GetResourceString("ScheduledDowntimePeriod_End"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_End_Help

		public static string ScheduledDowntimePeriod_End_Help { get { return GetResourceString("ScheduledDowntimePeriod_End_Help"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_EndOfDay

		public static string ScheduledDowntimePeriod_EndOfDay { get { return GetResourceString("ScheduledDowntimePeriod_EndOfDay"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_EndOfDay_Help

		public static string ScheduledDowntimePeriod_EndOfDay_Help { get { return GetResourceString("ScheduledDowntimePeriod_EndOfDay_Help"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_Help

		public static string ScheduledDowntimePeriod_Help { get { return GetResourceString("ScheduledDowntimePeriod_Help"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_InvalidTimeFormat

		public static string ScheduledDowntimePeriod_InvalidTimeFormat { get { return GetResourceString("ScheduledDowntimePeriod_InvalidTimeFormat"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_Start

		public static string ScheduledDowntimePeriod_Start { get { return GetResourceString("ScheduledDowntimePeriod_Start"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_Start_Help

		public static string ScheduledDowntimePeriod_Start_Help { get { return GetResourceString("ScheduledDowntimePeriod_Start_Help"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_StartOfDay

		public static string ScheduledDowntimePeriod_StartOfDay { get { return GetResourceString("ScheduledDowntimePeriod_StartOfDay"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_StartOfDay_Help

		public static string ScheduledDowntimePeriod_StartOfDay_Help { get { return GetResourceString("ScheduledDowntimePeriod_StartOfDay_Help"); } }
//Resources:UserAdminResources:ScheduledDowntimePeriod_TItle

		public static string ScheduledDowntimePeriod_TItle { get { return GetResourceString("ScheduledDowntimePeriod_TItle"); } }
//Resources:UserAdminResources:ScheduledDowntimes_Title

		public static string ScheduledDowntimes_Title { get { return GetResourceString("ScheduledDowntimes_Title"); } }
//Resources:UserAdminResources:ScheduleType_WeekDay

		public static string ScheduleType_WeekDay { get { return GetResourceString("ScheduleType_WeekDay"); } }
//Resources:UserAdminResources:ScheduleType_WeekEnd

		public static string ScheduleType_WeekEnd { get { return GetResourceString("ScheduleType_WeekEnd"); } }
//Resources:UserAdminResources:ScheudledDowntime_Week

		public static string ScheudledDowntime_Week { get { return GetResourceString("ScheudledDowntime_Week"); } }
//Resources:UserAdminResources:SecureLink_Description

		public static string SecureLink_Description { get { return GetResourceString("SecureLink_Description"); } }
//Resources:UserAdminResources:SecureLink_Help

		public static string SecureLink_Help { get { return GetResourceString("SecureLink_Help"); } }
//Resources:UserAdminResources:SecureLink_Name

		public static string SecureLink_Name { get { return GetResourceString("SecureLink_Name"); } }
//Resources:UserAdminResources:SendCodeVM_Description

		public static string SendCodeVM_Description { get { return GetResourceString("SendCodeVM_Description"); } }
//Resources:UserAdminResources:SendCodeVM_Help

		public static string SendCodeVM_Help { get { return GetResourceString("SendCodeVM_Help"); } }
//Resources:UserAdminResources:SendCodeVM_Title

		public static string SendCodeVM_Title { get { return GetResourceString("SendCodeVM_Title"); } }
//Resources:UserAdminResources:SendResetPasswordLink_Description

		public static string SendResetPasswordLink_Description { get { return GetResourceString("SendResetPasswordLink_Description"); } }
//Resources:UserAdminResources:SendResetPasswordLink_Help

		public static string SendResetPasswordLink_Help { get { return GetResourceString("SendResetPasswordLink_Help"); } }
//Resources:UserAdminResources:SendResetPasswordLink_Name

		public static string SendResetPasswordLink_Name { get { return GetResourceString("SendResetPasswordLink_Name"); } }
//Resources:UserAdminResources:SentEmail_Description

		public static string SentEmail_Description { get { return GetResourceString("SentEmail_Description"); } }
//Resources:UserAdminResources:SentEmail_Title

		public static string SentEmail_Title { get { return GetResourceString("SentEmail_Title"); } }
//Resources:UserAdminResources:SentEmails_Title

		public static string SentEmails_Title { get { return GetResourceString("SentEmails_Title"); } }
//Resources:UserAdminResources:SetCondition_DontCare

		public static string SetCondition_DontCare { get { return GetResourceString("SetCondition_DontCare"); } }
//Resources:UserAdminResources:SetCondition_NotSet

		public static string SetCondition_NotSet { get { return GetResourceString("SetCondition_NotSet"); } }
//Resources:UserAdminResources:SetCondition_Set

		public static string SetCondition_Set { get { return GetResourceString("SetCondition_Set"); } }
//Resources:UserAdminResources:SetPasswordVM_Description

		public static string SetPasswordVM_Description { get { return GetResourceString("SetPasswordVM_Description"); } }
//Resources:UserAdminResources:SetPasswordVM_Help

		public static string SetPasswordVM_Help { get { return GetResourceString("SetPasswordVM_Help"); } }
//Resources:UserAdminResources:SetPasswordVM_Title

		public static string SetPasswordVM_Title { get { return GetResourceString("SetPasswordVM_Title"); } }
//Resources:UserAdminResources:Shape_Fill

		public static string Shape_Fill { get { return GetResourceString("Shape_Fill"); } }
//Resources:UserAdminResources:Shape_FlipX

		public static string Shape_FlipX { get { return GetResourceString("Shape_FlipX"); } }
//Resources:UserAdminResources:Shape_FlipY

		public static string Shape_FlipY { get { return GetResourceString("Shape_FlipY"); } }
//Resources:UserAdminResources:Shape_FontSize

		public static string Shape_FontSize { get { return GetResourceString("Shape_FontSize"); } }
//Resources:UserAdminResources:Shape_Locked

		public static string Shape_Locked { get { return GetResourceString("Shape_Locked"); } }
//Resources:UserAdminResources:Shape_Rotation

		public static string Shape_Rotation { get { return GetResourceString("Shape_Rotation"); } }
//Resources:UserAdminResources:Shape_Scale

		public static string Shape_Scale { get { return GetResourceString("Shape_Scale"); } }
//Resources:UserAdminResources:Shape_Stroke

		public static string Shape_Stroke { get { return GetResourceString("Shape_Stroke"); } }
//Resources:UserAdminResources:Shape_TextOffsetX

		public static string Shape_TextOffsetX { get { return GetResourceString("Shape_TextOffsetX"); } }
//Resources:UserAdminResources:Shape_TextOffsetY

		public static string Shape_TextOffsetY { get { return GetResourceString("Shape_TextOffsetY"); } }
//Resources:UserAdminResources:Shape_TextRotation

		public static string Shape_TextRotation { get { return GetResourceString("Shape_TextRotation"); } }
//Resources:UserAdminResources:ShapeType

		public static string ShapeType { get { return GetResourceString("ShapeType"); } }
//Resources:UserAdminResources:ShapeType_Building

		public static string ShapeType_Building { get { return GetResourceString("ShapeType_Building"); } }
//Resources:UserAdminResources:ShapeType_Circle

		public static string ShapeType_Circle { get { return GetResourceString("ShapeType_Circle"); } }
//Resources:UserAdminResources:ShapeType_Closet

		public static string ShapeType_Closet { get { return GetResourceString("ShapeType_Closet"); } }
//Resources:UserAdminResources:ShapeType_Door

		public static string ShapeType_Door { get { return GetResourceString("ShapeType_Door"); } }
//Resources:UserAdminResources:ShapeType_ExternalEntrance

		public static string ShapeType_ExternalEntrance { get { return GetResourceString("ShapeType_ExternalEntrance"); } }
//Resources:UserAdminResources:ShapeType_ParkingLot

		public static string ShapeType_ParkingLot { get { return GetResourceString("ShapeType_ParkingLot"); } }
//Resources:UserAdminResources:ShapeType_Polygon

		public static string ShapeType_Polygon { get { return GetResourceString("ShapeType_Polygon"); } }
//Resources:UserAdminResources:ShapeType_Room

		public static string ShapeType_Room { get { return GetResourceString("ShapeType_Room"); } }
//Resources:UserAdminResources:ShapeType_Select

		public static string ShapeType_Select { get { return GetResourceString("ShapeType_Select"); } }
//Resources:UserAdminResources:ShapeType_Window

		public static string ShapeType_Window { get { return GetResourceString("ShapeType_Window"); } }
//Resources:UserAdminResources:SingleUseToken_Description

		public static string SingleUseToken_Description { get { return GetResourceString("SingleUseToken_Description"); } }
//Resources:UserAdminResources:SingleUseToken_Help

		public static string SingleUseToken_Help { get { return GetResourceString("SingleUseToken_Help"); } }
//Resources:UserAdminResources:SingleUseToken_Name

		public static string SingleUseToken_Name { get { return GetResourceString("SingleUseToken_Name"); } }
//Resources:UserAdminResources:SMS_CouldNotVerify

		public static string SMS_CouldNotVerify { get { return GetResourceString("SMS_CouldNotVerify"); } }
//Resources:UserAdminResources:SMS_Verification_Body

		public static string SMS_Verification_Body { get { return GetResourceString("SMS_Verification_Body"); } }
//Resources:UserAdminResources:Subscription_Description

		public static string Subscription_Description { get { return GetResourceString("Subscription_Description"); } }
//Resources:UserAdminResources:Subscription_Help

		public static string Subscription_Help { get { return GetResourceString("Subscription_Help"); } }
//Resources:UserAdminResources:Subscription_PaymentMethod

		public static string Subscription_PaymentMethod { get { return GetResourceString("Subscription_PaymentMethod"); } }
//Resources:UserAdminResources:Subscription_PaymentMethod_Date

		public static string Subscription_PaymentMethod_Date { get { return GetResourceString("Subscription_PaymentMethod_Date"); } }
//Resources:UserAdminResources:Subscription_PaymentMethod_Expires

		public static string Subscription_PaymentMethod_Expires { get { return GetResourceString("Subscription_PaymentMethod_Expires"); } }
//Resources:UserAdminResources:Subscription_PaymentMethod_Help

		public static string Subscription_PaymentMethod_Help { get { return GetResourceString("Subscription_PaymentMethod_Help"); } }
//Resources:UserAdminResources:Subscription_PaymentMethod_Status

		public static string Subscription_PaymentMethod_Status { get { return GetResourceString("Subscription_PaymentMethod_Status"); } }
//Resources:UserAdminResources:Subscription_Status

		public static string Subscription_Status { get { return GetResourceString("Subscription_Status"); } }
//Resources:UserAdminResources:Subscription_Title

		public static string Subscription_Title { get { return GetResourceString("Subscription_Title"); } }
//Resources:UserAdminResources:Subscriptions_Title

		public static string Subscriptions_Title { get { return GetResourceString("Subscriptions_Title"); } }
//Resources:UserAdminResources:Team_Description

		public static string Team_Description { get { return GetResourceString("Team_Description"); } }
//Resources:UserAdminResources:Team_Help

		public static string Team_Help { get { return GetResourceString("Team_Help"); } }
//Resources:UserAdminResources:Team_Title

		public static string Team_Title { get { return GetResourceString("Team_Title"); } }
//Resources:UserAdminResources:TeamUser_Description

		public static string TeamUser_Description { get { return GetResourceString("TeamUser_Description"); } }
//Resources:UserAdminResources:TeamUser_Help

		public static string TeamUser_Help( string teamId, string userId) { return GetResourceString("TeamUser_Help", "{teamId}", teamId, "{userId}", userId); }
//Resources:UserAdminResources:TeamUser_Name

		public static string TeamUser_Name { get { return GetResourceString("TeamUser_Name"); } }
//Resources:UserAdminResources:TeamUserSummary_Description

		public static string TeamUserSummary_Description { get { return GetResourceString("TeamUserSummary_Description"); } }
//Resources:UserAdminResources:TeamUserSummary_Help

		public static string TeamUserSummary_Help { get { return GetResourceString("TeamUserSummary_Help"); } }
//Resources:UserAdminResources:TeamUserSummary_Name

		public static string TeamUserSummary_Name { get { return GetResourceString("TeamUserSummary_Name"); } }
//Resources:UserAdminResources:Technical_Contact

		public static string Technical_Contact { get { return GetResourceString("Technical_Contact"); } }
//Resources:UserAdminResources:TestRunArtifact_Description

		public static string TestRunArtifact_Description { get { return GetResourceString("TestRunArtifact_Description"); } }
//Resources:UserAdminResources:TestRunArtifact_Help

		public static string TestRunArtifact_Help { get { return GetResourceString("TestRunArtifact_Help"); } }
//Resources:UserAdminResources:TestRunArtifact_Name

		public static string TestRunArtifact_Name { get { return GetResourceString("TestRunArtifact_Name"); } }
//Resources:UserAdminResources:TestRunVerification_Description

		public static string TestRunVerification_Description { get { return GetResourceString("TestRunVerification_Description"); } }
//Resources:UserAdminResources:TestRunVerification_Help

		public static string TestRunVerification_Help { get { return GetResourceString("TestRunVerification_Help"); } }
//Resources:UserAdminResources:TestRunVerification_Name

		public static string TestRunVerification_Name { get { return GetResourceString("TestRunVerification_Name"); } }
//Resources:UserAdminResources:TestUserCredentials_Description

		public static string TestUserCredentials_Description { get { return GetResourceString("TestUserCredentials_Description"); } }
//Resources:UserAdminResources:TestUserCredentials_Help

		public static string TestUserCredentials_Help { get { return GetResourceString("TestUserCredentials_Help"); } }
//Resources:UserAdminResources:TestUserCredentials_Name

		public static string TestUserCredentials_Name { get { return GetResourceString("TestUserCredentials_Name"); } }
//Resources:UserAdminResources:TokenAuthOptions_Description

		public static string TokenAuthOptions_Description { get { return GetResourceString("TokenAuthOptions_Description"); } }
//Resources:UserAdminResources:TokenAuthOptions_Help

		public static string TokenAuthOptions_Help { get { return GetResourceString("TokenAuthOptions_Help"); } }
//Resources:UserAdminResources:TokenAuthOptions_Name

		public static string TokenAuthOptions_Name { get { return GetResourceString("TokenAuthOptions_Name"); } }
//Resources:UserAdminResources:TwitterAccessToken_Description

		public static string TwitterAccessToken_Description { get { return GetResourceString("TwitterAccessToken_Description"); } }
//Resources:UserAdminResources:TwitterAccessToken_Help

		public static string TwitterAccessToken_Help { get { return GetResourceString("TwitterAccessToken_Help"); } }
//Resources:UserAdminResources:TwitterAccessToken_Name

		public static string TwitterAccessToken_Name { get { return GetResourceString("TwitterAccessToken_Name"); } }
//Resources:UserAdminResources:TwitterError_Description

		public static string TwitterError_Description { get { return GetResourceString("TwitterError_Description"); } }
//Resources:UserAdminResources:TwitterError_Help

		public static string TwitterError_Help { get { return GetResourceString("TwitterError_Help"); } }
//Resources:UserAdminResources:TwitterError_Name

		public static string TwitterError_Name { get { return GetResourceString("TwitterError_Name"); } }
//Resources:UserAdminResources:TwitterErrorResponse_Description

		public static string TwitterErrorResponse_Description { get { return GetResourceString("TwitterErrorResponse_Description"); } }
//Resources:UserAdminResources:TwitterErrorResponse_Help

		public static string TwitterErrorResponse_Help { get { return GetResourceString("TwitterErrorResponse_Help"); } }
//Resources:UserAdminResources:TwitterErrorResponse_Name

		public static string TwitterErrorResponse_Name { get { return GetResourceString("TwitterErrorResponse_Name"); } }
//Resources:UserAdminResources:TwitterRequestToken_Description

		public static string TwitterRequestToken_Description { get { return GetResourceString("TwitterRequestToken_Description"); } }
//Resources:UserAdminResources:TwitterRequestToken_Help

		public static string TwitterRequestToken_Help { get { return GetResourceString("TwitterRequestToken_Help"); } }
//Resources:UserAdminResources:TwitterRequestToken_Name

		public static string TwitterRequestToken_Name { get { return GetResourceString("TwitterRequestToken_Name"); } }
//Resources:UserAdminResources:UiCategory_Description

		public static string UiCategory_Description { get { return GetResourceString("UiCategory_Description"); } }
//Resources:UserAdminResources:UiCategory_Icon

		public static string UiCategory_Icon { get { return GetResourceString("UiCategory_Icon"); } }
//Resources:UserAdminResources:UiCategory_Icon_Select

		public static string UiCategory_Icon_Select { get { return GetResourceString("UiCategory_Icon_Select"); } }
//Resources:UserAdminResources:UiCateogry_Title

		public static string UiCateogry_Title { get { return GetResourceString("UiCateogry_Title"); } }
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
//Resources:UserAdminResources:UserAccess_Description

		public static string UserAccess_Description { get { return GetResourceString("UserAccess_Description"); } }
//Resources:UserAdminResources:UserAccess_Help

		public static string UserAccess_Help { get { return GetResourceString("UserAccess_Help"); } }
//Resources:UserAdminResources:UserAccess_Name

		public static string UserAccess_Name { get { return GetResourceString("UserAccess_Name"); } }
//Resources:UserAdminResources:UserFavorite_Description

		public static string UserFavorite_Description { get { return GetResourceString("UserFavorite_Description"); } }
//Resources:UserAdminResources:UserFavorite_Help

		public static string UserFavorite_Help { get { return GetResourceString("UserFavorite_Help"); } }
//Resources:UserAdminResources:UserFavorite_Name

		public static string UserFavorite_Name { get { return GetResourceString("UserFavorite_Name"); } }
//Resources:UserAdminResources:UserFavorites_Description

		public static string UserFavorites_Description { get { return GetResourceString("UserFavorites_Description"); } }
//Resources:UserAdminResources:UserFavorites_Help

		public static string UserFavorites_Help { get { return GetResourceString("UserFavorites_Help"); } }
//Resources:UserAdminResources:UserFavorites_Name

		public static string UserFavorites_Name { get { return GetResourceString("UserFavorites_Name"); } }
//Resources:UserAdminResources:UserLoginResponse_Description

		public static string UserLoginResponse_Description { get { return GetResourceString("UserLoginResponse_Description"); } }
//Resources:UserAdminResources:UserLoginResponse_Help

		public static string UserLoginResponse_Help { get { return GetResourceString("UserLoginResponse_Help"); } }
//Resources:UserAdminResources:UserLoginResponse_Name

		public static string UserLoginResponse_Name { get { return GetResourceString("UserLoginResponse_Name"); } }
//Resources:UserAdminResources:UserRole_Description

		public static string UserRole_Description { get { return GetResourceString("UserRole_Description"); } }
//Resources:UserAdminResources:UserRole_Help

		public static string UserRole_Help { get { return GetResourceString("UserRole_Help"); } }
//Resources:UserAdminResources:UserRole_Name

		public static string UserRole_Name { get { return GetResourceString("UserRole_Name"); } }
//Resources:UserAdminResources:UserRoleDTO_Description

		public static string UserRoleDTO_Description { get { return GetResourceString("UserRoleDTO_Description"); } }
//Resources:UserAdminResources:UserRoleDTO_Help

		public static string UserRoleDTO_Help { get { return GetResourceString("UserRoleDTO_Help"); } }
//Resources:UserAdminResources:UserRoleDTO_Name

		public static string UserRoleDTO_Name { get { return GetResourceString("UserRoleDTO_Name"); } }
//Resources:UserAdminResources:VerfiyPhoneNumber_Description

		public static string VerfiyPhoneNumber_Description { get { return GetResourceString("VerfiyPhoneNumber_Description"); } }
//Resources:UserAdminResources:VerfiyPhoneNumber_Help

		public static string VerfiyPhoneNumber_Help { get { return GetResourceString("VerfiyPhoneNumber_Help"); } }
//Resources:UserAdminResources:VerfiyPhoneNumber_Name

		public static string VerfiyPhoneNumber_Name { get { return GetResourceString("VerfiyPhoneNumber_Name"); } }
//Resources:UserAdminResources:VerifyCodeViewModel_Code

		public static string VerifyCodeViewModel_Code { get { return GetResourceString("VerifyCodeViewModel_Code"); } }
//Resources:UserAdminResources:VerifyCodeViewModel_Provider

		public static string VerifyCodeViewModel_Provider { get { return GetResourceString("VerifyCodeViewModel_Provider"); } }
//Resources:UserAdminResources:VerifyCodeViewModel_RememberBrowser

		public static string VerifyCodeViewModel_RememberBrowser { get { return GetResourceString("VerifyCodeViewModel_RememberBrowser"); } }
//Resources:UserAdminResources:VerifyCodeViewModel_RememberMe

		public static string VerifyCodeViewModel_RememberMe { get { return GetResourceString("VerifyCodeViewModel_RememberMe"); } }
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
//Resources:UserAdminResources:VerticalAlign_Bottom

		public static string VerticalAlign_Bottom { get { return GetResourceString("VerticalAlign_Bottom"); } }
//Resources:UserAdminResources:VerticalAlign_Middle

		public static string VerticalAlign_Middle { get { return GetResourceString("VerticalAlign_Middle"); } }
//Resources:UserAdminResources:VerticalAlign_Top

		public static string VerticalAlign_Top { get { return GetResourceString("VerticalAlign_Top"); } }
//Resources:UserAdminResources:WebAuthnAssertionResponseWire_Description

		public static string WebAuthnAssertionResponseWire_Description { get { return GetResourceString("WebAuthnAssertionResponseWire_Description"); } }
//Resources:UserAdminResources:WebAuthnAssertionResponseWire_Help

		public static string WebAuthnAssertionResponseWire_Help { get { return GetResourceString("WebAuthnAssertionResponseWire_Help"); } }
//Resources:UserAdminResources:WebAuthnAssertionResponseWire_Name

		public static string WebAuthnAssertionResponseWire_Name { get { return GetResourceString("WebAuthnAssertionResponseWire_Name"); } }
//Resources:UserAdminResources:WebAuthnAssertionWire_Description

		public static string WebAuthnAssertionWire_Description { get { return GetResourceString("WebAuthnAssertionWire_Description"); } }
//Resources:UserAdminResources:WebAuthnAssertionWire_Help

		public static string WebAuthnAssertionWire_Help { get { return GetResourceString("WebAuthnAssertionWire_Help"); } }
//Resources:UserAdminResources:WebAuthnAssertionWire_Name

		public static string WebAuthnAssertionWire_Name { get { return GetResourceString("WebAuthnAssertionWire_Name"); } }
//Resources:UserAdminResources:WebAuthnAttestationResponseWire_Description

		public static string WebAuthnAttestationResponseWire_Description { get { return GetResourceString("WebAuthnAttestationResponseWire_Description"); } }
//Resources:UserAdminResources:WebAuthnAttestationResponseWire_Help

		public static string WebAuthnAttestationResponseWire_Help { get { return GetResourceString("WebAuthnAttestationResponseWire_Help"); } }
//Resources:UserAdminResources:WebAuthnAttestationResponseWire_Name

		public static string WebAuthnAttestationResponseWire_Name { get { return GetResourceString("WebAuthnAttestationResponseWire_Name"); } }
//Resources:UserAdminResources:WebAuthnAttestationWire_Description

		public static string WebAuthnAttestationWire_Description { get { return GetResourceString("WebAuthnAttestationWire_Description"); } }
//Resources:UserAdminResources:WebAuthnAttestationWire_Help

		public static string WebAuthnAttestationWire_Help { get { return GetResourceString("WebAuthnAttestationWire_Help"); } }
//Resources:UserAdminResources:WebAuthnAttestationWire_Name

		public static string WebAuthnAttestationWire_Name { get { return GetResourceString("WebAuthnAttestationWire_Name"); } }

		public static class Names
		{
			public const string AcceptInviteResponse_Description = "AcceptInviteResponse_Description";
			public const string AcceptInviteResponse_Help = "AcceptInviteResponse_Help";
			public const string AcceptInviteResponse_Name = "AcceptInviteResponse_Name";
			public const string AcceptInviteVM_Description = "AcceptInviteVM_Description";
			public const string AcceptInviteVM_Help = "AcceptInviteVM_Help";
			public const string AcceptInviteVM_Title = "AcceptInviteVM_Title";
			public const string AccessLog_Description = "AccessLog_Description";
			public const string AccessLog_Help = "AccessLog_Help";
			public const string AccessLog_Name = "AccessLog_Name";
			public const string Admin_Contact = "Admin_Contact";
			public const string AppUser_Bio = "AppUser_Bio";
			public const string AppUser_City = "AppUser_City";
			public const string AppUser_ConfirmPassword = "AppUser_ConfirmPassword";
			public const string AppUser_Country = "AppUser_Country";
			public const string AppUser_Description = "AppUser_Description";
			public const string AppUser_Email = "AppUser_Email";
			public const string AppUser_EmailSignature = "AppUser_EmailSignature";
			public const string AppUser_FirstName = "AppUser_FirstName";
			public const string AppUser_Help = "AppUser_Help";
			public const string Appuser_IsAccountDisabled = "Appuser_IsAccountDisabled";
			public const string AppUser_IsAppBuilder = "AppUser_IsAppBuilder";
			public const string AppUser_IsOrgAdmin = "AppUser_IsOrgAdmin";
			public const string Appuser_IsRuntimeUser = "Appuser_IsRuntimeUser";
			public const string AppUser_IsSystemAdmin = "AppUser_IsSystemAdmin";
			public const string AppUser_IsUserDevice = "AppUser_IsUserDevice";
			public const string AppUser_Language = "AppUser_Language";
			public const string AppUser_Language_Select = "AppUser_Language_Select";
			public const string AppUser_LastName = "AppUser_LastName";
			public const string AppUser_NewPassword = "AppUser_NewPassword";
			public const string AppUser_OldPassword = "AppUser_OldPassword";
			public const string AppUser_Password = "AppUser_Password";
			public const string AppUser_PasswordConfirmPasswordMatch = "AppUser_PasswordConfirmPasswordMatch";
			public const string AppUser_PhoneNumber = "AppUser_PhoneNumber";
			public const string AppUser_PhoneVerificationCode = "AppUser_PhoneVerificationCode";
			public const string AppUser_PrimaryOrganization = "AppUser_PrimaryOrganization";
			public const string AppUser_PrimaryOrganization_Help = "AppUser_PrimaryOrganization_Help";
			public const string AppUser_RememberMe = "AppUser_RememberMe";
			public const string AppUser_RememberMe_InThisBrowser = "AppUser_RememberMe_InThisBrowser";
			public const string AppUser_SSN = "AppUser_SSN";
			public const string AppUser_SSN_Help = "AppUser_SSN_Help";
			public const string AppUser_StateProvince = "AppUser_StateProvince";
			public const string AppUser_TeamsAccountName = "AppUser_TeamsAccountName";
			public const string AppUser_Title = "AppUser_Title";
			public const string AppUser_UserName = "AppUser_UserName";
			public const string AppUser_UserTitle = "AppUser_UserTitle";
			public const string AppUserInboxItem_Description = "AppUserInboxItem_Description";
			public const string AppUserInboxItem_Help = "AppUserInboxItem_Help";
			public const string AppUserInboxItem_Name = "AppUserInboxItem_Name";
			public const string AppUserTestingDSL_Action = "AppUserTestingDSL_Action";
			public const string AppUserTestingDSL_Action_Help = "AppUserTestingDSL_Action_Help";
			public const string AppUserTestingDSL_AuthView = "AppUserTestingDSL_AuthView";
			public const string AppUserTestingDSL_AuthView_Help = "AppUserTestingDSL_AuthView_Help";
			public const string AppUserTestingDSL_Expected = "AppUserTestingDSL_Expected";
			public const string AppUserTestingDSL_Expected_Help = "AppUserTestingDSL_Expected_Help";
			public const string AppUserTestingDSL_Inputs = "AppUserTestingDSL_Inputs";
			public const string AppUserTestingDSL_Inputs_Help = "AppUserTestingDSL_Inputs_Help";
			public const string AppUserTestingDSL_Preconditions = "AppUserTestingDSL_Preconditions";
			public const string AppUserTestingDSL_Preconditions_Help = "AppUserTestingDSL_Preconditions_Help";
			public const string AppUserTestingExpectedOutcome_Description = "AppUserTestingExpectedOutcome_Description";
			public const string AppUserTestingExpectedOutcome_ExpectedAuthLogEvents = "AppUserTestingExpectedOutcome_ExpectedAuthLogEvents";
			public const string AppUserTestingExpectedOutcome_ExpectedAuthLogEvents_Help = "AppUserTestingExpectedOutcome_ExpectedAuthLogEvents_Help";
			public const string AppUserTestingExpectedOutcome_ExpectedLanding = "AppUserTestingExpectedOutcome_ExpectedLanding";
			public const string AppUserTestingExpectedOutcome_ExpectedLanding_Help = "AppUserTestingExpectedOutcome_ExpectedLanding_Help";
			public const string AppUserTestingExpectedOutcome_Help = "AppUserTestingExpectedOutcome_Help";
			public const string AppUserTestingExpectedOutcome_Postconditions = "AppUserTestingExpectedOutcome_Postconditions";
			public const string AppUserTestingExpectedOutcome_Postconditions_Help = "AppUserTestingExpectedOutcome_Postconditions_Help";
			public const string AppUserTestingExpectedOutcome_Title = "AppUserTestingExpectedOutcome_Title";
			public const string AppUserTestRun_Description = "AppUserTestRun_Description";
			public const string AppUserTestRun_Help = "AppUserTestRun_Help";
			public const string AppUserTestRun_Name = "AppUserTestRun_Name";
			public const string AppUserTestRunEvent_Description = "AppUserTestRunEvent_Description";
			public const string AppUserTestRunEvent_Help = "AppUserTestRunEvent_Help";
			public const string AppUserTestRunEvent_Name = "AppUserTestRunEvent_Name";
			public const string AppUserTestRunSummary_Description = "AppUserTestRunSummary_Description";
			public const string AppUserTestRunSummary_Help = "AppUserTestRunSummary_Help";
			public const string AppUserTestRunSummary_Name = "AppUserTestRunSummary_Name";
			public const string AppUserTestScenario_AuthView = "AppUserTestScenario_AuthView";
			public const string AppUserTestScenario_AuthView_Help = "AppUserTestScenario_AuthView_Help";
			public const string Area_Help = "Area_Help";
			public const string Area_IsForProductLine = "Area_IsForProductLine";
			public const string Area_PageCategories = "Area_PageCategories";
			public const string Area_Title = "Area_Title";
			public const string AssetSet_Description = "AssetSet_Description";
			public const string AssetSet_Help = "AssetSet_Help";
			public const string AssetSet_IsRestricted = "AssetSet_IsRestricted";
			public const string AssetSet_IsRestricted_Help = "AssetSet_IsRestricted_Help";
			public const string AssetSet_Title = "AssetSet_Title";
			public const string AuthDSL_AuthView = "AuthDSL_AuthView";
			public const string AuthDSL_Description = "AuthDSL_Description";
			public const string AuthDSL_Help = "AuthDSL_Help";
			public const string AuthDSL_Title = "AuthDSL_Title";
			public const string AuthenticationLogs_Description = "AuthenticationLogs_Description";
			public const string AuthenticationLogs_Title = "AuthenticationLogs_Title";
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
			public const string AuthFailureRecord_Description = "AuthFailureRecord_Description";
			public const string AuthFailureRecord_Help = "AuthFailureRecord_Help";
			public const string AuthFailureRecord_Name = "AuthFailureRecord_Name";
			public const string AuthFieldAction_Description = "AuthFieldAction_Description";
			public const string AuthFieldAction_Finder = "AuthFieldAction_Finder";
			public const string AuthFieldAction_Finder_Help = "AuthFieldAction_Finder_Help";
			public const string AuthFieldAction_Help = "AuthFieldAction_Help";
			public const string AuthFieldAction_Name = "AuthFieldAction_Name";
			public const string AuthFieldAction_Name_Help = "AuthFieldAction_Name_Help";
			public const string AuthFieldAction_Title = "AuthFieldAction_Title";
			public const string AuthLoginRequest_Description = "AuthLoginRequest_Description";
			public const string AuthLoginRequest_Help = "AuthLoginRequest_Help";
			public const string AuthLoginRequest_Name = "AuthLoginRequest_Name";
			public const string AuthLogReviewSummary_Description = "AuthLogReviewSummary_Description";
			public const string AuthLogReviewSummary_Help = "AuthLogReviewSummary_Help";
			public const string AuthLogReviewSummary_Name = "AuthLogReviewSummary_Name";
			public const string AuthResponse_Description = "AuthResponse_Description";
			public const string AuthResponse_Help = "AuthResponse_Help";
			public const string AuthResponse_Name = "AuthResponse_Name";
			public const string AuthRunnerAction_Description = "AuthRunnerAction_Description";
			public const string AuthRunnerAction_Help = "AuthRunnerAction_Help";
			public const string AuthRunnerAction_Name = "AuthRunnerAction_Name";
			public const string AuthRunnerArtifact_Description = "AuthRunnerArtifact_Description";
			public const string AuthRunnerArtifact_Help = "AuthRunnerArtifact_Help";
			public const string AuthRunnerArtifact_Name = "AuthRunnerArtifact_Name";
			public const string AuthRunnerInput_Description = "AuthRunnerInput_Description";
			public const string AuthRunnerInput_Help = "AuthRunnerInput_Help";
			public const string AuthRunnerInput_Name = "AuthRunnerInput_Name";
			public const string AuthRunnerObservations_Description = "AuthRunnerObservations_Description";
			public const string AuthRunnerObservations_Help = "AuthRunnerObservations_Help";
			public const string AuthRunnerObservations_Name = "AuthRunnerObservations_Name";
			public const string AuthRunnerOptions_Description = "AuthRunnerOptions_Description";
			public const string AuthRunnerOptions_Help = "AuthRunnerOptions_Help";
			public const string AuthRunnerOptions_Name = "AuthRunnerOptions_Name";
			public const string AuthRunnerPlan_Description = "AuthRunnerPlan_Description";
			public const string AuthRunnerPlan_Help = "AuthRunnerPlan_Help";
			public const string AuthRunnerPlan_Name = "AuthRunnerPlan_Name";
			public const string AuthRunnerResult_Description = "AuthRunnerResult_Description";
			public const string AuthRunnerResult_Help = "AuthRunnerResult_Help";
			public const string AuthRunnerResult_Name = "AuthRunnerResult_Name";
			public const string AuthSingleuseToken_TokenNotFound = "AuthSingleuseToken_TokenNotFound";
			public const string AuthSingleuseToken_UserNotFound = "AuthSingleuseToken_UserNotFound";
			public const string AuthTenantStateSnapshot_BelongsToOrg = "AuthTenantStateSnapshot_BelongsToOrg";
			public const string AuthTenantStateSnapshot_BelongsToOrg_Help = "AuthTenantStateSnapshot_BelongsToOrg_Help";
			public const string AuthTenantStateSnapshot_Description = "AuthTenantStateSnapshot_Description";
			public const string AuthTenantStateSnapshot_EmailConfirmed = "AuthTenantStateSnapshot_EmailConfirmed";
			public const string AuthTenantStateSnapshot_EmailConfirmed_Help = "AuthTenantStateSnapshot_EmailConfirmed_Help";
			public const string AuthTenantStateSnapshot_EnsureUserDoesNotExist = "AuthTenantStateSnapshot_EnsureUserDoesNotExist";
			public const string AuthTenantStateSnapshot_EnsureUserDoesNotExist_Help = "AuthTenantStateSnapshot_EnsureUserDoesNotExist_Help";
			public const string AuthTenantStateSnapshot_EnsureUserExists = "AuthTenantStateSnapshot_EnsureUserExists";
			public const string AuthTenantStateSnapshot_EnsureUserExists_Help = "AuthTenantStateSnapshot_EnsureUserExists_Help";
			public const string AuthTenantStateSnapshot_ExternalLoginProviders = "AuthTenantStateSnapshot_ExternalLoginProviders";
			public const string AuthTenantStateSnapshot_ExternalLoginProviders_Help = "AuthTenantStateSnapshot_ExternalLoginProviders_Help";
			public const string AuthTenantStateSnapshot_GenerateEmailConfirm = "AuthTenantStateSnapshot_GenerateEmailConfirm";
			public const string AuthTenantStateSnapshot_HasPasskey = "AuthTenantStateSnapshot_HasPasskey";
			public const string AuthTenantStateSnapshot_HasPassword = "AuthTenantStateSnapshot_HasPassword";
			public const string AuthTenantStateSnapshot_HasPendingIdentity = "AuthTenantStateSnapshot_HasPendingIdentity";
			public const string AuthTenantStateSnapshot_HasTotp = "AuthTenantStateSnapshot_HasTotp";
			public const string AuthTenantStateSnapshot_Help = "AuthTenantStateSnapshot_Help";
			public const string AuthTenantStateSnapshot_IsAccountDisabled = "AuthTenantStateSnapshot_IsAccountDisabled";
			public const string AuthTenantStateSnapshot_IsAccountDisabled_Help = "AuthTenantStateSnapshot_IsAccountDisabled_Help";
			public const string AuthTenantStateSnapshot_IsAnonymous = "AuthTenantStateSnapshot_IsAnonymous";
			public const string AuthTenantStateSnapshot_IsAnonymous_Help = "AuthTenantStateSnapshot_IsAnonymous_Help";
			public const string AuthTenantStateSnapshot_IsLoggedIn = "AuthTenantStateSnapshot_IsLoggedIn";
			public const string AuthTenantStateSnapshot_IsLoggedIn_Help = "AuthTenantStateSnapshot_IsLoggedIn_Help";
			public const string AuthTenantStateSnapshot_IsOrgAdmin = "AuthTenantStateSnapshot_IsOrgAdmin";
			public const string AuthTenantStateSnapshot_LastMfaDateTimeUtc = "AuthTenantStateSnapshot_LastMfaDateTimeUtc";
			public const string AuthTenantStateSnapshot_LastMfaDateTimeUtc_Help = "AuthTenantStateSnapshot_LastMfaDateTimeUtc_Help";
			public const string AuthTenantStateSnapshot_PhoneNumberConfirmed = "AuthTenantStateSnapshot_PhoneNumberConfirmed";
			public const string AuthTenantStateSnapshot_PhoneNumberConfirmed_Help = "AuthTenantStateSnapshot_PhoneNumberConfirmed_Help";
			public const string AuthTenantStateSnapshot_SendMagicLink = "AuthTenantStateSnapshot_SendMagicLink";
			public const string AuthTenantStateSnapshot_ShouldInviteToOrg = "AuthTenantStateSnapshot_ShouldInviteToOrg";
			public const string AuthTenantStateSnapshot_ShouldSendConfirmationEmail = "AuthTenantStateSnapshot_ShouldSendConfirmationEmail";
			public const string AuthTenantStateSnapshot_ShowWelcome = "AuthTenantStateSnapshot_ShowWelcome";
			public const string AuthTenantStateSnapshot_ShowWelcome_Help = "AuthTenantStateSnapshot_ShowWelcome_Help";
			public const string AuthTenantStateSnapshot_Title = "AuthTenantStateSnapshot_Title";
			public const string AuthTenantStateSnapshot_TwoFactorEnabled = "AuthTenantStateSnapshot_TwoFactorEnabled";
			public const string AuthTenantStateSnapshot_TwoFactorEnabled_Help = "AuthTenantStateSnapshot_TwoFactorEnabled_Help";
			public const string AuthTenantStateSnapshot_UserHasEmail = "AuthTenantStateSnapshot_UserHasEmail";
			public const string AuthTenantStateSnapshot_UserHasFirstName = "AuthTenantStateSnapshot_UserHasFirstName";
			public const string AuthTenantStateSnapshot_UserHasLastName = "AuthTenantStateSnapshot_UserHasLastName";
			public const string AuthTenantStateSnapshot_UserHasOAuthGitHub = "AuthTenantStateSnapshot_UserHasOAuthGitHub";
			public const string AuthView_Actions = "AuthView_Actions";
			public const string AuthView_Actions_Help = "AuthView_Actions_Help";
			public const string AuthView_Description = "AuthView_Description";
			public const string AuthView_Fields = "AuthView_Fields";
			public const string AuthView_FormFiield_Description = "AuthView_FormFiield_Description";
			public const string AuthView_Help = "AuthView_Help";
			public const string AuthView_Route = "AuthView_Route";
			public const string AuthView_Route_Help = "AuthView_Route_Help";
			public const string AuthView_RouteHelp = "AuthView_RouteHelp";
			public const string AuthView_Title = "AuthView_Title";
			public const string AuthView_ViewId = "AuthView_ViewId";
			public const string AuthView_Views = "AuthView_Views";
			public const string AuthView_Views_Help = "AuthView_Views_Help";
			public const string AuthViewAction_Description = "AuthViewAction_Description";
			public const string AuthViewAction_Finder = "AuthViewAction_Finder";
			public const string AuthViewField_Description = "AuthViewField_Description";
			public const string AuthViewField_FieldName = "AuthViewField_FieldName";
			public const string AuthViewField_FieldType = "AuthViewField_FieldType";
			public const string AuthViewField_Finder = "AuthViewField_Finder";
			public const string AuthViewField_Finder_Help = "AuthViewField_Finder_Help";
			public const string AuthViewField_Help = "AuthViewField_Help";
			public const string AuthViewField_Name = "AuthViewField_Name";
			public const string AuthViewField_Name_Help = "AuthViewField_Name_Help";
			public const string AuthViewField_Title = "AuthViewField_Title";
			public const string AuthViewFormField_Title = "AuthViewFormField_Title";
			public const string AuthViews_Title = "AuthViews_Title";
			public const string BasicTheme_Description = "BasicTheme_Description";
			public const string BasicTheme_Help = "BasicTheme_Help";
			public const string BasicTheme_Name = "BasicTheme_Name";
			public const string Billing_Contact = "Billing_Contact";
			public const string Calendar_AllDay = "Calendar_AllDay";
			public const string Calendar_Date = "Calendar_Date";
			public const string Calendar_Description = "Calendar_Description";
			public const string Calendar_End = "Calendar_End";
			public const string Calendar_EventLink = "Calendar_EventLink";
			public const string Calendar_EventType = "Calendar_EventType";
			public const string Calendar_EventType_Select = "Calendar_EventType_Select";
			public const string Calendar_ObjectDescription = "Calendar_ObjectDescription";
			public const string Calendar_ObjectTitle = "Calendar_ObjectTitle";
			public const string Calendar_Start = "Calendar_Start";
			public const string CalendarEvent_Title = "CalendarEvent_Title";
			public const string CalendarEventType_Holiday = "CalendarEventType_Holiday";
			public const string CalendarEventType_Networking = "CalendarEventType_Networking";
			public const string CalendarEventType_Other = "CalendarEventType_Other";
			public const string CalendarEventType_OutOfOffice = "CalendarEventType_OutOfOffice";
			public const string CalendarEventType_UserGroup = "CalendarEventType_UserGroup";
			public const string CallLog_Description = "CallLog_Description";
			public const string CallLog_Help = "CallLog_Help";
			public const string CallLog_Name = "CallLog_Name";
			public const string ChangePassword_Description = "ChangePassword_Description";
			public const string ChangePassword_Help = "ChangePassword_Help";
			public const string ChangePassword_Name = "ChangePassword_Name";
			public const string ChangePasswordVM_Description = "ChangePasswordVM_Description";
			public const string ChangePasswordVM_Help = "ChangePasswordVM_Help";
			public const string ChangePasswordVM_Title = "ChangePasswordVM_Title";
			public const string ClientApp_AppAuthKey1 = "ClientApp_AppAuthKey1";
			public const string ClientApp_AppAuthKey2 = "ClientApp_AppAuthKey2";
			public const string ClientApp_Description = "ClientApp_Description";
			public const string ClientApp_DeviceConfigs = "ClientApp_DeviceConfigs";
			public const string ClientApp_DeviceTypes = "ClientApp_DeviceTypes";
			public const string ClientApp_Help = "ClientApp_Help";
			public const string ClientApp_Instance = "ClientApp_Instance";
			public const string ClientApp_SelectInstance = "ClientApp_SelectInstance";
			public const string ClientApp_Title = "ClientApp_Title";
			public const string Common_Address1 = "Common_Address1";
			public const string Common_Address2 = "Common_Address2";
			public const string Common_Category = "Common_Category";
			public const string Common_Category_Select = "Common_Category_Select";
			public const string Common_City = "Common_City";
			public const string Common_Country = "Common_Country";
			public const string Common_CreatedBy = "Common_CreatedBy";
			public const string Common_CreationDate = "Common_CreationDate";
			public const string Common_Customer = "Common_Customer";
			public const string Common_Description = "Common_Description";
			public const string Common_DesktopSupport = "Common_DesktopSupport";
			public const string Common_EmailAddress = "Common_EmailAddress";
			public const string Common_FillColor = "Common_FillColor";
			public const string Common_HorizontalAlign = "Common_HorizontalAlign";
			public const string Common_HorizontalAlign_Select = "Common_HorizontalAlign_Select";
			public const string Common_Icon = "Common_Icon";
			public const string Common_Icon_Size_ExtraLarge = "Common_Icon_Size_ExtraLarge";
			public const string Common_Icon_Size_Large = "Common_Icon_Size_Large";
			public const string Common_Icon_Size_Medium = "Common_Icon_Size_Medium";
			public const string Common_Icon_Size_Small = "Common_Icon_Size_Small";
			public const string Common_IconSize = "Common_IconSize";
			public const string Common_IconSize_Select = "Common_IconSize_Select";
			public const string Common_Id = "Common_Id";
			public const string Common_IsPublic = "Common_IsPublic";
			public const string Common_Key = "Common_Key";
			public const string Common_Key_Help = "Common_Key_Help";
			public const string Common_Key_Validation = "Common_Key_Validation";
			public const string Common_LastUpdatedBy = "Common_LastUpdatedBy";
			public const string Common_LastUpdatedDate = "Common_LastUpdatedDate";
			public const string Common_Name = "Common_Name";
			public const string Common_Namespace = "Common_Namespace";
			public const string Common_Notes = "Common_Notes";
			public const string Common_PhoneNumber = "Common_PhoneNumber";
			public const string Common_PhoneSupport = "Common_PhoneSupport";
			public const string Common_PostalCode = "Common_PostalCode";
			public const string Common_PrimaryBgColor = "Common_PrimaryBgColor";
			public const string Common_PrimaryTextColor = "Common_PrimaryTextColor";
			public const string Common_Role = "Common_Role";
			public const string Common_SelectDevice = "Common_SelectDevice";
			public const string Common_SelectLocation = "Common_SelectLocation";
			public const string Common_State = "Common_State";
			public const string Common_Status = "Common_Status";
			public const string Common_StrokeColor = "Common_StrokeColor";
			public const string Common_TabletSupport = "Common_TabletSupport";
			public const string Common_TextColor = "Common_TextColor";
			public const string Common_TimeZome = "Common_TimeZome";
			public const string Common_TimeZome_Picker = "Common_TimeZome_Picker";
			public const string Common_VerticalAlign = "Common_VerticalAlign";
			public const string Common_VerticalAlign_Select = "Common_VerticalAlign_Select";
			public const string ConfirmEmail_Description = "ConfirmEmail_Description";
			public const string ConfirmEmail_Help = "ConfirmEmail_Help";
			public const string ConfirmEmail_Name = "ConfirmEmail_Name";
			public const string ContactList_Description = "ContactList_Description";
			public const string ContactList_Title = "ContactList_Title";
			public const string CoreUserInfo_Description = "CoreUserInfo_Description";
			public const string CoreUserInfo_Help = "CoreUserInfo_Help";
			public const string CoreUserInfo_Name = "CoreUserInfo_Name";
			public const string CreateLocationVM_Description = "CreateLocationVM_Description";
			public const string CreateLocationVM_Help = "CreateLocationVM_Help";
			public const string CreateLocationVM_Title = "CreateLocationVM_Title";
			public const string CreateOrganizationVM_Description = "CreateOrganizationVM_Description";
			public const string CreateOrganizationVM_Help = "CreateOrganizationVM_Help";
			public const string CreateOrganizationVM_Title = "CreateOrganizationVM_Title";
			public const string CreateUserResponse_Description = "CreateUserResponse_Description";
			public const string CreateUserResponse_Help = "CreateUserResponse_Help";
			public const string CreateUserResponse_Name = "CreateUserResponse_Name";
			public const string DayOfWeek_Friday = "DayOfWeek_Friday";
			public const string DayOfWeek_Monday = "DayOfWeek_Monday";
			public const string DayOfWeek_Saturday = "DayOfWeek_Saturday";
			public const string DayOfWeek_Select = "DayOfWeek_Select";
			public const string DayOfWeek_Sunday = "DayOfWeek_Sunday";
			public const string DayOfWeek_Thursday = "DayOfWeek_Thursday";
			public const string DayOfWeek_Tuesday = "DayOfWeek_Tuesday";
			public const string DayOfWeek_Wednesday = "DayOfWeek_Wednesday";
			public const string DeviceOwner_Description = "DeviceOwner_Description";
			public const string DeviceOwner_Devices = "DeviceOwner_Devices";
			public const string DeviceOwner_EmailAddress = "DeviceOwner_EmailAddress";
			public const string DeviceOwner_FirstName = "DeviceOwner_FirstName";
			public const string DeviceOwner_LastName = "DeviceOwner_LastName";
			public const string DeviceOwner_Password = "DeviceOwner_Password";
			public const string DeviceOwner_Password_Confirm = "DeviceOwner_Password_Confirm";
			public const string DeviceOwner_PhoneNumber = "DeviceOwner_PhoneNumber";
			public const string DeviceOwner_Title = "DeviceOwner_Title";
			public const string DeviceOwners_Title = "DeviceOwners_Title";
			public const string DeviceOwnersDevices_Description = "DeviceOwnersDevices_Description";
			public const string DeviceOwnersDevices_Device = "DeviceOwnersDevices_Device";
			public const string DeviceOwnersDevices_DeviceId = "DeviceOwnersDevices_DeviceId";
			public const string DeviceOwnersDevices_DeviceRepository = "DeviceOwnersDevices_DeviceRepository";
			public const string DeviceOwnersDevices_Organization = "DeviceOwnersDevices_Organization";
			public const string DeviceOwnersDevices_Product = "DeviceOwnersDevices_Product";
			public const string DeviceOwnersDevices_Title = "DeviceOwnersDevices_Title";
			public const string Diagram_Units = "Diagram_Units";
			public const string Diagram_Units_Select = "Diagram_Units_Select";
			public const string DiagramUnits_Feet = "DiagramUnits_Feet";
			public const string DiagramUnits_Meters = "DiagramUnits_Meters";
			public const string DistributionList_ExternalContacts = "DistributionList_ExternalContacts";
			public const string DistroList_Description = "DistroList_Description";
			public const string DistroList_Help = "DistroList_Help";
			public const string DistroList_Name = "DistroList_Name";
			public const string DistroList_ParentList = "DistroList_ParentList";
			public const string DistroList_ParentList_Help = "DistroList_ParentList_Help";
			public const string DistroLists_Name = "DistroLists_Name";
			public const string DownTimeType_Admin = "DownTimeType_Admin";
			public const string DownTimeType_Holiday = "DownTimeType_Holiday";
			public const string DownTimeType_Other = "DownTimeType_Other";
			public const string DownTimeType_ScheduledMaintenance = "DownTimeType_ScheduledMaintenance";
			public const string DownTimeType_Select = "DownTimeType_Select";
			public const string DownTimeType_WorkWeek = "DownTimeType_WorkWeek";
			public const string Email_ResetPassword_Body = "Email_ResetPassword_Body";
			public const string Email_ResetPassword_Subject = "Email_ResetPassword_Subject";
			public const string Email_RestPassword_ErrorSending = "Email_RestPassword_ErrorSending";
			public const string Email_Verification_Body = "Email_Verification_Body";
			public const string Email_Verification_Subject = "Email_Verification_Subject";
			public const string EmailSender_Description = "EmailSender_Description";
			public const string EmailSender_From = "EmailSender_From";
			public const string EmailSender_FromAddress = "EmailSender_FromAddress";
			public const string EmailSender_FromName = "EmailSender_FromName";
			public const string EmailSender_ReplyTo = "EmailSender_ReplyTo";
			public const string EmailSender_ReplyToAddress = "EmailSender_ReplyToAddress";
			public const string EmailSender_ReplyToName = "EmailSender_ReplyToName";
			public const string EmailSender_Title = "EmailSender_Title";
			public const string EmailSenders_Title = "EmailSenders_Title";
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
			public const string ExternalContact_Description = "ExternalContact_Description";
			public const string ExternalContact_SendEmail = "ExternalContact_SendEmail";
			public const string ExternalContact_SendSMS = "ExternalContact_SendSMS";
			public const string ExternalContact_Title = "ExternalContact_Title";
			public const string ExternalLoginConfirmVM_Description = "ExternalLoginConfirmVM_Description";
			public const string ExternalLoginConfirmVM_Help = "ExternalLoginConfirmVM_Help";
			public const string ExternalLoginConfirmVM_Title = "ExternalLoginConfirmVM_Title";
			public const string FavoritesByModule_Description = "FavoritesByModule_Description";
			public const string FavoritesByModule_Help = "FavoritesByModule_Help";
			public const string FavoritesByModule_Name = "FavoritesByModule_Name";
			public const string Feature_Help = "Feature_Help";
			public const string Feature_Icon = "Feature_Icon";
			public const string Feature_MenuTitle = "Feature_MenuTitle";
			public const string Feature_Summary = "Feature_Summary";
			public const string Feature_TItle = "Feature_TItle";
			public const string ForgotPasswordVM_Description = "ForgotPasswordVM_Description";
			public const string ForgotPasswordVM_Help = "ForgotPasswordVM_Help";
			public const string ForgotPasswordVM_Title = "ForgotPasswordVM_Title";
			public const string FunctionMap_Description = "FunctionMap_Description";
			public const string FunctionMap_Functions = "FunctionMap_Functions";
			public const string FunctionMap_Title = "FunctionMap_Title";
			public const string FunctionMapFunction_ChildFunctionMap = "FunctionMapFunction_ChildFunctionMap";
			public const string FunctionMapFunction_ChildFunctionMap_Select = "FunctionMapFunction_ChildFunctionMap_Select";
			public const string FunctionMapFunction_Description = "FunctionMapFunction_Description";
			public const string FunctionMapFunction_FuctionView = "FunctionMapFunction_FuctionView";
			public const string FunctionMapFunction_FuctionView_SelectView = "FunctionMapFunction_FuctionView_SelectView";
			public const string FunctionMapFunction_IconShape = "FunctionMapFunction_IconShape";
			public const string FunctionMapFunction_IconShape_Landscape = "FunctionMapFunction_IconShape_Landscape";
			public const string FunctionMapFunction_IconShape_Portrait = "FunctionMapFunction_IconShape_Portrait";
			public const string FunctionMapFunction_IconShape_Select = "FunctionMapFunction_IconShape_Select";
			public const string FunctionMapFunction_IconShape_Square = "FunctionMapFunction_IconShape_Square";
			public const string FunctionMapFunction_ImageIcon = "FunctionMapFunction_ImageIcon";
			public const string FunctionMapFunction_Title = "FunctionMapFunction_Title";
			public const string FunctionMapFunction_Type = "FunctionMapFunction_Type";
			public const string FunctionMapFunction_Type_ChildFunctionMap = "FunctionMapFunction_Type_ChildFunctionMap";
			public const string FunctionMapFunction_Type_FunctionView = "FunctionMapFunction_Type_FunctionView";
			public const string FunctionMapFunction_Type_Select = "FunctionMapFunction_Type_Select";
			public const string GeoLocation_Description = "GeoLocation_Description";
			public const string GeoLocation_Help = "GeoLocation_Help";
			public const string GeoLocation_Title = "GeoLocation_Title";
			public const string HelpResource_Description = "HelpResource_Description";
			public const string HelpResource_DisplayInline = "HelpResource_DisplayInline";
			public const string HelpResource_Guide = "HelpResource_Guide";
			public const string HelpResource_Link = "HelpResource_Link";
			public const string HelpResource_SelectGuide = "HelpResource_SelectGuide";
			public const string HelpResource_Summary = "HelpResource_Summary";
			public const string HelpResource_Title = "HelpResource_Title";
			public const string HolidaySet_Culture_Or_Country = "HolidaySet_Culture_Or_Country";
			public const string HolidaySet_Description = "HolidaySet_Description";
			public const string HolidaySet_Help = "HolidaySet_Help";
			public const string HolidaySet_Holidays = "HolidaySet_Holidays";
			public const string HolidaySet_Title = "HolidaySet_Title";
			public const string HolidaySets_Title = "HolidaySets_Title";
			public const string HorizontalAlign_Center = "HorizontalAlign_Center";
			public const string HorizontalAlign_Left = "HorizontalAlign_Left";
			public const string HorizontalAlign_Right = "HorizontalAlign_Right";
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
			public const string Invite_Customer = "Invite_Customer";
			public const string Invite_CustomerContact = "Invite_CustomerContact";
			public const string Invite_CustomerContactId = "Invite_CustomerContactId";
			public const string Invite_CustomerId = "Invite_CustomerId";
			public const string Invite_EndUserAppOrg = "Invite_EndUserAppOrg";
			public const string Invite_EndUserAppOrgId = "Invite_EndUserAppOrgId";
			public const string Invite_Greeting_Subject = "Invite_Greeting_Subject";
			public const string InviteErr_EmailInvalid = "InviteErr_EmailInvalid";
			public const string InviteErr_EmailIsEmpty = "InviteErr_EmailIsEmpty";
			public const string InviteErr_InviteIsNull = "InviteErr_InviteIsNull";
			public const string InviteErr_NameIsRequired = "InviteErr_NameIsRequired";
			public const string InviteUser_AlreadyPartOfOrg = "InviteUser_AlreadyPartOfOrg";
			public const string InviteUser_ClickHere = "InviteUser_ClickHere";
			public const string InviteUser_Description = "InviteUser_Description";
			public const string InviteUser_Greeting_Label = "InviteUser_Greeting_Label";
			public const string InviteUser_Greeting_Message = "InviteUser_Greeting_Message";
			public const string InviteUser_Help = "InviteUser_Help";
			public const string InviteUser_InvitedByEmail = "InviteUser_InvitedByEmail";
			public const string InviteUser_InvitedById = "InviteUser_InvitedById";
			public const string InviteUser_InvitedByName = "InviteUser_InvitedByName";
			public const string InviteUser_Name = "InviteUser_Name";
			public const string InviteUser_Status = "InviteUser_Status";
			public const string InviteUser_Status_Accepted = "InviteUser_Status_Accepted";
			public const string InviteUser_Status_Queued = "InviteUser_Status_Queued";
			public const string InviteUserVM_Description = "InviteUserVM_Description";
			public const string InviteUserVM_Help = "InviteUserVM_Help";
			public const string InviteUserVM_Title = "InviteUserVM_Title";
			public const string KioskLoginViewModel_Description = "KioskLoginViewModel_Description";
			public const string KioskLoginViewModel_Help = "KioskLoginViewModel_Help";
			public const string KioskLoginViewModel_Name = "KioskLoginViewModel_Name";
			public const string Location_Admin_Contact = "Location_Admin_Contact";
			public const string Location_City = "Location_City";
			public const string Location_Country = "Location_Country";
			public const string Location_GeoLocation = "Location_GeoLocation";
			public const string Location_LocationName = "Location_LocationName";
			public const string Location_RoomNumber = "Location_RoomNumber";
			public const string Location_State = "Location_State";
			public const string LocationDevice_Description = "LocationDevice_Description";
			public const string LocationDevice_Help = "LocationDevice_Help";
			public const string LocationDevice_Name = "LocationDevice_Name";
			public const string LocationDiagram_BaseLocation = "LocationDiagram_BaseLocation";
			public const string LocationDiagram_BaseLocation_Help = "LocationDiagram_BaseLocation_Help";
			public const string LocationDiagram_Groups = "LocationDiagram_Groups";
			public const string LocationDiagram_Height = "LocationDiagram_Height";
			public const string LocationDiagram_Layers = "LocationDiagram_Layers";
			public const string LocationDiagram_Left = "LocationDiagram_Left";
			public const string LocationDiagram_Shape = "LocationDiagram_Shape";
			public const string LocationDiagram_Shape_CustomerLocation = "LocationDiagram_Shape_CustomerLocation";
			public const string LocationDiagram_Shape_CustomerLocation_Help = "LocationDiagram_Shape_CustomerLocation_Help";
			public const string LocationDiagram_Shape_Help = "LocationDiagram_Shape_Help";
			public const string LocationDiagram_Shape_OrgLocation = "LocationDiagram_Shape_OrgLocation";
			public const string LocationDiagram_Shape_OrgLocation_Help = "LocationDiagram_Shape_OrgLocation_Help";
			public const string LocationDiagram_Shapes = "LocationDiagram_Shapes";
			public const string LocationDiagram_Title = "LocationDiagram_Title";
			public const string LocationDiagram_Top = "LocationDiagram_Top";
			public const string LocationDiagram_Width = "LocationDiagram_Width";
			public const string LocationDiagramLayer_Description = "LocationDiagramLayer_Description";
			public const string LocationDiagramLayer_Groups = "LocationDiagramLayer_Groups";
			public const string LocationDiagramLayer_Locked = "LocationDiagramLayer_Locked";
			public const string LocationDiagramLayer_Shapes = "LocationDiagramLayer_Shapes";
			public const string LocationDiagramLayer_Title = "LocationDiagramLayer_Title";
			public const string LocationDiagramLayer_Visible = "LocationDiagramLayer_Visible";
			public const string LocationDiagrams_Description = "LocationDiagrams_Description";
			public const string LocationDiagrams_Title = "LocationDiagrams_Title";
			public const string LocationDiagramShapeGroup_Description = "LocationDiagramShapeGroup_Description";
			public const string LocationDiagramShapeGroup_Help = "LocationDiagramShapeGroup_Help";
			public const string LocationDiagramShapeGroup_Shapes = "LocationDiagramShapeGroup_Shapes";
			public const string LocationDiagramShapeGroup_Title = "LocationDiagramShapeGroup_Title";
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
			public const string MagicLinkAttempt_Description = "MagicLinkAttempt_Description";
			public const string MagicLinkAttempt_Help = "MagicLinkAttempt_Help";
			public const string MagicLinkAttempt_Title = "MagicLinkAttempt_Title";
			public const string MagicLinkConsumeContext_Description = "MagicLinkConsumeContext_Description";
			public const string MagicLinkConsumeContext_Help = "MagicLinkConsumeContext_Help";
			public const string MagicLinkConsumeContext_Title = "MagicLinkConsumeContext_Title";
			public const string MagicLinkConsumeResponse_Description = "MagicLinkConsumeResponse_Description";
			public const string MagicLinkConsumeResponse_Help = "MagicLinkConsumeResponse_Help";
			public const string MagicLinkConsumeResponse_Title = "MagicLinkConsumeResponse_Title";
			public const string MagicLinkExchangeContext_Description = "MagicLinkExchangeContext_Description";
			public const string MagicLinkExchangeContext_Help = "MagicLinkExchangeContext_Help";
			public const string MagicLinkExchangeContext_Title = "MagicLinkExchangeContext_Title";
			public const string MagicLinkExchangeResponse_Description = "MagicLinkExchangeResponse_Description";
			public const string MagicLinkExchangeResponse_Help = "MagicLinkExchangeResponse_Help";
			public const string MagicLinkExchangeResponse_Title = "MagicLinkExchangeResponse_Title";
			public const string MagicLinkRequest_Description = "MagicLinkRequest_Description";
			public const string MagicLinkRequest_Help = "MagicLinkRequest_Help";
			public const string MagicLinkRequest_Title = "MagicLinkRequest_Title";
			public const string MagicLinkRequestContext_Description = "MagicLinkRequestContext_Description";
			public const string MagicLinkRequestContext_Help = "MagicLinkRequestContext_Help";
			public const string MagicLinkRequestContext_Title = "MagicLinkRequestContext_Title";
			public const string ManagedAsset_Description = "ManagedAsset_Description";
			public const string ManagedAsset_Help = "ManagedAsset_Help";
			public const string ManagedAsset_Name = "ManagedAsset_Name";
			public const string ManagedAssetSummary_Description = "ManagedAssetSummary_Description";
			public const string ManagedAssetSummary_Help = "ManagedAssetSummary_Help";
			public const string ManagedAssetSummary_Name = "ManagedAssetSummary_Name";
			public const string Menu_DoNotDisplay = "Menu_DoNotDisplay";
			public const string Menu_DoNotDisplay_Help = "Menu_DoNotDisplay_Help";
			public const string MobileOAuthPendingAuth_Description = "MobileOAuthPendingAuth_Description";
			public const string MobileOAuthPendingAuth_Help = "MobileOAuthPendingAuth_Help";
			public const string MobileOAuthPendingAuth_Name = "MobileOAuthPendingAuth_Name";
			public const string Module_AreaCategories = "Module_AreaCategories";
			public const string Module_CardIcon = "Module_CardIcon";
			public const string Module_CardSummary = "Module_CardSummary";
			public const string Module_CardTitle = "Module_CardTitle";
			public const string Module_Help = "Module_Help";
			public const string Module_HelpResources = "Module_HelpResources";
			public const string Module_IsExternalLink = "Module_IsExternalLink";
			public const string Module_IsForProductLine = "Module_IsForProductLine";
			public const string Module_IsLegacyNGX = "Module_IsLegacyNGX";
			public const string Module_Link = "Module_Link";
			public const string Module_OpenInNewTab = "Module_OpenInNewTab";
			public const string Module_RestrictByDefault = "Module_RestrictByDefault";
			public const string Module_RestrictByDefault_Help = "Module_RestrictByDefault_Help";
			public const string Module_RootPath = "Module_RootPath";
			public const string Module_RootPath_Help = "Module_RootPath_Help";
			public const string Module_SortOrder = "Module_SortOrder";
			public const string Module_Title = "Module_Title";
			public const string ModuleCategory_Admin = "ModuleCategory_Admin";
			public const string ModuleCategory_Admin_Summary = "ModuleCategory_Admin_Summary";
			public const string ModuleCategory_IoT = "ModuleCategory_IoT";
			public const string ModuleCategory_IoT_Summary = "ModuleCategory_IoT_Summary";
			public const string ModuleCategory_Support = "ModuleCategory_Support";
			public const string ModuleCategory_Support_Summary = "ModuleCategory_Support_Summary";
			public const string ModuleCategory_Tools = "ModuleCategory_Tools";
			public const string ModuleCategory_Tools_Summary = "ModuleCategory_Tools_Summary";
			public const string ModuleCateogry_EndUser = "ModuleCateogry_EndUser";
			public const string ModuleCateogry_EndUser_Summary = "ModuleCateogry_EndUser_Summary";
			public const string ModuleCateogry_Other = "ModuleCateogry_Other";
			public const string Modules_Title = "Modules_Title";
			public const string ModuleStatus_Alpha = "ModuleStatus_Alpha";
			public const string ModuleStatus_Beta = "ModuleStatus_Beta";
			public const string ModuleStatus_Development = "ModuleStatus_Development";
			public const string ModuleStatus_Live = "ModuleStatus_Live";
			public const string ModuleStatus_Preview = "ModuleStatus_Preview";
			public const string ModuleStatus_Retired = "ModuleStatus_Retired";
			public const string ModuleStatus_Select = "ModuleStatus_Select";
			public const string Month_April = "Month_April";
			public const string Month_August = "Month_August";
			public const string Month_December = "Month_December";
			public const string Month_February = "Month_February";
			public const string Month_January = "Month_January";
			public const string Month_July = "Month_July";
			public const string Month_June = "Month_June";
			public const string Month_March = "Month_March";
			public const string Month_May = "Month_May";
			public const string Month_November = "Month_November";
			public const string Month_October = "Month_October";
			public const string Month_Select = "Month_Select";
			public const string Month_September = "Month_September";
			public const string MostRecentlyUsed_Description = "MostRecentlyUsed_Description";
			public const string MostRecentlyUsed_Help = "MostRecentlyUsed_Help";
			public const string MostRecentlyUsed_Name = "MostRecentlyUsed_Name";
			public const string MostRecentlyUsedItem_Description = "MostRecentlyUsedItem_Description";
			public const string MostRecentlyUsedItem_Help = "MostRecentlyUsedItem_Help";
			public const string MostRecentlyUsedItem_Name = "MostRecentlyUsedItem_Name";
			public const string MostRecentlyUsedModule_Description = "MostRecentlyUsedModule_Description";
			public const string MostRecentlyUsedModule_Help = "MostRecentlyUsedModule_Help";
			public const string MostRecentlyUsedModule_Name = "MostRecentlyUsedModule_Name";
			public const string NewAppUserInboxItemItem_Description = "NewAppUserInboxItemItem_Description";
			public const string NewAppUserInboxItemItem_Help = "NewAppUserInboxItemItem_Help";
			public const string NewAppUserInboxItemItem_Name = "NewAppUserInboxItemItem_Name";
			public const string NotificationContact_Description = "NotificationContact_Description";
			public const string NotificationContact_Help = "NotificationContact_Help";
			public const string NotificationContact_Name = "NotificationContact_Name";
			public const string Organization = "Organization";
			public const string Organization_AccentColor = "Organization_AccentColor";
			public const string Organization_ApEmailAddress = "Organization_ApEmailAddress";
			public const string Organization_ApEmailAddress_Help = "Organization_ApEmailAddress_Help";
			public const string Organization_AppUrl = "Organization_AppUrl";
			public const string Organization_AppUrl_Help = "Organization_AppUrl_Help";
			public const string Organization_ArEmailAddress = "Organization_ArEmailAddress";
			public const string Organization_ArEmailAddress_Help = "Organization_ArEmailAddress_Help";
			public const string Organization_BillingLocation = "Organization_BillingLocation";
			public const string Organization_BillingLocation_Select = "Organization_BillingLocation_Select";
			public const string Organization_CantCreate = "Organization_CantCreate";
			public const string Organization_CreateGettingStartedData = "Organization_CreateGettingStartedData";
			public const string Organization_CreateGettingStartedData_Help = "Organization_CreateGettingStartedData_Help";
			public const string Organization_DefaultAgentContext = "Organization_DefaultAgentContext";
			public const string Organization_DefaultAgentContext_Help = "Organization_DefaultAgentContext_Help";
			public const string Organization_DefaultAgentContext_Select = "Organization_DefaultAgentContext_Select";
			public const string Organization_DefaultBusinessDevelopmentRep = "Organization_DefaultBusinessDevelopmentRep";
			public const string Organization_DefaultBusinessDevelopmentRep_Help = "Organization_DefaultBusinessDevelopmentRep_Help";
			public const string Organization_DefaultContributor = "Organization_DefaultContributor";
			public const string Organization_DefaultContributor_Help = "Organization_DefaultContributor_Help";
			public const string Organization_DefaultDemoInstance = "Organization_DefaultDemoInstance";
			public const string Organization_DefaultDevelopmentInstance = "Organization_DefaultDevelopmentInstance";
			public const string Organization_DefaultIndustry = "Organization_DefaultIndustry";
			public const string Organization_DefaultIndustry_Help = "Organization_DefaultIndustry_Help";
			public const string Organization_DefaultIndustry_Select = "Organization_DefaultIndustry_Select";
			public const string Organization_DefaultInstance = "Organization_DefaultInstance";
			public const string Organization_DefaultInstance_Select = "Organization_DefaultInstance_Select";
			public const string Organization_DefaultLandingPage = "Organization_DefaultLandingPage";
			public const string Organization_DefaultLandingPage_Select = "Organization_DefaultLandingPage_Select";
			public const string Organization_DefaultProjectAdminLead = "Organization_DefaultProjectAdminLead";
			public const string Organization_DefaultProjectAdminLead_Help = "Organization_DefaultProjectAdminLead_Help";
			public const string Organization_DefaultProjectLead = "Organization_DefaultProjectLead";
			public const string Organization_DefaultProjectLead_Help = "Organization_DefaultProjectLead_Help";
			public const string Organization_DefaultQAResource = "Organization_DefaultQAResource";
			public const string Organization_DefaultQAResource_Help = "Organization_DefaultQAResource_Help";
			public const string Organization_DefaultRepo = "Organization_DefaultRepo";
			public const string Organization_DefaultRepo_Select = "Organization_DefaultRepo_Select";
			public const string Organization_DefaultResource_Watermark = "Organization_DefaultResource_Watermark";
			public const string Organization_DefaultTeamsWebHook = "Organization_DefaultTeamsWebHook";
			public const string Organization_DefaultTeamsWebHook_Help = "Organization_DefaultTeamsWebHook_Help";
			public const string Organization_DefaultTestInstance = "Organization_DefaultTestInstance";
			public const string Organization_DefaultTheme = "Organization_DefaultTheme";
			public const string Organization_Description = "Organization_Description";
			public const string Organization_Details = "Organization_Details";
			public const string Organization_EmailConfirmBody = "Organization_EmailConfirmBody";
			public const string Organization_EmailConfirmBody_Help = "Organization_EmailConfirmBody_Help";
			public const string Organization_EmailConfirmSubject = "Organization_EmailConfirmSubject";
			public const string Organization_EmailConfirmSubject_Help = "Organization_EmailConfirmSubject_Help";
			public const string Organization_EndUserHomePage = "Organization_EndUserHomePage";
			public const string Organization_EndUserHomePage_Help = "Organization_EndUserHomePage_Help";
			public const string Organization_FacebookUrl = "Organization_FacebookUrl";
			public const string Organization_Help = "Organization_Help";
			public const string Organization_HeroBackground = "Organization_HeroBackground";
			public const string Organization_HeroTitle = "Organization_HeroTitle";
			public const string Organization_HomePage = "Organization_HomePage";
			public const string Organization_HomePage_Help = "Organization_HomePage_Help";
			public const string Organization_HrEmailAddress = "Organization_HrEmailAddress";
			public const string Organization_HrEmailAddress_Help = "Organization_HrEmailAddress_Help";
			public const string Organization_IsForProductLine = "Organization_IsForProductLine";
			public const string Organization_LandingPageHostName = "Organization_LandingPageHostName";
			public const string Organization_LandingPageHostName_Help = "Organization_LandingPageHostName_Help";
			public const string Organization_LinkedInUrl = "Organization_LinkedInUrl";
			public const string Organization_Location = "Organization_Location";
			public const string Organization_Location_Description = "Organization_Location_Description";
			public const string Organization_Location_Help = "Organization_Location_Help";
			public const string Organization_Location_Title = "Organization_Location_Title";
			public const string Organization_Locations = "Organization_Locations";
			public const string Organization_Locations_Title = "Organization_Locations_Title";
			public const string Organization_Logo_DarkColor = "Organization_Logo_DarkColor";
			public const string Organization_Logo_DarkColor_Help = "Organization_Logo_DarkColor_Help";
			public const string Organization_Logo_Light = "Organization_Logo_Light";
			public const string Organization_Logo_LightColor_Help = "Organization_Logo_LightColor_Help";
			public const string Organization_Name = "Organization_Name";
			public const string Organization_NamespaceInUse = "Organization_NamespaceInUse";
			public const string Organization_OrgStatuses_Active = "Organization_OrgStatuses_Active";
			public const string Organization_OrgStatuses_Deactivated = "Organization_OrgStatuses_Deactivated";
			public const string Organization_OrgStatuses_Spam = "Organization_OrgStatuses_Spam";
			public const string Organization_Owner = "Organization_Owner";
			public const string Organization_Primary_Location = "Organization_Primary_Location";
			public const string Organization_SalesContact = "Organization_SalesContact";
			public const string Organization_Status_Active = "Organization_Status_Active";
			public const string Organization_Status_Active_BehindPayments = "Organization_Status_Active_BehindPayments";
			public const string Organization_Status_Archived = "Organization_Status_Archived";
			public const string Organization_SubLocation = "Organization_SubLocation";
			public const string Organization_SubLocation_Details = "Organization_SubLocation_Details";
			public const string Organization_SubLocations = "Organization_SubLocations";
			public const string Organization_SupportEmailAddress = "Organization_SupportEmailAddress";
			public const string Organization_SupportEmailAddress_Help = "Organization_SupportEmailAddress_Help";
			public const string Organization_TagLine = "Organization_TagLine";
			public const string Organization_TestingIndustry = "Organization_TestingIndustry";
			public const string Organization_TestingIndustry_Help = "Organization_TestingIndustry_Help";
			public const string Organization_TestingIndustry_Select = "Organization_TestingIndustry_Select";
			public const string Organization_TestingIndustryNiche = "Organization_TestingIndustryNiche";
			public const string Organization_TestingIndustryNiche_Select = "Organization_TestingIndustryNiche_Select";
			public const string Organization_Title = "Organization_Title";
			public const string Organization_User_Description = "Organization_User_Description";
			public const string Organization_User_Help = "Organization_User_Help";
			public const string Organization_User_Title = "Organization_User_Title";
			public const string Organization_VimeoUrl = "Organization_VimeoUrl";
			public const string Organization_WebSite = "Organization_WebSite";
			public const string Organization_XUrl = "Organization_XUrl";
			public const string Organization_YouTubeUrl = "Organization_YouTubeUrl";
			public const string OrganizationDetailsVM_Description = "OrganizationDetailsVM_Description";
			public const string OrganizationDetailVM_Help = "OrganizationDetailVM_Help";
			public const string OrganizationDetailVM_Title = "OrganizationDetailVM_Title";
			public const string OrganizationLocation_NamespaceInUse = "OrganizationLocation_NamespaceInUse";
			public const string OrganizationNamespace_Help = "OrganizationNamespace_Help";
			public const string Organizations_Title = "Organizations_Title";
			public const string OrganizationUser_CouldntAdd = "OrganizationUser_CouldntAdd";
			public const string OrganizationUser_UserExists = "OrganizationUser_UserExists";
			public const string OrganizationUserRole_Description = "OrganizationUserRole_Description";
			public const string OrganizationUserRole_Help = "OrganizationUserRole_Help";
			public const string OrganizationUserRole_Title = "OrganizationUserRole_Title";
			public const string OrganizationVM_Description = "OrganizationVM_Description";
			public const string OrganizationVM_Help = "OrganizationVM_Help";
			public const string OrganizationVM_Title = "OrganizationVM_Title";
			public const string OrgLocation_AdminContact_Select = "OrgLocation_AdminContact_Select";
			public const string OrgLocation_DeviceRepository = "OrgLocation_DeviceRepository";
			public const string OrgLocation_DeviceRepository_Select = "OrgLocation_DeviceRepository_Select";
			public const string OrgLocation_Diagram = "OrgLocation_Diagram";
			public const string OrgLocation_Diagram_Select = "OrgLocation_Diagram_Select";
			public const string OrgLocation_DistributionList = "OrgLocation_DistributionList";
			public const string OrgLocation_DistributionList_Select = "OrgLocation_DistributionList_Select";
			public const string OrgLocation_GeoBoundingBox = "OrgLocation_GeoBoundingBox";
			public const string OrgLocation_GeoBoundingBox_Help = "OrgLocation_GeoBoundingBox_Help";
			public const string OrgLocation_PhoneNumber = "OrgLocation_PhoneNumber";
			public const string OrgLocation_PrimaryDevice = "OrgLocation_PrimaryDevice";
			public const string OrgLocation_PrimaryDevice_Select = "OrgLocation_PrimaryDevice_Select";
			public const string OrgLocation_TechnicalContact_Select = "OrgLocation_TechnicalContact_Select";
			public const string Page_Help = "Page_Help";
			public const string Page_IsForProductLine = "Page_IsForProductLine";
			public const string Page_Title = "Page_Title";
			public const string PasskeyAuthenticationCompleteRequest_Description = "PasskeyAuthenticationCompleteRequest_Description";
			public const string PasskeyAuthenticationCompleteRequest_Help = "PasskeyAuthenticationCompleteRequest_Help";
			public const string PasskeyAuthenticationCompleteRequest_Name = "PasskeyAuthenticationCompleteRequest_Name";
			public const string PasskeyAuthenticationCompleteRequestWire_Description = "PasskeyAuthenticationCompleteRequestWire_Description";
			public const string PasskeyAuthenticationCompleteRequestWire_Help = "PasskeyAuthenticationCompleteRequestWire_Help";
			public const string PasskeyAuthenticationCompleteRequestWire_Name = "PasskeyAuthenticationCompleteRequestWire_Name";
			public const string PasskeyBeginOptionsResponse_Description = "PasskeyBeginOptionsResponse_Description";
			public const string PasskeyBeginOptionsResponse_Help = "PasskeyBeginOptionsResponse_Help";
			public const string PasskeyBeginOptionsResponse_Name = "PasskeyBeginOptionsResponse_Name";
			public const string PasskeyChallenge_Description = "PasskeyChallenge_Description";
			public const string PasskeyChallenge_Help = "PasskeyChallenge_Help";
			public const string PasskeyChallenge_Name = "PasskeyChallenge_Name";
			public const string PasskeyChallengePacket_Description = "PasskeyChallengePacket_Description";
			public const string PasskeyChallengePacket_Help = "PasskeyChallengePacket_Help";
			public const string PasskeyChallengePacket_Name = "PasskeyChallengePacket_Name";
			public const string PasskeyCredential_Description = "PasskeyCredential_Description";
			public const string PasskeyCredential_Help = "PasskeyCredential_Help";
			public const string PasskeyCredential_Name = "PasskeyCredential_Name";
			public const string PasskeyCredentialIndex_Description = "PasskeyCredentialIndex_Description";
			public const string PasskeyCredentialIndex_Help = "PasskeyCredentialIndex_Help";
			public const string PasskeyCredentialIndex_Name = "PasskeyCredentialIndex_Name";
			public const string PasskeyCredentialSummary_Description = "PasskeyCredentialSummary_Description";
			public const string PasskeyCredentialSummary_Help = "PasskeyCredentialSummary_Help";
			public const string PasskeyCredentialSummary_Name = "PasskeyCredentialSummary_Name";
			public const string PasskeyRegistrationCompleteRequest_Description = "PasskeyRegistrationCompleteRequest_Description";
			public const string PasskeyRegistrationCompleteRequest_Help = "PasskeyRegistrationCompleteRequest_Help";
			public const string PasskeyRegistrationCompleteRequest_Name = "PasskeyRegistrationCompleteRequest_Name";
			public const string PasskeyRegistrationCompleteRequestWire_Description = "PasskeyRegistrationCompleteRequestWire_Description";
			public const string PasskeyRegistrationCompleteRequestWire_Help = "PasskeyRegistrationCompleteRequestWire_Help";
			public const string PasskeyRegistrationCompleteRequestWire_Name = "PasskeyRegistrationCompleteRequestWire_Name";
			public const string PasskeySignInResult_Description = "PasskeySignInResult_Description";
			public const string PasskeySignInResult_Help = "PasskeySignInResult_Help";
			public const string PasskeySignInResult_Name = "PasskeySignInResult_Name";
			public const string PaymentAccounts_Description = "PaymentAccounts_Description";
			public const string PaymentAccounts_Help = "PaymentAccounts_Help";
			public const string PaymentAccounts_Name = "PaymentAccounts_Name";
			public const string PendingIdentity_Description = "PendingIdentity_Description";
			public const string PendingIdentity_Help = "PendingIdentity_Help";
			public const string PendingIdentity_Name = "PendingIdentity_Name";
			public const string PushNotificationChannel_Description = "PushNotificationChannel_Description";
			public const string PushNotificationChannel_Help = "PushNotificationChannel_Help";
			public const string PushNotificationChannel_Name = "PushNotificationChannel_Name";
			public const string RegErr_ErrorSendingEmail = "RegErr_ErrorSendingEmail";
			public const string RegErr_ErrorSendingPhoneNumber = "RegErr_ErrorSendingPhoneNumber";
			public const string RegErr_InvalidEmailAddress = "RegErr_InvalidEmailAddress";
			public const string RegErr_MissingFirstName = "RegErr_MissingFirstName";
			public const string RegErr_MissingLastName = "RegErr_MissingLastName";
			public const string RegErr_MissingPassword = "RegErr_MissingPassword";
			public const string RegErr_MissingPhoneNumber = "RegErr_MissingPhoneNumber";
			public const string RegErr_UserAlreadyExists = "RegErr_UserAlreadyExists";
			public const string RegisterUser_Description = "RegisterUser_Description";
			public const string RegisterUser_Help = "RegisterUser_Help";
			public const string RegisterUser_Name = "RegisterUser_Name";
			public const string RegisterUserExists_3rdParty = "RegisterUserExists_3rdParty";
			public const string RegisterVM_Description = "RegisterVM_Description";
			public const string RegisterVM_Help = "RegisterVM_Help";
			public const string RegisterVM_Title = "RegisterVM_Title";
			public const string RenamePasskeyRequest_Description = "RenamePasskeyRequest_Description";
			public const string RenamePasskeyRequest_Help = "RenamePasskeyRequest_Help";
			public const string RenamePasskeyRequest_Name = "RenamePasskeyRequest_Name";
			public const string ResetPassword_Description = "ResetPassword_Description";
			public const string ResetPassword_Help = "ResetPassword_Help";
			public const string ResetPassword_Name = "ResetPassword_Name";
			public const string ResetPassword_Title = "ResetPassword_Title";
			public const string Role_Description = "Role_Description";
			public const string Role_Help = "Role_Help";
			public const string Role_IsSystemRole = "Role_IsSystemRole";
			public const string Role_Title = "Role_Title";
			public const string RoleAccess_Description = "RoleAccess_Description";
			public const string RoleAccess_Help = "RoleAccess_Help";
			public const string RoleAccess_Name = "RoleAccess_Name";
			public const string RoleAccessDTO_Description = "RoleAccessDTO_Description";
			public const string RoleAccessDTO_Help = "RoleAccessDTO_Help";
			public const string RoleAccessDTO_Name = "RoleAccessDTO_Name";
			public const string ScheduledDowntime_AllDay = "ScheduledDowntime_AllDay";
			public const string ScheduledDowntime_Day = "ScheduledDowntime_Day";
			public const string ScheduledDowntime_DayOfWeek = "ScheduledDowntime_DayOfWeek";
			public const string ScheduledDowntime_DayOfWeek_Select = "ScheduledDowntime_DayOfWeek_Select";
			public const string ScheduledDowntime_Description = "ScheduledDowntime_Description";
			public const string ScheduledDowntime_DowntimeType = "ScheduledDowntime_DowntimeType";
			public const string ScheduledDowntime_Help = "ScheduledDowntime_Help";
			public const string ScheduledDowntime_Month = "ScheduledDowntime_Month";
			public const string ScheduledDowntime_Periods = "ScheduledDowntime_Periods";
			public const string ScheduledDowntime_ScheduleType = "ScheduledDowntime_ScheduleType";
			public const string ScheduledDowntime_ScheduleType_DayInMonth = "ScheduledDowntime_ScheduleType_DayInMonth";
			public const string ScheduledDowntime_ScheduleType_DayOfWeek = "ScheduledDowntime_ScheduleType_DayOfWeek";
			public const string ScheduledDowntime_ScheduleType_DayOfWeekOfWeekInMonth = "ScheduledDowntime_ScheduleType_DayOfWeekOfWeekInMonth";
			public const string ScheduledDowntime_ScheduleType_Select = "ScheduledDowntime_ScheduleType_Select";
			public const string ScheduledDowntime_ScheduleType_SpecificDate = "ScheduledDowntime_ScheduleType_SpecificDate";
			public const string ScheduledDowntime_Title = "ScheduledDowntime_Title";
			public const string ScheduledDowntime_Type = "ScheduledDowntime_Type";
			public const string ScheduledDowntime_Year = "ScheduledDowntime_Year";
			public const string ScheduledDowntimePeriod_Description = "ScheduledDowntimePeriod_Description";
			public const string ScheduledDowntimePeriod_End = "ScheduledDowntimePeriod_End";
			public const string ScheduledDowntimePeriod_End_Help = "ScheduledDowntimePeriod_End_Help";
			public const string ScheduledDowntimePeriod_EndOfDay = "ScheduledDowntimePeriod_EndOfDay";
			public const string ScheduledDowntimePeriod_EndOfDay_Help = "ScheduledDowntimePeriod_EndOfDay_Help";
			public const string ScheduledDowntimePeriod_Help = "ScheduledDowntimePeriod_Help";
			public const string ScheduledDowntimePeriod_InvalidTimeFormat = "ScheduledDowntimePeriod_InvalidTimeFormat";
			public const string ScheduledDowntimePeriod_Start = "ScheduledDowntimePeriod_Start";
			public const string ScheduledDowntimePeriod_Start_Help = "ScheduledDowntimePeriod_Start_Help";
			public const string ScheduledDowntimePeriod_StartOfDay = "ScheduledDowntimePeriod_StartOfDay";
			public const string ScheduledDowntimePeriod_StartOfDay_Help = "ScheduledDowntimePeriod_StartOfDay_Help";
			public const string ScheduledDowntimePeriod_TItle = "ScheduledDowntimePeriod_TItle";
			public const string ScheduledDowntimes_Title = "ScheduledDowntimes_Title";
			public const string ScheduleType_WeekDay = "ScheduleType_WeekDay";
			public const string ScheduleType_WeekEnd = "ScheduleType_WeekEnd";
			public const string ScheudledDowntime_Week = "ScheudledDowntime_Week";
			public const string SecureLink_Description = "SecureLink_Description";
			public const string SecureLink_Help = "SecureLink_Help";
			public const string SecureLink_Name = "SecureLink_Name";
			public const string SendCodeVM_Description = "SendCodeVM_Description";
			public const string SendCodeVM_Help = "SendCodeVM_Help";
			public const string SendCodeVM_Title = "SendCodeVM_Title";
			public const string SendResetPasswordLink_Description = "SendResetPasswordLink_Description";
			public const string SendResetPasswordLink_Help = "SendResetPasswordLink_Help";
			public const string SendResetPasswordLink_Name = "SendResetPasswordLink_Name";
			public const string SentEmail_Description = "SentEmail_Description";
			public const string SentEmail_Title = "SentEmail_Title";
			public const string SentEmails_Title = "SentEmails_Title";
			public const string SetCondition_DontCare = "SetCondition_DontCare";
			public const string SetCondition_NotSet = "SetCondition_NotSet";
			public const string SetCondition_Set = "SetCondition_Set";
			public const string SetPasswordVM_Description = "SetPasswordVM_Description";
			public const string SetPasswordVM_Help = "SetPasswordVM_Help";
			public const string SetPasswordVM_Title = "SetPasswordVM_Title";
			public const string Shape_Fill = "Shape_Fill";
			public const string Shape_FlipX = "Shape_FlipX";
			public const string Shape_FlipY = "Shape_FlipY";
			public const string Shape_FontSize = "Shape_FontSize";
			public const string Shape_Locked = "Shape_Locked";
			public const string Shape_Rotation = "Shape_Rotation";
			public const string Shape_Scale = "Shape_Scale";
			public const string Shape_Stroke = "Shape_Stroke";
			public const string Shape_TextOffsetX = "Shape_TextOffsetX";
			public const string Shape_TextOffsetY = "Shape_TextOffsetY";
			public const string Shape_TextRotation = "Shape_TextRotation";
			public const string ShapeType = "ShapeType";
			public const string ShapeType_Building = "ShapeType_Building";
			public const string ShapeType_Circle = "ShapeType_Circle";
			public const string ShapeType_Closet = "ShapeType_Closet";
			public const string ShapeType_Door = "ShapeType_Door";
			public const string ShapeType_ExternalEntrance = "ShapeType_ExternalEntrance";
			public const string ShapeType_ParkingLot = "ShapeType_ParkingLot";
			public const string ShapeType_Polygon = "ShapeType_Polygon";
			public const string ShapeType_Room = "ShapeType_Room";
			public const string ShapeType_Select = "ShapeType_Select";
			public const string ShapeType_Window = "ShapeType_Window";
			public const string SingleUseToken_Description = "SingleUseToken_Description";
			public const string SingleUseToken_Help = "SingleUseToken_Help";
			public const string SingleUseToken_Name = "SingleUseToken_Name";
			public const string SMS_CouldNotVerify = "SMS_CouldNotVerify";
			public const string SMS_Verification_Body = "SMS_Verification_Body";
			public const string Subscription_Description = "Subscription_Description";
			public const string Subscription_Help = "Subscription_Help";
			public const string Subscription_PaymentMethod = "Subscription_PaymentMethod";
			public const string Subscription_PaymentMethod_Date = "Subscription_PaymentMethod_Date";
			public const string Subscription_PaymentMethod_Expires = "Subscription_PaymentMethod_Expires";
			public const string Subscription_PaymentMethod_Help = "Subscription_PaymentMethod_Help";
			public const string Subscription_PaymentMethod_Status = "Subscription_PaymentMethod_Status";
			public const string Subscription_Status = "Subscription_Status";
			public const string Subscription_Title = "Subscription_Title";
			public const string Subscriptions_Title = "Subscriptions_Title";
			public const string Team_Description = "Team_Description";
			public const string Team_Help = "Team_Help";
			public const string Team_Title = "Team_Title";
			public const string TeamUser_Description = "TeamUser_Description";
			public const string TeamUser_Help = "TeamUser_Help";
			public const string TeamUser_Name = "TeamUser_Name";
			public const string TeamUserSummary_Description = "TeamUserSummary_Description";
			public const string TeamUserSummary_Help = "TeamUserSummary_Help";
			public const string TeamUserSummary_Name = "TeamUserSummary_Name";
			public const string Technical_Contact = "Technical_Contact";
			public const string TestRunArtifact_Description = "TestRunArtifact_Description";
			public const string TestRunArtifact_Help = "TestRunArtifact_Help";
			public const string TestRunArtifact_Name = "TestRunArtifact_Name";
			public const string TestRunVerification_Description = "TestRunVerification_Description";
			public const string TestRunVerification_Help = "TestRunVerification_Help";
			public const string TestRunVerification_Name = "TestRunVerification_Name";
			public const string TestUserCredentials_Description = "TestUserCredentials_Description";
			public const string TestUserCredentials_Help = "TestUserCredentials_Help";
			public const string TestUserCredentials_Name = "TestUserCredentials_Name";
			public const string TokenAuthOptions_Description = "TokenAuthOptions_Description";
			public const string TokenAuthOptions_Help = "TokenAuthOptions_Help";
			public const string TokenAuthOptions_Name = "TokenAuthOptions_Name";
			public const string TwitterAccessToken_Description = "TwitterAccessToken_Description";
			public const string TwitterAccessToken_Help = "TwitterAccessToken_Help";
			public const string TwitterAccessToken_Name = "TwitterAccessToken_Name";
			public const string TwitterError_Description = "TwitterError_Description";
			public const string TwitterError_Help = "TwitterError_Help";
			public const string TwitterError_Name = "TwitterError_Name";
			public const string TwitterErrorResponse_Description = "TwitterErrorResponse_Description";
			public const string TwitterErrorResponse_Help = "TwitterErrorResponse_Help";
			public const string TwitterErrorResponse_Name = "TwitterErrorResponse_Name";
			public const string TwitterRequestToken_Description = "TwitterRequestToken_Description";
			public const string TwitterRequestToken_Help = "TwitterRequestToken_Help";
			public const string TwitterRequestToken_Name = "TwitterRequestToken_Name";
			public const string UiCategory_Description = "UiCategory_Description";
			public const string UiCategory_Icon = "UiCategory_Icon";
			public const string UiCategory_Icon_Select = "UiCategory_Icon_Select";
			public const string UiCateogry_Title = "UiCateogry_Title";
			public const string UpdateLocationVM_Help = "UpdateLocationVM_Help";
			public const string UpdateLocationVM_Title = "UpdateLocationVM_Title";
			public const string UpdateLocatoinVM_Description = "UpdateLocatoinVM_Description";
			public const string UpdateOrganizationVM_Description = "UpdateOrganizationVM_Description";
			public const string UpdateOrganizationVM_Help = "UpdateOrganizationVM_Help";
			public const string UpdateOrganizationVM_Title = "UpdateOrganizationVM_Title";
			public const string User = "User";
			public const string UserAccess_Description = "UserAccess_Description";
			public const string UserAccess_Help = "UserAccess_Help";
			public const string UserAccess_Name = "UserAccess_Name";
			public const string UserFavorite_Description = "UserFavorite_Description";
			public const string UserFavorite_Help = "UserFavorite_Help";
			public const string UserFavorite_Name = "UserFavorite_Name";
			public const string UserFavorites_Description = "UserFavorites_Description";
			public const string UserFavorites_Help = "UserFavorites_Help";
			public const string UserFavorites_Name = "UserFavorites_Name";
			public const string UserLoginResponse_Description = "UserLoginResponse_Description";
			public const string UserLoginResponse_Help = "UserLoginResponse_Help";
			public const string UserLoginResponse_Name = "UserLoginResponse_Name";
			public const string UserRole_Description = "UserRole_Description";
			public const string UserRole_Help = "UserRole_Help";
			public const string UserRole_Name = "UserRole_Name";
			public const string UserRoleDTO_Description = "UserRoleDTO_Description";
			public const string UserRoleDTO_Help = "UserRoleDTO_Help";
			public const string UserRoleDTO_Name = "UserRoleDTO_Name";
			public const string VerfiyPhoneNumber_Description = "VerfiyPhoneNumber_Description";
			public const string VerfiyPhoneNumber_Help = "VerfiyPhoneNumber_Help";
			public const string VerfiyPhoneNumber_Name = "VerfiyPhoneNumber_Name";
			public const string VerifyCodeViewModel_Code = "VerifyCodeViewModel_Code";
			public const string VerifyCodeViewModel_Provider = "VerifyCodeViewModel_Provider";
			public const string VerifyCodeViewModel_RememberBrowser = "VerifyCodeViewModel_RememberBrowser";
			public const string VerifyCodeViewModel_RememberMe = "VerifyCodeViewModel_RememberMe";
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
			public const string VerticalAlign_Bottom = "VerticalAlign_Bottom";
			public const string VerticalAlign_Middle = "VerticalAlign_Middle";
			public const string VerticalAlign_Top = "VerticalAlign_Top";
			public const string WebAuthnAssertionResponseWire_Description = "WebAuthnAssertionResponseWire_Description";
			public const string WebAuthnAssertionResponseWire_Help = "WebAuthnAssertionResponseWire_Help";
			public const string WebAuthnAssertionResponseWire_Name = "WebAuthnAssertionResponseWire_Name";
			public const string WebAuthnAssertionWire_Description = "WebAuthnAssertionWire_Description";
			public const string WebAuthnAssertionWire_Help = "WebAuthnAssertionWire_Help";
			public const string WebAuthnAssertionWire_Name = "WebAuthnAssertionWire_Name";
			public const string WebAuthnAttestationResponseWire_Description = "WebAuthnAttestationResponseWire_Description";
			public const string WebAuthnAttestationResponseWire_Help = "WebAuthnAttestationResponseWire_Help";
			public const string WebAuthnAttestationResponseWire_Name = "WebAuthnAttestationResponseWire_Name";
			public const string WebAuthnAttestationWire_Description = "WebAuthnAttestationWire_Description";
			public const string WebAuthnAttestationWire_Help = "WebAuthnAttestationWire_Help";
			public const string WebAuthnAttestationWire_Name = "WebAuthnAttestationWire_Name";
		}
	}
}

