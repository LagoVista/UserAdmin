using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IPendingIdentityResolutionService
    {
        Task<InvokeResult<AppUser>> ResolveOAuthAsync(string pendingIdentityId, RegisterUser registrationContext);
    }
}
