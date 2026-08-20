using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Authentication
{
    public interface IMfaChallengeFlowService
    {
        Task<InvokeResult<MfaChallenge>> ValidateAsync(string challengeId, string provider, string email = null);
        Task<InvokeResult<MfaChallenge>> ConsumeAsync(string challengeId, string provider, string email = null);
    }
}
