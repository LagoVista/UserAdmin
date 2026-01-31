using Security.Models;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Account
{
    public interface IPendingIdentityRepo
    {
        Task AddPendingIdentityAsync(PendingIdentity identity);
        Task<PendingIdentity> GetPendingIdentityAsync(string pendingIdentityId);
        Task UpdatePendingIndentiy(PendingIdentity identity);
        Task DeletePendingIdentityAsync(string pendingIdentityId);
    }
}
