using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    internal sealed class CustomerUserManager : ICustomerUserManager
    {
        private readonly IUserRegistrationManager _userRegistrationManager;

        public CustomerUserManager(IUserRegistrationManager userRegistrationManager)
        {
            _userRegistrationManager = userRegistrationManager ?? throw new ArgumentNullException(nameof(userRegistrationManager));
        }

        public Task<InvokeResult<CreateUserResponse>> CreateCustomerUserAsync(CreateCustomerUserRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (EntityHeader.IsNullOrEmpty(request.EndUserAppOrg)) throw new InvalidOperationException("EndUserAppOrg is required when creating a customer user.");
            if (EntityHeader.IsNullOrEmpty(request.Customer)) throw new InvalidOperationException("Customer is required when creating a customer user.");
            if (EntityHeader.IsNullOrEmpty(request.CustomerContact)) throw new InvalidOperationException("CustomerContact is required when creating a customer user.");

            var registration = new RegisterUser
            {
                Source = UserCreationSource.AdminRegister,
                LoginType = LoginTypes.AppEndUser,
                AppId = request.AppId,
                AppInstanceId = request.AppInstanceId,
                ClientType = request.ClientType,
                DeviceId = request.DeviceId,
                OrgId = request.EndUserAppOrg.Id,
                EndUserAppOrg = request.EndUserAppOrg,
                Customer = request.Customer,
                CustomerContact = request.CustomerContact,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Password = request.Password,
                IsCustomerAdmin = request.IsCustomerAdmin,
            };

            return _userRegistrationManager.CreateUserAsync(registration, request.AutoLogin);
        }
    }
}
