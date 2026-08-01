using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public class OAuthClientApplicationRepo : DocumentDBRepoBase<OAuthClientApplication>, IOAuthClientApplicationRepo
    {
        public OAuthClientApplicationRepo(IUserAdminSettings userAdminSettings, IDocumentCloudCachedServices services) :
            base(userAdminSettings.UserStorage.Uri, userAdminSettings.UserStorage.AccessKey, userAdminSettings.UserStorage.ResourceName, services)
        {
        }

        public Task AddOAuthClientApplicationAsync(OAuthClientApplication client)
        {
            return CreateDocumentAsync(client);
        }

        public Task UpdateOAuthClientApplicationAsync(OAuthClientApplication client)
        {
            return UpsertDocumentAsync(client);
        }

        public Task DeleteOAuthClientApplicationAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<OAuthClientApplication> GetOAuthClientApplicationAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public async Task<OAuthClientApplication> GetOAuthClientApplicationByClientIdAsync(string clientId)
        {
            return (await QueryAsync(client => client.ClientId == clientId)).FirstOrDefault();
        }

        public Task<ListResponse<OAuthClientApplicationSummary>> GetOAuthClientApplicationsAsync(string orgId, ListRequest listRequest)
        {
            return QuerySummaryAsync<OAuthClientApplicationSummary, OAuthClientApplication>(client => client.OwnerOrganization.Id == orgId, client => client.Name, listRequest);
        }

        public async Task<bool> QueryKeyInUseAsync(string key, string orgId)
        {
            return (await QueryAsync(client => (client.OwnerOrganization.Id == orgId || client.IsPublic == true) && client.Key == key)).Any();
        }

        public async Task<bool> QueryClientIdInUseAsync(string clientId, string currentId = null)
        {
            var matches = await QueryAsync(client => client.ClientId == clientId);
            return matches.Any(client => currentId == null || client.Id != currentId);
        }
    }
}
