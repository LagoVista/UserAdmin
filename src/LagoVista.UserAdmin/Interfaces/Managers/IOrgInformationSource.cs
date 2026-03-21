// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 91f8226c0a63ee141df21c5c6cba92548088798859efd58e56ac1b0c60f37e91
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IOrgInformationSource
    {
        Task<PublicOrgInformation> GetPublicOrginfoAsync(string orgns);
        Task<string> GetOrgNameAsync(string orgId);
        Task<string> GetOrgNameSpaceAsync(string orgId);
        Task<string> GetOrgIdForNameSpaceAsync(string orgNameSpace);
        Task<EntityHeader> GetOrgEntityHeaderForNameSpaceAsync(string orgNameSpace);
        Task<InvokeResult<BasicTheme>> GetBasicThemeForOrgAsync(string orgid);
        Task<bool> QueryLocationNamespaceInUseAsync(string orgId, string namespaceText);
        Task<bool> QueryOrganizationHasUserAsync(string orgId, string userId, EntityHeader org, EntityHeader user);
        Task<bool> QueryOrgNamespaceInUseAsync(string namespaceText);
        Task<InvokeResult<string>> GetLandingPageForOrgAsync(string orgid);
        Task<OrgLocation> GetOrgLocationAsync(string id, EntityHeader org, EntityHeader user);
        Task<ListResponse<OrgLocationSummary>> GetLocationsForOrganizationsAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
    }
}
