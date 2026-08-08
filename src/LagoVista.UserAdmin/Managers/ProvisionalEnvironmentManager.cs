using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class ProvisionalEnvironmentManager : IProvisionalEnvironmentManager
    {
        private const int RecoveryTokenBytes = 32;
        private const int ActiveLifetimeDays = 30;

        private readonly IProvisionalEnvironmentRepo _environmentRepo;
        private readonly IUserManager _userManager;
        private readonly IOrganizationManager _organizationManager;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ISubscriptionLevelManager _subscriptionLevelManager;

        public ProvisionalEnvironmentManager(IProvisionalEnvironmentRepo environmentRepo, IUserManager userManager, IOrganizationManager organizationManager, ISubscriptionManager subscriptionManager, ISubscriptionLevelManager subscriptionLevelManager)
        {
            _environmentRepo = environmentRepo ?? throw new ArgumentNullException(nameof(environmentRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _organizationManager = organizationManager ?? throw new ArgumentNullException(nameof(organizationManager));
            _subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _subscriptionLevelManager = subscriptionLevelManager ?? throw new ArgumentNullException(nameof(subscriptionLevelManager));
        }

        public async Task<InvokeResult<CreateProvisionalEnvironmentResponse>> CreateAsync(CreateProvisionalEnvironmentRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (String.IsNullOrWhiteSpace(request.CreationRequestId)) return InvokeResult<CreateProvisionalEnvironmentResponse>.FromError("CreationRequestId is required.");

            var recoveryToken = CreateRecoveryToken();
            var environment = await _environmentRepo.FindByCreationRequestIdAsync(request.CreationRequestId);
            var wasResumed = environment != null;

            if (environment == null)
            {
                var now = DateTime.UtcNow;
                environment = new ProvisionalEnvironment
                {
                    Id = Guid.NewGuid().ToId(),
                    State = ProvisionalEnvironmentState.Provisioning,
                    CreationRequestId = request.CreationRequestId,
                    AppUserId = Guid.NewGuid().ToId(),
                    OrganizationId = Guid.NewGuid().ToId(),
                    SubscriptionId = Guid.NewGuid().ToId(),
                    RecoveryTokenHash = Hash(recoveryToken),
                    InstallationIdHash = HashOptional(request.InstallationId),
                    CreatedUtc = now,
                    LastActivityUtc = now,
                    ExpiresUtc = now.AddDays(ActiveLifetimeDays),
                    StateChangedUtc = now,
                    ConversionJourneyId = request.ConversionJourneyId,
                    AcquisitionSourceKey = request.AcquisitionSourceKey,
                    CampaignKey = request.CampaignKey,
                    EntryPointType = request.EntryPointType,
                    EntryPointKey = request.EntryPointKey,
                    ExperimentKey = request.ExperimentKey,
                    ExperimentVariantKey = request.ExperimentVariantKey,
                    AgentKey = request.AgentKey,
                    AgentVersion = request.AgentVersion,
                    PromptVersion = request.PromptVersion
                };

                try
                {
                    await _environmentRepo.CreateAsync(environment);
                }
                catch
                {
                    environment = await _environmentRepo.FindByCreationRequestIdAsync(request.CreationRequestId);
                    if (environment == null) throw;
                    wasResumed = true;
                }
            }

            if (environment.State == ProvisionalEnvironmentState.Claimed || environment.State == ProvisionalEnvironmentState.Expired || environment.State == ProvisionalEnvironmentState.PurgePending)
                return InvokeResult<CreateProvisionalEnvironmentResponse>.FromError($"Provisional environment is {environment.State.ToString().ToLowerInvariant()}.");

            if (wasResumed)
            {
                environment.RecoveryTokenHash = Hash(recoveryToken);
                if (!String.IsNullOrWhiteSpace(request.InstallationId)) environment.InstallationIdHash = Hash(request.InstallationId);
                await _environmentRepo.UpdateAsync(environment);
            }

            var appUser = await EnsureUserAsync(environment);
            if (!appUser.Successful) return InvokeResult<CreateProvisionalEnvironmentResponse>.FromInvokeResult(appUser.ToInvokeResult());

            var organization = await _organizationManager.CreateProvisionalOrganizationAsync(appUser.Result, environment.OrganizationId);
            if (!organization.Successful) return InvokeResult<CreateProvisionalEnvironmentResponse>.FromInvokeResult(organization.ToInvokeResult());

            var subscription = await EnsureSubscriptionAsync(environment, organization.Result, appUser.Result);
            if (!subscription.Successful) return InvokeResult<CreateProvisionalEnvironmentResponse>.FromInvokeResult(subscription);

            var activatedUtc = DateTime.UtcNow;
            var stateChanged = environment.State != ProvisionalEnvironmentState.Active;
            environment.State = ProvisionalEnvironmentState.Active;
            environment.ActivatedUtc = environment.ActivatedUtc ?? activatedUtc;
            environment.LastActivityUtc = activatedUtc;
            environment.ExpiresUtc = activatedUtc.AddDays(ActiveLifetimeDays);
            if (stateChanged) environment.StateChangedUtc = activatedUtc;
            await _environmentRepo.UpdateAsync(environment);

            return InvokeResult<CreateProvisionalEnvironmentResponse>.Create(new CreateProvisionalEnvironmentResponse
            {
                ProvisionalEnvironmentId = environment.Id,
                AppUserId = environment.AppUserId,
                OrganizationId = environment.OrganizationId,
                SubscriptionId = environment.SubscriptionId,
                RecoveryToken = recoveryToken,
                ExpiresUtc = environment.ExpiresUtc,
                WasResumed = wasResumed
            });
        }

        private async Task<InvokeResult<AppUser>> EnsureUserAsync(ProvisionalEnvironment environment)
        {
            var appUser = await _userManager.FindByIdAsync(environment.AppUserId);
            if (appUser != null) return InvokeResult<AppUser>.Create(appUser);

            appUser = new AppUser(null, $"provisional-{environment.AppUserId}", "Provisional Environment")
            {
                Id = environment.AppUserId,
                CreatedBy = EntityHeader.Create(environment.AppUserId, "Provisional Environment"),
                LastUpdatedBy = EntityHeader.Create(environment.AppUserId, "Provisional Environment"),
                IsAnonymous = true,
                ShowWelcome = false
            };

            var createResult = await _userManager.CreateAsync(appUser);
            return createResult.Successful ? InvokeResult<AppUser>.Create(appUser) : InvokeResult<AppUser>.FromInvokeResult(createResult);
        }

        private async Task<InvokeResult> EnsureSubscriptionAsync(ProvisionalEnvironment environment, Organization organization, AppUser appUser)
        {
            var org = organization.ToEntityHeader();
            var user = EntityHeader.Create(appUser.Id, appUser.UserName);
            var existing = await _subscriptionManager.GetSubscriptionAsync(environment.SubscriptionId, org, user);
            if (existing != null) return InvokeResult.Success;

            var subscriptionLevel = await _subscriptionLevelManager.GetSubscriptionLevelByKeyAsync(Subscription.SubscriptionKey_Provisional);
            if (subscriptionLevel == null) return InvokeResult.FromError("The provisional subscription level is not configured.");

            var subscription = new Subscription
            {
                Id = environment.SubscriptionId,
                Key = Subscription.SubscriptionKey_Provisional,
                Name = "Provisional",
                Description = "Provisional working environment",
                SubscriptionLevel = EntityHeader.Create(subscriptionLevel.Id.ToString(), subscriptionLevel.Name),
                Start = CalendarDate.Today(),
                ActiveDate = CalendarDate.Today(),
                IsActive = true,
                IsTrial = false
            };

            return await _subscriptionManager.AddSubscriptionAsync(subscription, org, user);
        }

        private static string CreateRecoveryToken()
        {
            var bytes = new byte[RecoveryTokenBytes];
            RandomNumberGenerator.Fill(bytes);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", String.Empty);
        }

        private static string HashOptional(string value)
        {
            return String.IsNullOrWhiteSpace(value) ? null : Hash(value);
        }

        private static string Hash(string value)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                var builder = new StringBuilder(bytes.Length * 2);
                foreach (var item in bytes) builder.Append(item.ToString("x2"));
                return builder.ToString();
            }
        }
    }
}
