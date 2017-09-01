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
        Task<Invitation> GetInvitationAsync(String invitation);
        Task<ListResponse<Invitation>> GetInvitationsForOrgAsync(String orgId, ListRequest listRequest, Invitation.StatusTypes? byStatus = null);
        Task<ListResponse<Invitation>> GetActiveInvitationsForOrgAsync(String orgId, ListRequest listRequest);
        Task<Invitation> GetInviteByOrgIdAndEmailAsync(String orgId, String email);
    }
}
