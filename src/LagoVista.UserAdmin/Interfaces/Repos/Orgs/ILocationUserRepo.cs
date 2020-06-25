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
