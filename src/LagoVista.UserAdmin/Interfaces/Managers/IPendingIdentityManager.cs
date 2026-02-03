using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Managers;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.ViewModels.Organization;
using Security.Models;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IPendingIdentityManager
    {
        Task AddPendingIdentity(PendingIdentity identity);
        Task<PendingIdentity> GetPendingIdentityAsync(string pendingIdentityId);
        Task UpdatePendingIdentity(PendingIdentity identity);
        Task AddRegistrationAsync(string pendingIdentityId, RegisterUser registration);
        Task AddNewOrgAsync(string pendingIdentityId, CreateOrganizationViewModel newOrg);
        Task DeletePendingIdentityAsync(string pendingIdentityId);
        Task<InvokeResult<AppUser>> TryCreateAppUserAsync(string pendingIdentityId);
    }
}
