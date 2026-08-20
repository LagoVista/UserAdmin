using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IMfaChallengeStore
    {
        Task<InvokeResult<MfaChallenge>> CreateAsync(MfaChallenge challenge);
        Task<InvokeResult<MfaChallenge>> GetAsync(string challengeId);
        Task<InvokeResult<MfaChallenge>> ConsumeAsync(string challengeId);
    }
}
