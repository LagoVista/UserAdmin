using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IContinuitySessionManager
    {
        Task<InvokeResult<ContinuitySessionResponse>> ResolveAsync(string continuityToken);
        Task<InvokeResult<ContinuitySessionResponse>> ResetAsync(string actorId, string identityStage, string continuityToken);
    }
}
