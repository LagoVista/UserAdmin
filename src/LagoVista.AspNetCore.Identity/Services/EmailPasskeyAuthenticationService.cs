using LagoVista.AspNetCore.Identity.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Auth.Passkeys;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Services
{
    public class EmailPasskeyAuthenticationService : IEmailPasskeyAuthenticationService
    {
        private readonly IAppUserRepo _appUserRepo;
        private readonly IAppUserPasskeyManager _passkeyManager;

        public EmailPasskeyAuthenticationService(IAppUserRepo appUserRepo, IAppUserPasskeyManager passkeyManager)
        {
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
            _passkeyManager = passkeyManager ?? throw new ArgumentNullException(nameof(passkeyManager));
        }

        public async Task<InvokeResult<PasskeyBeginOptionsResponse>> BeginAsync(string email, string passkeyUrl, EntityHeader organization, EntityHeader user)
        {
            var appUser = await ResolveUserAsync(email);
            if (appUser == null)
                return InvokeResult<PasskeyBeginOptionsResponse>.FromError("passkey_not_available");

            // Primary passkey sign-in and passkey MFA both require user verification.
            // BeginAuthenticationOptionsAsync uses isStepUp=true to emit UV=required;
            // whether the completed proof counts as step-up is carried separately.
            var result = await _passkeyManager.BeginAuthenticationOptionsAsync(appUser.Id, true, passkeyUrl, organization, user);
            if (!result.Successful)
                return InvokeResult<PasskeyBeginOptionsResponse>.FromError("passkey_not_available");

            return result;
        }

        public async Task<InvokeResult<AppUser>> CompleteAsync(string email, PasskeyAuthenticationCompleteRequest request, bool isStepUp, EntityHeader organization, EntityHeader user)
        {
            if (request == null)
                return InvokeResult<AppUser>.FromError("passkey_request_required");

            var appUser = await ResolveUserAsync(email);
            if (appUser == null)
                return InvokeResult<AppUser>.FromError("passkey_authentication_failed");

            // CompleteAuthenticationAsync verifies that the consumed challenge belongs
            // to this exact user and that the asserted credential is registered to them.
            // isStepUp only controls MFA-freshness bookkeeping after successful proof.
            var result = await _passkeyManager.CompleteAuthenticationAsync(appUser.Id, request, isStepUp, organization, user);
            if (!result.Successful)
                return InvokeResult<AppUser>.FromError("passkey_authentication_failed");

            return InvokeResult<AppUser>.Create(appUser);
        }

        private async Task<AppUser> ResolveUserAsync(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
                return null;

            return await _appUserRepo.FindByEmailAsync(email.Trim());
        }
    }
}
