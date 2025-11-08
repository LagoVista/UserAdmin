// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f8c4174d045f557ef8ce9381b11a428d66f842b6c346fd7939cccaf1e54694c6
// IndexVersion: 2
// --- END CODE INDEX META ---
using System.Threading.Tasks;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;
using System.Linq;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Core.Models.UIMetaData;
using System;
using LagoVista.Core.Models;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class InviteUserRepo : TableStorageBase<Invitation>, IInviteUserRepo
    {
        public InviteUserRepo(IUserAdminSettings settings, IAdminLogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task<Invitation> GetInvitationAsync(string id)
        {
            return base.GetAsync(id, false);
        }

        public async Task<ListResponse<Invitation>> GetInvitationsForOrgAsync(string orgId, ListRequest listRequest, Invitation.StatusTypes? byStatus)
        {
            var invitations = await GetPagedResultsAsync(orgId, listRequest);
            if (byStatus.HasValue)
            {
                //TODO: Yeah, we can do better...should extend calls with ListRequest to do additional filtering.
                invitations.Model = invitations.Model.Where(invite => invite.Status == byStatus.Value);
            }

            return invitations;
        }

        public async Task<ListResponse<Invitation>> GetActiveInvitationsForOrgAsync(string orgId, ListRequest listRequest)
        {
            var invitations = await GetPagedResultsAsync(orgId, listRequest);
            //TODO: Yeah, we can do better...should extend calls with ListRequest to do additional filtering.
            invitations.Model = invitations.Model.Where(invite => (invite.Status == Invitation.StatusTypes.Sent ||
                                                                    invite.Status == Invitation.StatusTypes.Replaced ||
                                                                    invite.Status == Invitation.StatusTypes.Resent ||
                                                                    invite.Status == Invitation.StatusTypes.New));
            return invitations;
        }

        public async Task<Invitation> GetInviteByOrgIdAndEmailAsync(string orgId, string email)
        {
            return (await GetByFilterAsync(
                 FilterOptions.Create(nameof(Invitation.Email), FilterOptions.Operators.Equals, email.ToUpper()),
                 FilterOptions.Create(nameof(Invitation.OrganizationId), FilterOptions.Operators.Equals, orgId)
                 )).FirstOrDefault();
        }

        public Task InsertInvitationAsync(Invitation invitation)
        {
            invitation.Email = invitation.Email.ToUpper();
            return base.InsertAsync(invitation);
        }

        public Task UpdateInvitationAsync(Invitation invitation)
        {
            return base.UpdateAsync(invitation);
        }
    }
}
