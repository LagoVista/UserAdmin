using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Users
{
    public interface IProvisionalEnvironmentArchiveAccountingService
    {
        Task<ProvisionalEnvironmentArchiveAccountingResult> EnsureRollupAsync(ProvisionalEnvironmentArchiveAccountingRequest request);
    }
}
