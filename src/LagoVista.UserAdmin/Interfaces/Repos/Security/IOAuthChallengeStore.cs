using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public interface IOAuthChallengeStore
    {
        Task<InvokeResult<MobileOAuthPendingAuth>> ConsumeAsync(string codeId);
        Task CreateAsync(MobileOAuthPendingAuth packet);
        Task<InvokeResult<MobileOAuthPendingAuth>> GetAsync(string codeId);
    }
}