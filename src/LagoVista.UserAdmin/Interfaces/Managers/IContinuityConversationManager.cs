using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IContinuityConversationManager
    {
        Task<InvokeResult<ContinuityConversationResponse>> GetAsync(string actorId);
        Task<InvokeResult<ContinuityConversationResponse>> SendAsync(string actorId, string identityStage, ContinuityConversationMessageRequest request);
        Task<InvokeResult> ClearAsync(string actorId);
    }
}
