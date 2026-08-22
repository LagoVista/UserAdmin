using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public static class OidcTeamRoleProjection
    {
        public static string GetTeamRole(IEnumerable<string> scopes, string isSystemAdmin)
        {
            if (scopes == null || !scopes.Contains(AuthorizationServerConstants.ScopeTeamRole, StringComparer.Ordinal))
                return null;

            return Boolean.TryParse(isSystemAdmin, out var isAdmin) && isAdmin
                ? AuthorizationServerConstants.TeamRoleOwner
                : null;
        }
    }
}
