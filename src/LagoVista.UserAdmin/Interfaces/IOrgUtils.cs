// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d63f5c631894de04e80b1565f2b2ec809a03e07a965cd1fb16615449fce1ac05
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Orgs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IOrgUtils
    {
        Task<InvokeResult<string>> GetOrgNamespaceAsync(string orgId);
        Task<InvokeResult<string>> GetOrgIdFromNameSpaceAsync(string orgNs);
        Task<InvokeResult<Organization>> GetOrgFromNameSpaceAsync(string orgNs);
    }
}
