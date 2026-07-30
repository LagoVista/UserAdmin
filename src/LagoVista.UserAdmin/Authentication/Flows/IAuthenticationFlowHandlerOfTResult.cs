using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public interface IAuthenticationFlowHandler<TRequest, TResult>
    {
        Task<AuthenticationFlowResult<TResult>> HandleAsync(TRequest request);
    }
}
