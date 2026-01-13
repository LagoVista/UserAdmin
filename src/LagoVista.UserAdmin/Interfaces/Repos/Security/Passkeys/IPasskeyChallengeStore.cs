using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys
{
    public interface IPasskeyChallengeStore
    {
        Task<InvokeResult<PasskeyChallenge>> CreateAsync(PasskeyChallenge challenge);
        Task<InvokeResult<PasskeyChallenge>> GetAsync(string challengeId);
        Task<InvokeResult<PasskeyChallenge>> ConsumeAsync(string challengeId);
    }
}
