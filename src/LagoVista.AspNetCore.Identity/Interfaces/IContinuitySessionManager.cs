using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Interfaces
{
    public interface IContinuitySessionManager
    {
        Task<InvokeResult<ContinuitySessionResponse>> ResolveAsync(string continuityToken, string appUserId = null);
        Task<InvokeResult<ContinuitySessionResponse>> CreatePromotedProvisionalSessionAsync(AnonymousVisitorPromotionResponse promotion);
        Task<InvokeResult<ContinuitySessionResponse>> GetClaimedSessionAsync(string provisionalEnvironmentId, string appUserId, bool wasRestored = true);
        Task<InvokeResult<ContinuitySessionResponse>> ResetAsync(string actorId, string identityStage, string continuityToken);
    }
}
