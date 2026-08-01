using System.Threading.Tasks;

namespace LagoVista.AspNetCore.AuthorizationServer
{
    public interface IOAuthClientPolicyResolver
    {
        Task<OAuthClientPolicy> GetByClientIdAsync(string clientId);
    }
}
