using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public interface ICustomerUserManager
    {
        Task<InvokeResult<CreateUserResponse>> CreateCustomerUserAsync(CreateCustomerUserRequest request);
        Task<InvokeResult<CreateUserResponse>> CreateCustomerUserAsync(CreateCustomerUserRequest request, EntityHeader customer, EntityHeader org, EntityHeader user);
        Task<ListResponse<UserInfoSummary>> GetCustomerUsersAsync(EntityHeader customer, EntityHeader org, EntityHeader user, ListRequest listRequest);
        Task<InvokeResult<CustomerUserSummary>> GetCustomerUserAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user);
        Task<InvokeResult<CustomerUserSummary>> UpdateCustomerUserAsync(string userId, UpdateCustomerUserRequest request, EntityHeader customer, EntityHeader org, EntityHeader user);
        Task<InvokeResult> EnableCustomerUserAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DisableCustomerUserAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user);
        Task<InvokeResult> SetCustomerAdminAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user);
        Task<InvokeResult> ClearCustomerAdminAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user);
    }
}
