using LagoVista.UserAdmin.Models.Auth;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IAuthenticationResponseResolver
    {
        AuthenticationResponseState ResolveState(AuthenticationResolutionContext context);
        AuthenticationResponse Resolve(AuthenticationResolutionContext context);
        AuthenticationResponse Apply(AuthenticationResponse response, AuthenticationResolutionContext context);
    }
}
