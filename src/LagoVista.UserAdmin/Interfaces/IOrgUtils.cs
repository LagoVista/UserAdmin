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
