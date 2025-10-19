// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7cb24426994e5eed82bc3fe6f62d4be67a8e42149335a211b9e77334e087f554
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IMostRecentlyUsedManager
    {
        Task<InvokeResult<MostRecentlyUsed>> AddMostRecentlyUsedAsync(MostRecentlyUsedItem mostRecentlyUsedItem, EntityHeader org, EntityHeader user);
        Task<InvokeResult<MostRecentlyUsed>> GetMostRecentlyUsedAsync(EntityHeader org,EntityHeader user);
        Task<InvokeResult> ClearMostRecentlyUsedAsync(EntityHeader org, EntityHeader user); 
    }
}
