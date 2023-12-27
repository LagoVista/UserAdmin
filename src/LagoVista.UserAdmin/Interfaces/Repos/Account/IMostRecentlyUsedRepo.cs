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
