using LagoVista.Core.Validation;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin
{
    public interface IOrgUtils
    {
        Task<InvokeResult<string>> GetOrgNamespaceAsync(string orgId);
    }
}
