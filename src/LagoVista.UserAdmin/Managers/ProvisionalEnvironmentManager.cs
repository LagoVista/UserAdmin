using LagoVista.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class ProvisionalEnvironmentManager : IProvisionalEnvironmentManager
    {
        private const int RecoveryTokenBytes = 32;
        private const int ActiveLifetimeDays = 30;
        private const int ExpiredRetentionDays = 30;
        private const int MaximumLifecycleBatchSize = 500;

        private readonly IProvisionalEnvironmentRepo _environmentRepo;
        private readonly IUserManager _userManager;
        private readonly IOrganizationManager _organizationManager;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ISubscriptionLevelManager _subscriptionLevelManager;
        private readonly IProvisionalEnvironmentBillingArchiveRepo _billingArchiveRepo;
        private readonly IProvisionalEnvironmentArchiveStore _archiveStore;
        private readonly IProvisionalEnvironmentArchiveAccountingService _archiveAccountingService;

        public ProvisionalEnvironmentManager(IProvisionalEnvironmentRepo environmentRepo, IUserManager userManager, IOrganizationManager organizationManager, ISubscriptionManager subscriptionManager, ISubscriptionLevelManager subscriptionLevelManager, IProvisionalEnvironmentBillingArchiveRepo billingArchiveRepo, IProvisionalEnvironmentArchiveStore archiveStore, IProvisionalEnvironmentArchiveAccountingService archiveAccountingService)
        {
            _environmentRepo = environmentRepo ?? throw new ArgumentNullException(nameof(environmentRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _organizationManager = organizationManager ?? throw new ArgumentNullException(nameof(organizationManager));
            _subscriptionManager = subscriptionManager ?? throw new ArgumentNullException(nameof(subscriptionManager));
            _subscriptionLevelManager = subscriptionLevelManager ?? throw new ArgumentNullException(nameof(subscriptionLevelManager));
            _billingArchiveRepo = billingArchiveRepo ?? throw new ArgumentNullException(nameof(billingArchiveRepo));
            _archiveStore = archiveStore ?? throw new ArgumentNullException(nameof(archiveStore));
            _archiveAccountingService = archiveAccountingService ?? throw new ArgumentNullException(nameof(archiveAccountingService));
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

        public async Task<InvokeResult<RestoreProvisionalEnvironmentResponse>> RestoreAsync(RestoreProvisionalEnvironmentRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var hasRecoveryToken = !String.IsNullOrWhiteSpace(request.RecoveryToken);
            var hasInstallationId = !String.IsNullOrWhiteSpace(request.InstallationId);
            if (!hasRecoveryToken && !hasInstallationId) return InvokeResult<RestoreProvisionalEnvironmentResponse>.FromError("A recovery token or installation ID is required.");

            var recoveryEnvironment = hasRecoveryToken ? await _environmentRepo.FindByRecoveryTokenHashAsync(Hash(request.RecoveryToken)) : null;
            var installationEnvironment = hasInstallationId ? await _environmentRepo.FindByInstallationIdHashAsync(Hash(request.InstallationId)) : null;

            if (hasRecoveryToken && hasInstallationId && (recoveryEnvironment == null || installationEnvironment == null || !String.Equals(recoveryEnvironment.Id, installationEnvironment.Id, StringComparison.Ordinal)))
                return InvokeResult<RestoreProvisionalEnvironmentResponse>.FromError("The supplied continuity credentials do not identify the same provisional environment.");

            var environment = recoveryEnvironment ?? installationEnvironment;
            if (environment == null) return InvokeResult<RestoreProvisionalEnvironmentResponse>.FromError("The provisional environment could not be restored.");

            var activityResult = await RecordActivityAsync(environment);
            if (!activityResult.Successful) return InvokeResult<RestoreProvisionalEnvironmentResponse>.FromInvokeResult(activityResult);

            return InvokeResult<RestoreProvisionalEnvironmentResponse>.Create(ToRestoreResponse(environment));
        }

        public async Task<InvokeResult> RecordActivityAsync(string provisionalEnvironmentId)
        {
            if (String.IsNullOrWhiteSpace(provisionalEnvironmentId)) return InvokeResult.FromError("ProvisionalEnvironmentId is required.");

            var environment = await _environmentRepo.GetByIdAsync(provisionalEnvironmentId);
            if (environment == null) return InvokeResult.FromError("The provisional environment was not found.");

            return await RecordActivityAsync(environment);
        }

        public async Task<InvokeResult> ClaimAsync(string provisionalEnvironmentId, string appUserId)
        {
            if (String.IsNullOrWhiteSpace(provisionalEnvironmentId)) return InvokeResult.FromError("ProvisionalEnvironmentId is required.");
            if (String.IsNullOrWhiteSpace(appUserId)) return InvokeResult.FromError("AppUserId is required.");

            var environment = await _environmentRepo.GetByIdAsync(provisionalEnvironmentId);
            if (environment == null) return InvokeResult.FromError("The provisional environment was not found.");
            if (!String.Equals(environment.AppUserId, appUserId, StringComparison.Ordinal)) return InvokeResult.FromError("The provisional environment does not belong to the current user.");
            if (environment.State == ProvisionalEnvironmentState.Claimed) return InvokeResult.Success;
            if (environment.State != ProvisionalEnvironmentState.Active) return InvokeResult.FromError($"Provisional environment is {environment.State.ToString().ToLowerInvariant()}.");

            var now = DateTime.UtcNow;
            if (environment.ExpiresUtc.ToUniversalTime() <= now)
            {
                environment.State = ProvisionalEnvironmentState.Expired;
                environment.ExpiredUtc = now;
                environment.PurgeAfterUtc = now.AddDays(ExpiredRetentionDays);
                environment.StateChangedUtc = now;
                await _environmentRepo.UpdateAsync(environment);
                return InvokeResult.FromError("The provisional environment has expired.");
            }

            var appUser = await _userManager.FindByIdAsync(environment.AppUserId);
            if (appUser == null) return InvokeResult.FromError("The provisional environment user was not found.");
            if (appUser.IsAnonymous) return InvokeResult.FromError("The provisional environment user must be established before the environment can be claimed.");

            environment.State = ProvisionalEnvironmentState.Claimed;
            environment.ClaimedUtc = now;
            environment.StateChangedUtc = now;
            environment.RecoveryTokenHash = null;
            environment.InstallationIdHash = null;
            await _environmentRepo.UpdateAsync(environment);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult<IEnumerable<ProvisionalEnvironmentLifecycleSummary>>> GetByStateAsync(ProvisionalEnvironmentState state, DateTime? dueBeforeUtc = null, int take = 100)
        {
            var takeResult = ValidateTake(take);
            if (!takeResult.Successful) return InvokeResult<IEnumerable<ProvisionalEnvironmentLifecycleSummary>>.FromInvokeResult(takeResult);

            var environments = await _environmentRepo.GetByStateAsync(state, dueBeforeUtc?.ToUniversalTime(), take);
            return InvokeResult<IEnumerable<ProvisionalEnvironmentLifecycleSummary>>.Create(environments.Select(ToLifecycleSummary).ToList());
        }

        public async Task<InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>> ExpireAsync(DateTime? asOfUtc = null, int take = 100)
        {
            var takeResult = ValidateTake(take);
            if (!takeResult.Successful) return InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>.FromInvokeResult(takeResult);

            var now = DateTime.UtcNow;
            var cutoffUtc = (asOfUtc ?? now).ToUniversalTime();
            var environments = (await _environmentRepo.GetByStateAsync(ProvisionalEnvironmentState.Active, cutoffUtc, take)).ToList();
            var result = new ProvisionalEnvironmentLifecycleBatchResult { ExaminedCount = environments.Count };

            foreach (var environment in environments)
            {
                if (environment.State != ProvisionalEnvironmentState.Active || environment.ExpiresUtc.ToUniversalTime() > cutoffUtc) continue;

                environment.State = ProvisionalEnvironmentState.Expired;
                environment.ExpiredUtc = now;
                environment.PurgeAfterUtc = now.AddDays(ExpiredRetentionDays);
                environment.StateChangedUtc = now;
                await _environmentRepo.UpdateAsync(environment);
                result.UpdatedCount++;
                result.ProvisionalEnvironmentIds.Add(environment.Id);
            }

            return InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>.Create(result);
        }

        public async Task<InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>> PrepareForPurgeAsync(DateTime? asOfUtc = null, int take = 100)
        {
            var takeResult = ValidateTake(take);
            if (!takeResult.Successful) return InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>.FromInvokeResult(takeResult);

            var now = DateTime.UtcNow;
            var cutoffUtc = (asOfUtc ?? now).ToUniversalTime();
            var environments = (await _environmentRepo.GetByStateAsync(ProvisionalEnvironmentState.Expired, cutoffUtc, take)).ToList();
            var result = new ProvisionalEnvironmentLifecycleBatchResult { ExaminedCount = environments.Count };

            foreach (var environment in environments)
            {
                if (environment.State != ProvisionalEnvironmentState.Expired || !environment.PurgeAfterUtc.HasValue || environment.PurgeAfterUtc.Value.ToUniversalTime() > cutoffUtc) continue;

                environment.State = ProvisionalEnvironmentState.PurgePending;
                environment.StateChangedUtc = now;
                await _environmentRepo.UpdateAsync(environment);
                result.UpdatedCount++;
                result.ProvisionalEnvironmentIds.Add(environment.Id);
            }

            return InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>.Create(result);
        }

        public async Task<InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>> PurgeAsync(int take = 100)
        {
            var takeResult = ValidateTake(take);
            if (!takeResult.Successful) return InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>.FromInvokeResult(takeResult);

            var environments = (await _environmentRepo.GetByStateAsync(ProvisionalEnvironmentState.PurgePending, null, take)).ToList();
            var result = new ProvisionalEnvironmentLifecycleBatchResult { ExaminedCount = environments.Count };

            foreach (var environment in environments)
            {
                try
                {
                    var validationResult = await _organizationManager.ValidateProvisionalOrganizationForPurgeAsync(environment.OrganizationId, environment.AppUserId, environment.SubscriptionId);
                    if (!validationResult.Successful)
                    {
                        AddPurgeFailure(result, environment.Id, validationResult.Errors.FirstOrDefault()?.Message ?? "The provisional environment is no longer eligible for purge.");
                        continue;
                    }

                    var billingEvents = await _billingArchiveRepo.GetBillingEventsAsync(environment.OrganizationId, environment.SubscriptionId);
                    var archive = await _archiveStore.WriteAndVerifyAsync(new ProvisionalEnvironmentArchiveWriteRequest { Manifest = CreateArchiveManifest(environment, billingEvents), BillingEvents = billingEvents });
                    await _archiveAccountingService.EnsureRollupAsync(new ProvisionalEnvironmentArchiveAccountingRequest { Environment = environment, Archive = archive, BillingEvents = billingEvents });
                    await _billingArchiveRepo.DeleteBillingEventsAsync(environment.OrganizationId, environment.SubscriptionId, billingEvents.Select(item => item.Id).ToList());

                    var purgeResult = await _organizationManager.PurgeProvisionalOrganizationAsync(environment.OrganizationId, environment.AppUserId, environment.SubscriptionId);
                    if (!purgeResult.Successful)
                    {
                        AddPurgeFailure(result, environment.Id, purgeResult.Errors.FirstOrDefault()?.Message ?? "The provisional environment could not be purged.");
                        continue;
                    }

                    await _environmentRepo.DeleteAsync(environment.Id);
                    result.DeletedCount++;
                    result.ProvisionalEnvironmentIds.Add(environment.Id);
                }
                catch (Exception ex)
                {
                    AddPurgeFailure(result, environment.Id, ex.Message);
                }
            }

            return InvokeResult<ProvisionalEnvironmentLifecycleBatchResult>.Create(result);
        }

        private static ProvisionalEnvironmentArchiveManifest CreateArchiveManifest(ProvisionalEnvironment environment, IReadOnlyCollection<ProvisionalEnvironmentBillingEventArchive> billingEvents)
        {
            return new ProvisionalEnvironmentArchiveManifest
            {
                ProvisionalEnvironmentId = environment.Id,
                AppUserId = environment.AppUserId,
                OrganizationId = environment.OrganizationId,
                SubscriptionId = environment.SubscriptionId,
                EstablishedUtc = environment.ActivatedUtc ?? environment.CreatedUtc,
                LastActivityUtc = environment.LastActivityUtc,
                ExpiredUtc = environment.ExpiredUtc,
                ArchivedUtc = DateTime.UtcNow,
                ArchiveReason = "Provisional environment retention period elapsed.",
                ConversionJourneyId = environment.ConversionJourneyId,
                AcquisitionSourceKey = environment.AcquisitionSourceKey,
                CampaignKey = environment.CampaignKey,
                EntryPointType = environment.EntryPointType,
                EntryPointKey = environment.EntryPointKey,
                ExperimentKey = environment.ExperimentKey,
                ExperimentVariantKey = environment.ExperimentVariantKey,
                AgentKey = environment.AgentKey,
                AgentVersion = environment.AgentVersion,
                PromptVersion = environment.PromptVersion,
                BillingEventCount = billingEvents.Count,
                TotalActualCost = billingEvents.Sum(item => item.ActualCost ?? 0m),
                TotalExtended = billingEvents.Sum(item => item.Extended ?? 0m),
                TotalTokens = billingEvents.Sum(item => item.Tokens ?? 0L),
                TotalQuantity = billingEvents.Sum(item => item.Quantity ?? 0m),
                EarliestBillingEventUtc = billingEvents.Count == 0 ? null : billingEvents.Min(item => item.StartTimestamp),
                LatestBillingEventUtc = billingEvents.Count == 0 ? null : billingEvents.Max(item => item.EndTimestamp ?? item.StartTimestamp)
            };
        }

        private static void AddPurgeFailure(ProvisionalEnvironmentLifecycleBatchResult result, string provisionalEnvironmentId, string reason)
        {
            result.BlockedCount++;
            result.Failures.Add(new ProvisionalEnvironmentLifecycleFailure { ProvisionalEnvironmentId = provisionalEnvironmentId, Reason = reason });
        }

        private async Task<InvokeResult> RecordActivityAsync(ProvisionalEnvironment environment)
        {
            if (environment.State != ProvisionalEnvironmentState.Active) return InvokeResult.FromError($"Provisional environment is {environment.State.ToString().ToLowerInvariant()}.");

            var now = DateTime.UtcNow;
            if (environment.ExpiresUtc.ToUniversalTime() <= now)
            {
                environment.State = ProvisionalEnvironmentState.Expired;
                environment.ExpiredUtc = now;
                environment.PurgeAfterUtc = now.AddDays(ExpiredRetentionDays);
                environment.StateChangedUtc = now;
                await _environmentRepo.UpdateAsync(environment);
                return InvokeResult.FromError("The provisional environment has expired.");
            }

            environment.LastActivityUtc = now;
            environment.ExpiresUtc = now.AddDays(ActiveLifetimeDays);
            await _environmentRepo.UpdateAsync(environment);
            return InvokeResult.Success;
        }

        private static RestoreProvisionalEnvironmentResponse ToRestoreResponse(ProvisionalEnvironment environment)
        {
            return new RestoreProvisionalEnvironmentResponse
            {
                ProvisionalEnvironmentId = environment.Id,
                AppUserId = environment.AppUserId,
                OrganizationId = environment.OrganizationId,
                SubscriptionId = environment.SubscriptionId,
                ExpiresUtc = environment.ExpiresUtc
            };
        }

        private static ProvisionalEnvironmentLifecycleSummary ToLifecycleSummary(ProvisionalEnvironment environment)
        {
            return new ProvisionalEnvironmentLifecycleSummary
            {
                ProvisionalEnvironmentId = environment.Id,
                State = environment.State,
                AppUserId = environment.AppUserId,
                OrganizationId = environment.OrganizationId,
                SubscriptionId = environment.SubscriptionId,
                CreatedUtc = environment.CreatedUtc,
                LastActivityUtc = environment.LastActivityUtc,
                ExpiresUtc = environment.ExpiresUtc,
                PurgeAfterUtc = environment.PurgeAfterUtc
            };
        }

        private static InvokeResult ValidateTake(int take)
        {
            if (take <= 0) return InvokeResult.FromError("Take must be greater than zero.");
            if (take > MaximumLifecycleBatchSize) return InvokeResult.FromError($"Take cannot exceed {MaximumLifecycleBatchSize}.");
            return InvokeResult.Success;
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
