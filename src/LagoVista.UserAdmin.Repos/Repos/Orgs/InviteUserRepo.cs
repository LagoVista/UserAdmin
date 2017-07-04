using System.Threading.Tasks;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;
using System.Linq;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.IoT.Logging.Loggers;

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
            return base.GetAsync(id);
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
