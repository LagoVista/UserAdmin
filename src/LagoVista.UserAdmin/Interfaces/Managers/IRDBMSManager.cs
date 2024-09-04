using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface  IRDBMSManager
    {
        Task<InvokeResult> AddAppUserToOrgAsyncAsync(string orgId, AppUser user);
        Task<InvokeResult> RemoveAppUserFromOrgAsync(string orgId, string userId);
        Task<InvokeResult> DeleteOrgAsync(string orgId);
        Task<InvokeResult> UpdateAppUserAsync(string orgId, AppUser user);
        Task<InvokeResult> AddOrgAsync(Models.Orgs.Organization org);
        Task<InvokeResult> UpdateOrgAsync(Models.Orgs.Organization org);
        Task<bool> HasBillingRecords(string orgId);
        Task<List<EntityHeader>> GetBillingContactOrgsForUserAsync(string orgId, string userId);
        Task<bool> UserExistsAsync(string orgId, string userId);
        Task<bool> OrgExistsAsync(string orgId);
    }
}
