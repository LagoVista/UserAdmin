using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    internal sealed class CustomerUserManager : ICustomerUserManager
    {
        private readonly IUserRegistrationManager _userRegistrationManager;
        private readonly IAppUserManager _appUserManager;
        private readonly IAppUserRepo _appUserRepo;

        public CustomerUserManager(IUserRegistrationManager userRegistrationManager, IAppUserManager appUserManager, IAppUserRepo appUserRepo)
        {
            _userRegistrationManager = userRegistrationManager ?? throw new ArgumentNullException(nameof(userRegistrationManager));
            _appUserManager = appUserManager ?? throw new ArgumentNullException(nameof(appUserManager));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
        }

        private static void ValidateContext(EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            if (EntityHeader.IsNullOrEmpty(customer)) throw new ArgumentNullException(nameof(customer));
            if (EntityHeader.IsNullOrEmpty(org)) throw new ArgumentNullException(nameof(org));
            if (EntityHeader.IsNullOrEmpty(user)) throw new ArgumentNullException(nameof(user));
        }

        private static void ValidateCustomerUser(AppUser appUser, EntityHeader customer, EntityHeader org)
        {
            if (appUser == null) throw new InvalidOperationException("Could not locate the requested customer user.");

            if (appUser.LoginType != LoginTypes.AppEndUser)
                throw new UnauthorizedAccessException("The selected account is not an AppEndUser account.");

            if (EntityHeader.IsNullOrEmpty(appUser.EndUserAppOrg) ||
                !String.Equals(appUser.EndUserAppOrg.Id, org.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("The selected account does not belong to the current organization.");
            }

            if (EntityHeader.IsNullOrEmpty(appUser.Customer) ||
                !String.Equals(appUser.Customer.Id, customer.Id, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("The selected account does not belong to the current customer.");
            }
        }

        private async Task AuthorizeCustomerAdministratorAsync(EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            ValidateContext(customer, org, user);

            var currentUser = await _appUserRepo.FindByIdAsync(user.Id);
            ValidateCustomerUser(currentUser, customer, org);

            if (!currentUser.IsCustomerAdmin)
                throw new UnauthorizedAccessException("Customer administrator privileges are required to manage customer users.");
        }

        private async Task<AppUser> GetAuthorizedCustomerUserAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            if (String.IsNullOrEmpty(userId)) throw new ArgumentNullException(nameof(userId));

            await AuthorizeCustomerAdministratorAsync(customer, org, user);

            var appUser = await _appUserRepo.FindByIdAsync(userId);
            ValidateCustomerUser(appUser, customer, org);
            return appUser;
        }

        private static CustomerUserSummary CreateSummary(AppUser appUser)
        {
            return new CustomerUserSummary
            {
                Id = appUser.Id,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email,
                EndUserAppOrg = appUser.EndUserAppOrg,
                Customer = appUser.Customer,
                CustomerContact = appUser.CustomerContact,
                IsCustomerAdmin = appUser.IsCustomerAdmin,
                IsAccountDisabled = appUser.IsAccountDisabled,
                EmailConfirmed = appUser.EmailConfirmed,
            };
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

        public async Task<InvokeResult<CreateUserResponse>> CreateCustomerUserAsync(CreateCustomerUserRequest request, EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            await AuthorizeCustomerAdministratorAsync(customer, org, user);

            request.Customer = customer;
            request.EndUserAppOrg = org;
            request.AutoLogin = false;

            return await CreateCustomerUserAsync(request);
        }

        public async Task<ListResponse<UserInfoSummary>> GetCustomerUsersAsync(EntityHeader customer, EntityHeader org, EntityHeader user, ListRequest listRequest)
        {
            await AuthorizeCustomerAdministratorAsync(customer, org, user);
            return await _appUserRepo.GetCustomerUsersAsync(org.Id, customer.Id, listRequest);
        }

        public async Task<InvokeResult<CustomerUserSummary>> GetCustomerUserAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            var appUser = await GetAuthorizedCustomerUserAsync(userId, customer, org, user);
            return InvokeResult<CustomerUserSummary>.Create(CreateSummary(appUser));
        }

        public async Task<InvokeResult<CustomerUserSummary>> UpdateCustomerUserAsync(string userId, UpdateCustomerUserRequest request, EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (EntityHeader.IsNullOrEmpty(request.CustomerContact)) return InvokeResult<CustomerUserSummary>.FromError("CustomerContact is required.");

            var appUser = await GetAuthorizedCustomerUserAsync(userId, customer, org, user);
            appUser.FirstName = request.FirstName;
            appUser.LastName = request.LastName;
            appUser.CustomerContact = request.CustomerContact;
            appUser.LastUpdatedBy = user;
            appUser.LastUpdatedDate = Core.UtcTimestamp.Now;

            var updateResult = await _appUserManager.UpdateUserAsync(appUser, org, user);
            if (!updateResult.Successful)
                return InvokeResult<CustomerUserSummary>.FromInvokeResult(updateResult);

            return InvokeResult<CustomerUserSummary>.Create(CreateSummary(appUser));
        }

        public async Task<InvokeResult> EnableCustomerUserAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            var appUser = await GetAuthorizedCustomerUserAsync(userId, customer, org, user);
            appUser.IsAccountDisabled = false;
            appUser.LastUpdatedBy = user;
            appUser.LastUpdatedDate = Core.UtcTimestamp.Now;
            return await _appUserManager.UpdateUserAsync(appUser, org, user);
        }

        public async Task<InvokeResult> DisableCustomerUserAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            var appUser = await GetAuthorizedCustomerUserAsync(userId, customer, org, user);

            if (String.Equals(appUser.Id, user.Id, StringComparison.OrdinalIgnoreCase))
                return InvokeResult.FromError("A customer administrator cannot disable their own account.");

            appUser.IsAccountDisabled = true;
            appUser.LastUpdatedBy = user;
            appUser.LastUpdatedDate = Core.UtcTimestamp.Now;
            return await _appUserManager.UpdateUserAsync(appUser, org, user);
        }

        public async Task<InvokeResult> SetCustomerAdminAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            await GetAuthorizedCustomerUserAsync(userId, customer, org, user);
            return await _appUserManager.SetEndUserContactAsCustomerAdminAsync(userId, customer, org, user);
        }

        public async Task<InvokeResult> ClearCustomerAdminAsync(string userId, EntityHeader customer, EntityHeader org, EntityHeader user)
        {
            await GetAuthorizedCustomerUserAsync(userId, customer, org, user);

            if (String.Equals(userId, user.Id, StringComparison.OrdinalIgnoreCase))
                return InvokeResult.FromError("A customer administrator cannot revoke their own customer-administrator privileges.");

            return await _appUserManager.ClearEndUserContactAsCustomerAdminAsync(userId, customer, org, user);
        }
    }
}
