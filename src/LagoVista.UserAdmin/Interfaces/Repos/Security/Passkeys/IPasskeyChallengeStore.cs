using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Security.Passkeys;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security.Passkeys
{
    public interface IPasskeyChallengeStore
    {
        Task<InvokeResult<PasskeyChallengePacket>> CreateAsync(PasskeyChallengePacket packet);
        Task<InvokeResult<PasskeyChallengePacket>> GetAsync(string challengeId);
        Task<InvokeResult<PasskeyChallengePacket>> ConsumeAsync(string challengeId);
    }
}
