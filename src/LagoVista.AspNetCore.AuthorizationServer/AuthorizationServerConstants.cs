namespace LagoVista.AspNetCore.AuthorizationServer
{
    public static class AuthorizationServerConstants
    {
        public const string AuthorizationEndpoint = "/connect/authorize";
        public const string TokenEndpoint = "/connect/token";
        public const string UserInfoEndpoint = "/connect/userinfo";
        public const string EndSessionEndpoint = "/connect/logout";
        public const string RevocationEndpoint = "/connect/revoke";

        public const string GrantTypeAuthorizationCode = "authorization_code";
        public const string GrantTypeRefreshToken = "refresh_token";

        public const string ScopeOpenId = "openid";
        public const string ScopeOfflineAccess = "offline_access";
        public const string ScopeTeamRole = "team_role";

        public const string ClaimTeamRole = "team_role";
        public const string TeamRoleOwner = "Owner";
    }
}
