// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d4a974d2a79cf758fe595481c31432a1da1ad22c5f0361c6c4663da8608196cd
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Orgs
{
    public interface IInviteUserRepo
    {
        Task InsertInvitationAsync(Invitation invitation);
        Task UpdateInvitationAsync(Invitation invitation);
        Task<Invitation> GetInvitationAsync(String invitationId);
        Task<ListResponse<Invitation>> GetInvitationsForOrgAsync(String orgId, ListRequest listRequest, Invitation.StatusTypes? byStatus = null);
        Task<ListResponse<Invitation>> GetActiveInvitationsForOrgAsync(String orgId, ListRequest listRequest);
        Task<Invitation> GetInviteByOrgIdAndEmailAsync(String orgId, String email);
    }
}
