using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Users;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Interfaces.Managers
{
    public interface IProvisionalEnvironmentManager
    {
        Task<InvokeResult<CreateProvisionalEnvironmentResponse>> CreateAsync(CreateProvisionalEnvironmentRequest request);
    }
}
