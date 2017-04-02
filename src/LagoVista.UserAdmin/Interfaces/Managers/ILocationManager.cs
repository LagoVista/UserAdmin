using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface ILocationManager
    {
        Task AddLocationAsync(OrganizationLocation location);

        Task UpdateLocationAsync(OrganizationLocation location);

        Task<OrganizationLocation> GetLocationAsync(String locationId);
    }
}
