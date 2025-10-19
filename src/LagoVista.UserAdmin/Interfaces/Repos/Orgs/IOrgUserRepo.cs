// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 40baeace74c35477d10990d62717e61e5e831cb1b47195975fc26d869c724a53
// IndexVersion: 0
// --- END CODE INDEX META ---
using System.Collections.Generic;
using System.Threading.Tasks;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.Core.Models;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IOrgUserRepo
    {
        Task AddOrgUserAsync(OrgUser orgUser);
        Task<IEnumerable<OrgUser>> GetOrgsForUserAsync(string userId);
        Task<IEnumerable<OrgUser>> GetUsersForOrgAsync(string orgId);
        Task RemoveUserFromOrgAsync(string orgid, string userId, EntityHeader removedBy);
        Task<bool> QueryOrgHasUserAsync(string orgId, string userId);
        Task<bool> QueryOrgHasUserByEmailAsync(string orgId, string email);

        Task<bool> IsUserOrgAdminAsync(string orgId, string userId);
        Task<bool> IsAppBuilderAsync(string orgId, string userId);
        Task<OrgUser> GetOrgUserAsync(string orgId, string userId);
        Task UpdateOrgUserAsync(OrgUser orgUser);
        Task ClearOrgCacheAsync(string orgId);
    }
}