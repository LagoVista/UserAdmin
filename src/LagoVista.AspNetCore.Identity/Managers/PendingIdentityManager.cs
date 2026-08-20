using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Account;
using LagoVista.UserAdmin.Interfaces.Repos.Security;
using LagoVista.UserAdmin.Models.Auth;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Resources;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.ViewModels.Organization;
using Microsoft.AspNetCore.Identity;
using Security.Models;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LagoVista.AspNetCore.Identity.Managers
{
    internal class PendingIdentityManager : ManagerBase, IPendingIdentityManager
    {
        private const string InvalidVerificationCodeMessage = "The email verification code is invalid or expired.";

        private readonly IPendingIdentityRepo _identityRepo;
        private readonly IPasswordHasher<PendingIdentity> _passwordHasher;
        private readonly IEmailVerificationCodeRepo _emailVerificationCodeRepo;
        private readonly IEmailSender _emailSender;
        private readonly IAppConfig _appConfig;

        public PendingIdentityManager(
            IPendingIdentityRepo pendingIdentityRepo,
            IDependencyManager depManager,
            IPasswordHasher<PendingIdentity> passwordHasher,
            IEmailVerificationCodeRepo emailVerificationCodeRepo,
            IEmailSender emailSender,
            ISecurity security,
            IAdminLogger logger,
            IAppConfig appConfig) : base(logger, appConfig, depManager, security)
        {
            _identityRepo = pendingIdentityRepo ?? throw new ArgumentNullException(nameof(pendingIdentityRepo));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _emailVerificationCodeRepo = emailVerificationCodeRepo ?? throw new ArgumentNullException(nameof(emailVerificationCodeRepo));
            _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
            _appConfig = appConfig ?? throw new ArgumentNullException(nameof(appConfig));
        }

        public async Task AddNewOrgAsync(string id, CreateOrganizationViewModel newOrg)
        {
            var identity = await _identityRepo.GetPendingIdentityAsync(id);
            identity.OrgName = newOrg.Name;
            identity.ProposedOrgNamespace = newOrg.Namespace;
            identity.OrgWebSite = newOrg.WebSite;
            await _identityRepo.UpdatePendingIndentiyAsync(identity);
        }

        public Task AddPendingIdentity(PendingIdentity identity)
        {
            return _identityRepo.AddPendingIdentityAsync(identity);
        }

        public async Task AddRegistrationAsync(string id, RegisterUser registration)
        {
            var identity = await _identityRepo.GetPendingIdentityAsync(id);
            identity.FirstName = registration.FirstName;
            identity.LastName = registration.LastName;
            identity.RegisteredEmail = registration.Email;

            if (!String.IsNullOrWhiteSpace(registration.Password))
                identity.PasswordHash = _passwordHasher.HashPassword(identity, registration.Password);

            if (!String.IsNullOrWhiteSpace(identity.RegisteredEmail))
                identity.Status = PendingIdentityStatus.EmailVerificationRequired;

            await _identityRepo.UpdatePendingIndentiyAsync(identity);
        }

        public async Task<AuthenticationResponse> PasswordSignInAsync(AuthLoginRequest loginRequest)
        {
            var identity = await _identityRepo.GetPendingIdentityAsync(loginRequest.Email);
            if (identity == null)
            {
                return new AuthenticationResponse
                {
                    AuthenticationState = AuthenticationResponseState.InvalidCredentials,
                    AuthenticationReasonCode = "pending_identity_not_found"
                };
            }

            var result = _passwordHasher.VerifyHashedPassword(identity, identity.PasswordHash, loginRequest.Password);
            return new AuthenticationResponse
            {
                AuthenticationState = result == PasswordVerificationResult.Failed ? AuthenticationResponseState.InvalidCredentials : AuthenticationResponseState.RegistrationRequired,
                PendingIdentityId = identity.Id,
                AuthenticationReasonCode = result == PasswordVerificationResult.Failed ? "invalid_credentials" : "pending_identity_authenticated"
            };
        }

        public async Task<InvokeResult<string>> SendEmailVerificationAsync(string pendingIdentityId)
        {
            var identity = await _identityRepo.GetPendingIdentityAsync(pendingIdentityId);
            if (identity == null)
                return InvokeResult<string>.FromError("Pending identity was not found.");

            if (String.IsNullOrWhiteSpace(identity.RegisteredEmail))
                return InvokeResult<string>.FromError("An email address is required before verification can begin.");

            if (identity.Status == PendingIdentityStatus.Resolved ||
                identity.Status == PendingIdentityStatus.Canceled ||
                identity.Status == PendingIdentityStatus.Expired ||
                identity.Status == PendingIdentityStatus.Failed)
                return InvokeResult<string>.FromError("Pending identity is not eligible for email verification.");

            var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");
            var now = DateTime.UtcNow;
            var verificationCode = new EmailVerificationCode
            {
                Id = Guid.NewGuid().ToString("N").ToUpperInvariant(),
                UserId = identity.Id,
                CodeHash = _passwordHasher.HashPassword(identity, code),
                CreatedUtc = now,
                ExpiresUtc = now.AddMinutes(10),
                AttemptCount = 0
            };

            await _emailVerificationCodeRepo.StoreAsync(verificationCode);

            var subject = UserAdminResources.Email_Verification_Subject.Replace("[APP_NAME]", _appConfig.AppName);
            var body = UserAdminResources.Email_Verification_Body
                .Replace("[VERIFICATION_CODE]", code)
                .Replace("[CODE]", code);

            var userText = $"{identity.FirstName} {identity.LastName}".Trim();
            if (String.IsNullOrWhiteSpace(userText))
                userText = identity.RegisteredEmail;

            var sendResult = await _emailSender.SendAsync(
                identity.RegisteredEmail,
                subject,
                body,
                _appConfig.SystemOwnerOrg,
                new EntityHeader { Id = identity.Id, Text = userText });

            if (!sendResult.Successful)
                return InvokeResult<string>.FromInvokeResult(sendResult);

            identity.OtpSendCount++;
            identity.LastOtpSentTimestamp = now.ToString("O");
            identity.Status = PendingIdentityStatus.VerifyingEmail;
            identity.LastStep = "email-verification-sent";
            await _identityRepo.UpdatePendingIndentiyAsync(identity);

            return InvokeResult<string>.Create(
                _appConfig.Environment == Environments.Development ||
                _appConfig.Environment == Environments.Local ||
                _appConfig.Environment == Environments.LocalDevelopment
                    ? code
                    : String.Empty);
        }

        public async Task<InvokeResult> VerifyEmailAsync(string pendingIdentityId, string code)
        {
            var identity = await _identityRepo.GetPendingIdentityAsync(pendingIdentityId);
            if (identity == null)
                return InvokeResult.FromError(InvalidVerificationCodeMessage);

            if (identity.Status != PendingIdentityStatus.VerifyingEmail ||
                String.IsNullOrWhiteSpace(identity.RegisteredEmail) ||
                String.IsNullOrWhiteSpace(code) ||
                code.Length != 6)
                return InvokeResult.FromError(InvalidVerificationCodeMessage);

            var verificationCode = await _emailVerificationCodeRepo.GetLatestAsync(identity.Id);
            if (verificationCode == null ||
                verificationCode.ConsumedUtc.HasValue ||
                verificationCode.ExpiresUtc <= DateTime.UtcNow ||
                verificationCode.AttemptCount >= 5)
                return InvokeResult.FromError(InvalidVerificationCodeMessage);

            var result = _passwordHasher.VerifyHashedPassword(identity, verificationCode.CodeHash, code);
            if (result == PasswordVerificationResult.Failed)
            {
                verificationCode.AttemptCount++;
                identity.OtpVerifyFailCount++;

                if (verificationCode.AttemptCount >= 5)
                    verificationCode.ConsumedUtc = DateTime.UtcNow;

                await _emailVerificationCodeRepo.UpdateAsync(verificationCode);
                await _identityRepo.UpdatePendingIndentiyAsync(identity);
                return InvokeResult.FromError(InvalidVerificationCodeMessage);
            }

            verificationCode.ConsumedUtc = DateTime.UtcNow;
            await _emailVerificationCodeRepo.UpdateAsync(verificationCode);

            identity.VerifiedEmail = identity.RegisteredEmail.Trim();
            identity.VerifiedEmailTimestamp = DateTime.UtcNow.ToString("O");
            identity.Status = PendingIdentityStatus.ResolutionRequired;
            identity.LastStep = "email-verified";
            await _identityRepo.UpdatePendingIndentiyAsync(identity);

            return InvokeResult.Success;
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
