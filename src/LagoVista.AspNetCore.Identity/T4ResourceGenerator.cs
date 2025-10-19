// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e60ee861b4efd3def289aefbc398454d20ea0dd01d30462b2b58574027a9a013
// IndexVersion: 0
// --- END CODE INDEX META ---
/*6/14/2025 5:57:53 PM*/
using System.Globalization;
using System.Reflection;

//Resources:IdentityResources:AuthErr_InvalidCredentials
namespace LagoVista.AspNetCore.Identity.Resources
{
	public class IdentityResources
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("LagoVista.AspNetCore.Identity.Resources.IdentityResources", typeof(IdentityResources).GetTypeInfo().Assembly);
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
		
		public static string AuthErr_InvalidCredentials { get { return GetResourceString("AuthErr_InvalidCredentials"); } }
//Resources:IdentityResources:AuthErr_InvalidRefreshToken

		public static string AuthErr_InvalidRefreshToken { get { return GetResourceString("AuthErr_InvalidRefreshToken"); } }
//Resources:IdentityResources:AuthErr_RefreshTokenExpired

		public static string AuthErr_RefreshTokenExpired { get { return GetResourceString("AuthErr_RefreshTokenExpired"); } }
//Resources:IdentityResources:EmailSender_Description

		public static string EmailSender_Description { get { return GetResourceString("EmailSender_Description"); } }
//Resources:IdentityResources:EmailSender_TItle

		public static string EmailSender_TItle { get { return GetResourceString("EmailSender_TItle"); } }
//Resources:IdentityResources:Login_Email

		public static string Login_Email { get { return GetResourceString("Login_Email"); } }
//Resources:IdentityResources:Login_ForgotPassword

		public static string Login_ForgotPassword { get { return GetResourceString("Login_ForgotPassword"); } }
//Resources:IdentityResources:Login_Password

		public static string Login_Password { get { return GetResourceString("Login_Password"); } }
//Resources:IdentityResources:Login_RememberMe

		public static string Login_RememberMe { get { return GetResourceString("Login_RememberMe"); } }
//Resources:IdentityResources:Login_RememberMe_Help

		public static string Login_RememberMe_Help { get { return GetResourceString("Login_RememberMe_Help"); } }
//Resources:IdentityResources:Login_UserName

		public static string Login_UserName { get { return GetResourceString("Login_UserName"); } }

		public static class Names
		{
			public const string AuthErr_InvalidCredentials = "AuthErr_InvalidCredentials";
			public const string AuthErr_InvalidRefreshToken = "AuthErr_InvalidRefreshToken";
			public const string AuthErr_RefreshTokenExpired = "AuthErr_RefreshTokenExpired";
			public const string EmailSender_Description = "EmailSender_Description";
			public const string EmailSender_TItle = "EmailSender_TItle";
			public const string Login_Email = "Login_Email";
			public const string Login_ForgotPassword = "Login_ForgotPassword";
			public const string Login_Password = "Login_Password";
			public const string Login_RememberMe = "Login_RememberMe";
			public const string Login_RememberMe_Help = "Login_RememberMe_Help";
			public const string Login_UserName = "Login_UserName";
		}
	}
}

