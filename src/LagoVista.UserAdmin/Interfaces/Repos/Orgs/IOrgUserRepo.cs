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
    }
}