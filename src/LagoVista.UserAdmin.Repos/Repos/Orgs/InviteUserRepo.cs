using System.Threading.Tasks;
using LagoVista.CloudStorage.Storage;
using LagoVista.Core.PlatformSupport;
using System.Linq;
using LagoVista.UserAdmin.Interfaces.Repos.Orgs;
using LagoVista.UserAdmin.Models.Orgs;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public class InviteUserRepo : TableStorageBase<Invitation>, IInviteUserRepo
    {
        public InviteUserRepo(IUserAdminSettings settings, ILogger logger) :
            base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger)
        {

        }

        public Task<Invitation> GetInvitationAsync(string id)
        {
            return base.GetAsync(id);
        }

        public async Task<Invitation> GetInviteByOrgIdAndEmailAsync(string organizationId, string email)
        {
            return (await GetByFilterAsync(
                 FilterOptions.Create("Email", FilterOptions.Operators.Equals, email.ToUpper()),
                 FilterOptions.Create("OrganizationId", FilterOptions.Operators.Equals, organizationId)
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
