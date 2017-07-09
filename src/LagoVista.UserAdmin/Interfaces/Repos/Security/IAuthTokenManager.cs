using LagoVista.Core.Authentication.Models;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IAuthTokenManager
    {
        Task<InvokeResult<AuthResponse>> AccessTokenGrantAsync(AuthRequest authRequest);

        Task<InvokeResult<AuthResponse>> RefreshTokenGrantAsync(AuthRequest authRequest, EntityHeader org, EntityHeader user);
    }
}
