// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 697774c362619dad10f8342c5f29b41fa0d313d791d062dc90b37664d26fab5e
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Account
{
    public interface IMostRecentlyUsedRepo
    {
        Task AddMostRecentlyUsedAsync(MostRecentlyUsed mostRecentlyUsed);
        Task<MostRecentlyUsed> GetMostRecentlyUsedAsync(string orgId, string userId);
        Task DeleteMostRecentlyUsedAsync(string orgId, string userId);
        Task UpdateMostRecentlyUsedAsync(MostRecentlyUsed mostRecentlyUsed);
    }
}
