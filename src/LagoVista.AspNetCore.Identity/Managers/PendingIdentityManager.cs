using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.ViewModels.Organization;
using Microsoft.AspNetCore.Identity;
using Security.Models;
using System;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    internal class PendingIdentityManager : ManagerBase, IPendingIdentityManager
    {
        IPendingIdentityRepo _identityRepo;
        IPasswordHasher<PendingIdentity> _passwordHasher;

        public PendingIdentityManager(IPendingIdentityRepo pendingIdentityRepo, IDependencyManager depManager, IPasswordHasher<PendingIdentity> passwordHasher, ISecurity security, IAdminLogger logger, IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _identityRepo = pendingIdentityRepo ?? throw new ArgumentNullException(nameof(pendingIdentityRepo));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        }

        public async Task AddNewOrgAsync(string id, CreateOrganizationViewModel newOrg)
        {
            var identity = await _identityRepo.GetPendingIdentityAsync(id);

            identity.OrgName = newOrg.Name;
            identity.ProposedOrgNamespace = newOrg.Namespace;
            identity.OrgWebSite = newOrg.WebSite;

            await _identityRepo.UpdatePendingIndentiyAsync(identity);
        }

        public async Task AddPendingIdentity(PendingIdentity identity)
        {
            await _identityRepo.AddPendingIdentityAsync(identity);
        }

        public async Task AddRegistrationAsync(string id, RegisterUser registration)
        {
            var identity = await _identityRepo.GetPendingIdentityAsync(id);

            identity.FirstName = registration.FirstName;
            identity.LastName = registration.LastName;
            identity.RegisteredEmail = registration.Email;
            identity.PasswordHash = _passwordHasher.HashPassword(identity, registration.Password);


            await _identityRepo.UpdatePendingIndentiyAsync(identity);
        }

        public async Task<UserLoginResponse> PasswordSignInAsync(AuthLoginRequest loginRequest)
        {
            var identity = await _identityRepo.GetPendingIdentityAsync(loginRequest.Email);


            var result = _passwordHasher.VerifyHashedPassword(identity, identity.PasswordHash, loginRequest.Password);

            return new UserLoginResponse()
            {
                 
            };
        }

        public Task DeletePendingIdentityAsync(string id)
        {
            return _identityRepo.DeletePendingIdentityAsync(id);
        }

        public Task<PendingIdentity> GetPendingIdentityAsync(string id)
        {
            return _identityRepo.GetPendingIdentityAsync(id);
        }

        public Task<InvokeResult<AppUser>> TryCreateAppUserAsync(string pendingIdentityId)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePendingIdentity(PendingIdentity identity)
        {
            return _identityRepo.UpdatePendingIndentiyAsync(identity);
        }
    }
}
