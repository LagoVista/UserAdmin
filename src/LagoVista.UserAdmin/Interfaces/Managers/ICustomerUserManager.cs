using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public interface ICustomerUserManager
    {
        Task<InvokeResult<CreateUserResponse>> CreateCustomerUserAsync(CreateCustomerUserRequest request);
    }
}
