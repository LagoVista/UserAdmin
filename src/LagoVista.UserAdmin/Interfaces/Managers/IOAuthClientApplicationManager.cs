using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IOAuthClientApplicationManager
    {
        Task<InvokeResult> AddOAuthClientApplicationAsync(OAuthClientApplication client, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateOAuthClientApplicationAsync(OAuthClientApplication client, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteOAuthClientApplicationAsync(string id, EntityHeader org, EntityHeader user);
        Task<OAuthClientApplication> GetOAuthClientApplicationAsync(string id, EntityHeader org, EntityHeader user);
        Task<OAuthClientApplication> GetOAuthClientApplicationByClientIdAsync(string clientId);
        Task<ListResponse<OAuthClientApplicationSummary>> GetOAuthClientApplicationsForOrgAsync(EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<bool> QueryKeyInUseAsync(string key, EntityHeader org);
        Task<bool> QueryClientIdInUseAsync(string clientId, string currentId = null);
        Task<DependentObjectCheckResult> CheckOAuthClientApplicationInUseAsync(string id, EntityHeader org, EntityHeader user);
    }
}
