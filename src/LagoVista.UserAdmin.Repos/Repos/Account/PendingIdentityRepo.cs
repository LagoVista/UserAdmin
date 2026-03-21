using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using Security.Models;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Account
{
    public class PendingIdentityRepo : DocumentDBRepoBase<PendingIdentity>, IPendingIdentityRepo
    {
        public PendingIdentityRepo(IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
        }

        public Task AddPendingIdentityAsync(PendingIdentity identity)
        {
            return CreateDocumentAsync(identity);
        }

        public Task DeletePendingIdentityAsync(string pendingIdentityId)
        {
            return DeleteDocumentAsync(pendingIdentityId);
        }

        public Task<PendingIdentity> GetPendingIdentityAsync(string pendingIdentityId)
        {
            return GetDocumentAsync(pendingIdentityId);
        }

        public async Task<PendingIdentity> GetPendingIdentityByEmailAsync(string email)
        {
            var identity = await QueryAsync(rec => rec.RegisteredEmail == email);
            return identity.FirstOrDefault();
        }

        public Task UpdatePendingIndentiyAsync(PendingIdentity identity)
        {
            return UpsertDocumentAsync(identity);
        }
    }
}
