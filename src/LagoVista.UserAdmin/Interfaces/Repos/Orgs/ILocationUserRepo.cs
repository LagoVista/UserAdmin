// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ce74da059bdd5a810773fe44c0b9cdd9b6c7ed7d396cb8fa7d78aa157e87157b
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface ILocationUserRepo
    {
        Task AddUserToLocationAsync(LocationUser locationUser);
        Task RemoveUserFromLocationAsync(string locationId, string userId, EntityHeader removedBy);
        Task<IEnumerable<LocationUser>> GetUsersForLocationAsync(string locationId);
        Task<IEnumerable<LocationUser>> GetLocationsForUserAsync(string userId);        
    }
}
