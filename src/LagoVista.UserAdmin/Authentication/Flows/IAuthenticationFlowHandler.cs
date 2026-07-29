using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public interface IAuthenticationFlowHandler<TRequest>
    {
        Task<AuthenticationFlowResult> HandleAsync(TRequest request);
    }
}
