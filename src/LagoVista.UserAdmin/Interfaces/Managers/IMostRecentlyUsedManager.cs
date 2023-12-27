using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IMostRecentlyUsedManager
    {
        Task<MostRecentlyUsed> AddMostRecentlyUsedAsync(MostRecentlyUsedItem mostRecentlyUsedItem, EntityHeader org, EntityHeader user);
        Task<MostRecentlyUsed> GetMostRecentlyUsedAsync(EntityHeader org,EntityHeader user);
        Task<InvokeResult> ClearMostRecentlyUsedAsync(EntityHeader org, EntityHeader user); 
    }
}
