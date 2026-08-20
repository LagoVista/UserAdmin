using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using Security.Models;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class PendingIdentityResolutionService : IPendingIdentityResolutionService
    {
        private readonly IPendingIdentityManager _pendingIdentityManager;
        private readonly IAppUserRepo _appUserRepo;
        private readonly IAppUserManager _appUserManager;
        private readonly IUserRegistrationManager _userRegistrationManager;

        public PendingIdentityResolutionService(
            IPendingIdentityManager pendingIdentityManager,
            IAppUserRepo appUserRepo,
            IAppUserManager appUserManager,
            IUserRegistrationManager userRegistrationManager)
        {
            _pendingIdentityManager = pendingIdentityManager ?? throw new ArgumentNullException(nameof(pendingIdentityManager));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _appUserManager = appUserManager ?? throw new ArgumentNullException(nameof(appUserManager));
            _userRegistrationManager = userRegistrationManager ?? throw new ArgumentNullException(nameof(userRegistrationManager));
        }

        public async Task<InvokeResult<AppUser>> ResolveOAuthAsync(string pendingIdentityId, RegisterUser registrationContext)
        {
            if (String.IsNullOrWhiteSpace(pendingIdentityId))
                return InvokeResult<AppUser>.FromError("Pending identity id is required.");

            var identity = await _pendingIdentityManager.GetPendingIdentityAsync(pendingIdentityId);
            if (identity == null)
                return InvokeResult<AppUser>.FromError("Pending identity was not found.");

            if (identity.FlowType != PendingIdentityFlowType.OAuthExternalLogin)
                return InvokeResult<AppUser>.FromError("Pending identity is not an OAuth external-login ceremony.");

            if (identity.Status != PendingIdentityStatus.ResolutionRequired || String.IsNullOrWhiteSpace(identity.VerifiedEmail))
                return InvokeResult<AppUser>.FromError("Pending identity does not have independently verified email proof.");

            if (String.IsNullOrWhiteSpace(identity.OAuthProvider) || String.IsNullOrWhiteSpace(identity.OAuthSubject))
                return InvokeResult<AppUser>.FromError("Pending identity does not contain a valid external provider subject.");

            if (!Enum.TryParse<ExternalLoginTypes>(identity.OAuthProvider, true, out var providerType))
                return InvokeResult<AppUser>.FromError($"Unsupported external login provider [{identity.OAuthProvider}].");

            var verifiedEmail = identity.VerifiedEmail.Trim();
            var externalLogin = new ExternalLogin
            {
                Provider = EntityHeader<ExternalLoginTypes>.Create(providerType),
                Id = identity.OAuthSubject,
                UserName = identity.OAuthSubject,
                Email = verifiedEmail,
                FirstName = identity.FirstName,
                LastName = identity.LastName,
                Organization = identity.OrgName
            };

            var subjectUser = await _appUserManager.GetUserByExternalLoginAsync(providerType, identity.OAuthSubject);
            if (subjectUser != null)
            {
                if (!String.Equals(subjectUser.Email, verifiedEmail, StringComparison.OrdinalIgnoreCase))
                    return InvokeResult<AppUser>.FromError("External provider subject is already linked to a different account.");

                return await CompleteResolutionAsync(identity, subjectUser, "provider-subject-already-linked");
            }

            var existingUser = await _appUserRepo.FindByEmailAsync(verifiedEmail);
            if (existingUser != null)
            {
                var linkedUser = await _appUserManager.AssociateExternalLoginAsync(existingUser.Id, externalLogin, existingUser.ToEntityHeader());
                if (!linkedUser.EmailConfirmed)
                {
                    linkedUser.EmailConfirmed = true;
                    await _appUserRepo.UpdateAsync(linkedUser);
                }

                return await CompleteResolutionAsync(identity, linkedUser, "verified-email-linked-existing-user");
            }

            if (registrationContext == null)
                return InvokeResult<AppUser>.FromError("Registration context is required to create a new durable user.");

            var registration = new RegisterUser
            {
                Source = UserCreationSource.OAuth,
                LoginType = registrationContext.LoginType,
                AppId = registrationContext.AppId,
                AppInstanceId = registrationContext.AppInstanceId,
                ClientType = registrationContext.ClientType,
                DeviceId = registrationContext.DeviceId,
                FirstName = String.IsNullOrWhiteSpace(identity.FirstName) ? registrationContext.FirstName : identity.FirstName,
                LastName = String.IsNullOrWhiteSpace(identity.LastName) ? registrationContext.LastName : identity.LastName,
                Email = verifiedEmail,
                InviteId = identity.InviteId,
                EndUserAppOrg = registrationContext.EndUserAppOrg,
                Customer = registrationContext.Customer,
                CustomerContact = registrationContext.CustomerContact,
                IsCustomerAdmin = registrationContext.IsCustomerAdmin,
                CustomerName = registrationContext.CustomerName,
                CustomerCity = registrationContext.CustomerCity,
                CustomerState = registrationContext.CustomerState
            };

            var createResult = await _userRegistrationManager.CreateUserAsync(registration, autoLogin: false, externalLogin: externalLogin);
            if (!createResult.Successful)
                return InvokeResult<AppUser>.FromInvokeResult(createResult.ToInvokeResult());

            var newUser = createResult.Result?.AppUser;
            if (newUser == null)
                return InvokeResult<AppUser>.FromError("Durable user creation completed without returning the created user.");

            if (!newUser.EmailConfirmed)
            {
                newUser.EmailConfirmed = true;
                await _appUserRepo.UpdateAsync(newUser);
            }

            return await CompleteResolutionAsync(identity, newUser, "verified-email-created-new-user");
        }

        private async Task<InvokeResult<AppUser>> CompleteResolutionAsync(PendingIdentity identity, AppUser appUser, string reasonCode)
        {
            identity.ResolutionTargetUserId = appUser.Id;
            identity.ResolutionReasonCode = reasonCode;
            identity.Status = PendingIdentityStatus.Resolved;
            identity.LastStep = "identity-resolved";
            await _pendingIdentityManager.UpdatePendingIdentity(identity);

            return InvokeResult<AppUser>.Create(appUser);
        }
    }
}
