using LagoVista.UserAdmin.Models.Auth;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IAuthenticationResponseResolver
    {
        AuthenticationResponseState ResolveState(AuthenticationResolutionContext context);
        UserLoginResponse Resolve(AuthenticationResolutionContext context);
        UserLoginResponse Apply(UserLoginResponse response, AuthenticationResolutionContext context);
    }
}
