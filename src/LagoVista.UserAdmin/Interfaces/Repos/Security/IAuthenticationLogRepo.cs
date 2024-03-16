using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public enum AuthLogTypes
    {
        PasswordAuthSuccess,
        PaswwordAuthFailed,
        CreateEmailUser,
        ManualOrgCreate,
        OAuthRedirect,
        OAuthCallback,
        OAuthError,
        OAuthCreateOrg,
        OAuthCreateUser,
        OAuthAppendUserLogin,
        InviteUser,
        RegisterUser,
        AcceptInvite
    }

    public interface IAuthenticationLogRepo
    {
        Task WriteAsync(string userName, string userId, string orgId, string orgName, AuthLogTypes type, string extras, string errors);
    }
}
