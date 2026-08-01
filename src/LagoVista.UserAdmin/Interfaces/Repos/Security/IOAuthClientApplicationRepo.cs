using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Auth;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Repos.Security
{
    public interface IOAuthClientApplicationRepo
    {
        Task AddOAuthClientApplicationAsync(OAuthClientApplication client);
        Task UpdateOAuthClientApplicationAsync(OAuthClientApplication client);
        Task DeleteOAuthClientApplicationAsync(string id);
        Task<OAuthClientApplication> GetOAuthClientApplicationAsync(string id);
        Task<OAuthClientApplication> GetOAuthClientApplicationByClientIdAsync(string clientId);
        Task<ListResponse<OAuthClientApplicationSummary>> GetOAuthClientApplicationsAsync(string orgId, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, string orgId);
        Task<bool> QueryClientIdInUseAsync(string clientId, string currentId = null);
    }
}
