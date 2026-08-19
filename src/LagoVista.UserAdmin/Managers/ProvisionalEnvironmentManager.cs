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
using System.Net.Mail;
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
        private readonly IUserVerficationManager _userVerificationManager;
        private readonly IAppUserRepo _appUserRepo;
        private readonly IOrganizationManager _organizationManager;
        private readonly ISubscriptionManager _subscriptionManager;
        private readonly ISubscriptionLevelManager _subscriptionLevelManager;
        private readonly IProvisionalEnvironmentBillingArchiveRepo _billingArchiveRepo;
        private readonly IProvisionalEnvironmentArchiveStore _archiveStore;
        private readonly IProvisionalEnvironmentArchiveAccountingService _archiveAccountingService;

        public ProvisionalEnvironmentManager(IProvisionalEnvironmentRepo environmentRepo, IUserManager userManager, IUserVerficationManager userVerificationManager, IAppUserRepo appUserRepo, IOrganizationManager organizationManager, ISubscriptionManager subscriptionManager, ISubscriptionLevelManager subscriptionLevelManager, IProvisionalEnvironmentBillingArchiveRepo billingArchiveRepo, IProvisionalEnvironmentArchiveStore archiveStore, IProvisionalEnvironmentArchiveAccountingService archiveAccountingService)
        {
            _environmentRepo = environmentRepo ?? throw new ArgumentNullException(nameof(environmentRepo));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _userVerificationManager = userVerificationManager ?? throw new ArgumentNullException(nameof(userVerificationManager));
            _appUserRepo = appUserRepo ?? throw new ArgumentNullException(nameof(appUserRepo));
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
            if ((request.BootstrapContext?.Length ?? 0) > AnonymousVisitor.MaximumBootstrapContextLength) return InvokeResult<CreateProvisionalEnvironmentResponse>.FromError($"BootstrapContext cannot exceed {AnonymousVisitor.MaximumBootstrapContextLength} characters.");
            if (request.TermsAndConditionsAccepted && String.IsNullOrWhiteSpace(request.TermsAndConditionsVersion)) return InvokeResult<CreateProvisionalEnvironmentResponse>.FromError("TermsAndConditionsVersion is required when terms and conditions are accepted.");

            var recoveryToken = CreateRecoveryToken();
            var environment = await _environmentRepo.FindByCreationRequestIdAsync(request.CreationRequestId);
            var wasResumed = environment != null;
            var createdEnvironment = false;

            if (environment == null)
            {
                var now = DateTime.UtcNow;
                environment = new ProvisionalEnvironment
                {
                    Id = Guid.NewGuid().ToId(),
                    State = ProvisionalEnvironmentState.Provisioning,
                    CreationRequestId = request.CreationRequestId,
                    OriginActorId = request.OriginActorId,
                    AppUserId = Guid.NewGuid().ToId(),
                    OrganizationId = Guid.NewGuid().ToId(),
                    SubscriptionId = Guid.NewGuid().ToString("D"),
                    RecoveryTokenHash = Hash(recoveryToken),
                    InstallationIdHash = HashOptional(request.InstallationId),
                    BootstrapContext = request.BootstrapContext,
                    TermsAndConditionsAccepted = request.TermsAndConditionsAccepted,
                    TermsAndConditionsVersion = request.TermsAndConditionsVersion,
                    TermsAndConditionsAcceptedIPAddress = request.TermsAndConditionsAcceptedIPAddress,
                    TermsAndConditionsAcceptedUtc = request.TermsAndConditionsAccepted ? (DateTime?)now : null,
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
                    createdEnvironment = true;
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
                if (environment.State == ProvisionalEnvironmentState.Provisioning)
                {
                    var normalizedSubscriptionId = NormalizeSubscriptionId(environment.SubscriptionId);
                    if (normalizedSubscriptionId == null)
                        return InvokeResult<CreateProvisionalEnvironmentResponse>.FromError("The provisional environment subscription ID is invalid.");

                    environment.SubscriptionId = normalizedSubscriptionId;
                }

                if (String.IsNullOrWhiteSpace(environment.OriginActorId)) environment.OriginActorId = request.OriginActorId;
                environment.RecoveryTokenHash = Hash(recoveryToken);
                if (!String.IsNullOrWhiteSpace(request.InstallationId)) environment.InstallationIdHash = Hash(request.InstallationId);
                await _environmentRepo.UpdateAsync(environment);
            }

            var appUser = createdEnvironment
                ? await CreateFreshUserAsync(environment)
                : await EnsureUserAsync(environment);
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
                ActorId = environment.OriginActorId ?? environment.AppUserId,
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
            if (String.IsNullOrWhiteSpace(request.RecoveryToken)) return InvokeResult<RestoreProvisionalEnvironmentResponse>.FromError("A recovery token is required.");

            var environment = await _environmentRepo.FindByRecoveryTokenHashAsync(Hash(request.RecoveryToken));
            if (environment == null) return InvokeResult<RestoreProvisionalEnvironmentResponse>.FromError("The provisional environment could not be restored.");
            if (environment.State != ProvisionalEnvironmentState.Active) return InvokeResult<RestoreProvisionalEnvironmentResponse>.FromError($"Provisional environment is {environment.State.ToString().ToLowerInvariant()}.");

            var now = DateTime.UtcNow;
            if (environment.ExpiresUtc.ToUniversalTime() <= now)
            {
                environment.State = ProvisionalEnvironmentState.Expired;
                environment.ExpiredUtc = now;
                environment.PurgeAfterUtc = now.AddDays(ExpiredRetentionDays);
                environment.StateChangedUtc = now;
                await _environmentRepo.UpdateAsync(environment);
                return InvokeResult<RestoreProvisionalEnvironmentResponse>.FromError("The provisional environment has expired.");
            }

            var recoveryToken = CreateRecoveryToken();
            environment.RecoveryTokenHash = Hash(recoveryToken);
            environment.LastActivityUtc = now;
            environment.ExpiresUtc = now.AddDays(ActiveLifetimeDays);
            await _environmentRepo.UpdateAsync(environment);

            return InvokeResult<RestoreProvisionalEnvironmentResponse>.Create(ToRestoreResponse(environment, recoveryToken));
        }

        public async Task<InvokeResult> RecordActivityAsync(string provisionalEnvironmentId)
        {
            if (String.IsNullOrWhiteSpace(provisionalEnvironmentId)) return InvokeResult.FromError("ProvisionalEnvironmentId is required.");

            var environment = await _environmentRepo.GetByIdAsync(provisionalEnvironmentId);
            if (environment == null) return InvokeResult.FromError("The provisional environment was not found.");

            return await RecordActivityAsync(environment);
        }

        public async Task<InvokeResult<EstablishProvisionalAccountResponse>> EstablishAccountAsync(EstablishProvisionalAccountRequest request, string appUserId)
        {
            if (request == null) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("Account establishment request is required.");
            if (String.IsNullOrWhiteSpace(request.ProvisionalEnvironmentId)) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("ProvisionalEnvironmentId is required.");
            if (String.IsNullOrWhiteSpace(appUserId)) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("AppUserId is required.");
            if (String.IsNullOrWhiteSpace(request.FirstName)) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("FirstName is required.");
            if (String.IsNullOrWhiteSpace(request.LastName)) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("LastName is required.");
            if (String.IsNullOrWhiteSpace(request.Email)) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("Email is required.");
            if (String.IsNullOrWhiteSpace(request.Password)) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("Password is required.");

            var firstName = request.FirstName.Trim();
            var lastName = request.LastName.Trim();
            var email = request.Email.Trim();

            try
            {
                if (!String.Equals(new MailAddress(email).Address, email, StringComparison.OrdinalIgnoreCase))
                    return InvokeResult<EstablishProvisionalAccountResponse>.FromError("Email is invalid.");
            }
            catch (FormatException)
            {
                return InvokeResult<EstablishProvisionalAccountResponse>.FromError("Email is invalid.");
            }

            var environment = await _environmentRepo.GetByIdAsync(request.ProvisionalEnvironmentId);
            if (environment == null) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("The provisional environment was not found.");
            if (!String.Equals(environment.AppUserId, appUserId, StringComparison.Ordinal)) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("The provisional environment does not belong to the current user.");
            if (environment.State != ProvisionalEnvironmentState.Active) return InvokeResult<EstablishProvisionalAccountResponse>.FromError($"Provisional environment is {environment.State.ToString().ToLowerInvariant()}.");
            if (environment.ExpiresUtc.ToUniversalTime() <= DateTime.UtcNow) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("The provisional environment has expired.");

            var appUser = await _userManager.FindByIdAsync(appUserId);
            if (appUser == null) return InvokeResult<EstablishProvisionalAccountResponse>.FromError("The provisional environment user was not found.");

            if (!appUser.IsAnonymous)
            {
                if (!String.Equals(appUser.Email, email, StringComparison.OrdinalIgnoreCase))
                    return InvokeResult<EstablishProvisionalAccountResponse>.FromError("The provisional account has already been established with a different email address.");

                return await SendAccountVerificationAsync(environment, appUser);
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null && !String.Equals(existingUser.Id, appUser.Id, StringComparison.Ordinal))
                return InvokeResult<EstablishProvisionalAccountResponse>.FromError("An account already exists for this email address.");

            appUser.FirstName = firstName;
            appUser.LastName = lastName;
            appUser.Email = email;
            appUser.UserName = email;
            appUser.LoginType = LoginTypes.AppUser;
            appUser.IsAnonymous = false;
            appUser.EmailConfirmed = false;
            appUser.HasGeneratedPassword = false;
            appUser.ShowWelcome = false;
            appUser.LastUpdatedBy = EntityHeader.Create(appUser.Id, $"{firstName} {lastName}");
            appUser.LastUpdatedDate = UtcTimestamp.Now;

            var passwordResult = await _userManager.AddPasswordAsync(appUser, request.Password);
            if (!passwordResult.Successful) return InvokeResult<EstablishProvisionalAccountResponse>.FromInvokeResult(passwordResult);

            return await SendAccountVerificationAsync(environment, appUser);
        }

        private async Task<InvokeResult<EstablishProvisionalAccountResponse>> SendAccountVerificationAsync(ProvisionalEnvironment environment, AppUser appUser)
        {
            if (appUser.EmailConfirmed)
            {
                return InvokeResult<EstablishProvisionalAccountResponse>.Create(ToEstablishAccountResponse(environment, appUser, null));
            }

            var verificationResult = await _userVerificationManager.SendConfirmationEmailAsync(appUser);
            if (!verificationResult.Successful) return InvokeResult<EstablishProvisionalAccountResponse>.FromInvokeResult(verificationResult.ToInvokeResult());

            return InvokeResult<EstablishProvisionalAccountResponse>.Create(ToEstablishAccountResponse(environment, appUser, verificationResult.Result));
        }

        private static EstablishProvisionalAccountResponse ToEstablishAccountResponse(ProvisionalEnvironment environment, AppUser appUser, string developmentVerificationCode)
        {
            return new EstablishProvisionalAccountResponse
            {
                ProvisionalEnvironmentId = environment.Id,
                AppUserId = appUser.Id,
                OrganizationId = environment.OrganizationId,
                Email = appUser.Email,
                EmailVerificationRequired = !appUser.EmailConfirmed,
                DevelopmentVerificationCode = developmentVerificationCode
            };
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
            if (!appUser.EmailConfirmed) return InvokeResult.FromError("The provisional environment user must verify their email before the environment can be claimed.");

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
                TermsAndConditionsAccepted = environment.TermsAndConditionsAccepted,
                TermsAndConditionsVersion = environment.TermsAndConditionsVersion,
                TermsAndConditionsAcceptedIPAddress = environment.TermsAndConditionsAcceptedIPAddress,
                TermsAndConditionsAcceptedUtc = environment.TermsAndConditionsAcceptedUtc,
                BillingEventCount = billingEvents.Count,
                TotalActualCost = billingEvents.Sum(item => item.ActualCost ?? 0m),
                TotalExtended = billingEvents.Sum(item => item.Extended ?? 0m),
                TotalTokens = billingEvents.Sum(item => item.Tokens ?? 0L),
                TotalQuantity = billingEvents.Sum(item => item.Quantity ?? 0m),
                EarliestBillingEventUtc = billingEvents.Count == 0 ? (DateTime?)null : billingEvents.Min(item => item.StartTimestamp),
                LatestBillingEventUtc = billingEvents.Count == 0 ? (DateTime?)null : billingEvents.Max(item => item.EndTimestamp ?? item.StartTimestamp)
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

        private static RestoreProvisionalEnvironmentResponse ToRestoreResponse(ProvisionalEnvironment environment, string recoveryToken)
        {
            return new RestoreProvisionalEnvironmentResponse
            {
                ActorId = environment.OriginActorId ?? environment.AppUserId,
                ProvisionalEnvironmentId = environment.Id,
                AppUserId = environment.AppUserId,
                OrganizationId = environment.OrganizationId,
                SubscriptionId = environment.SubscriptionId,
                RecoveryToken = recoveryToken,
                ExpiresUtc = environment.ExpiresUtc,
                BootstrapContext = environment.BootstrapContext
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

        private async Task<InvokeResult<AppUser>> CreateFreshUserAsync(ProvisionalEnvironment environment)
        {
            var appUser = CreateProvisionalUser(environment);
            await _appUserRepo.CreateAsync(appUser);
            await _appUserRepo.EnsureRelationalUserAsync(appUser);
            return InvokeResult<AppUser>.Create(appUser);
        }

        private async Task<InvokeResult<AppUser>> EnsureUserAsync(ProvisionalEnvironment environment)
        {
            var appUser = await _userManager.FindByIdAsync(environment.AppUserId);
            if (appUser != null)
            {
                await _appUserRepo.EnsureRelationalUserAsync(appUser);
                return InvokeResult<AppUser>.Create(appUser);
            }

            appUser = CreateProvisionalUser(environment);

            var createResult = await _userManager.CreateAsync(appUser);
            if (!createResult.Successful) return InvokeResult<AppUser>.FromInvokeResult(createResult);

            await _appUserRepo.EnsureRelationalUserAsync(appUser);
            return InvokeResult<AppUser>.Create(appUser);
        }

        private static AppUser CreateProvisionalUser(ProvisionalEnvironment environment)
        {
            return new AppUser(null, $"provisional-{environment.AppUserId}", "Provisional Environment")
            {
                Id = environment.AppUserId,
                CreatedBy = EntityHeader.Create(environment.AppUserId, "Provisional Environment"),
                LastUpdatedBy = EntityHeader.Create(environment.AppUserId, "Provisional Environment"),
                SecurityStamp = Guid.NewGuid().ToString("N"),
                IsAnonymous = true,
                ShowWelcome = false,
                TermsAndConditionsAccepted = environment.TermsAndConditionsAccepted,
                TermsAndConditionsAcceptedDateTime = environment.TermsAndConditionsAcceptedUtc?.ToString("O"),
                TermsAndConditionsAcceptedIPAddress = environment.TermsAndConditionsAcceptedIPAddress
            };
        }

        private async Task<InvokeResult> EnsureSubscriptionAsync(ProvisionalEnvironment environment, Organization organization, AppUser appUser)
        {
            var org = organization.ToEntityHeader();
            var user = EntityHeader.Create(appUser.Id, appUser.UserName);

            var subscriptionLevelResult = await _subscriptionLevelManager.EnsureSystemSubscriptionLevelAsync(SystemSubscriptionLevels.CreateProvisional());
            if (!subscriptionLevelResult.Successful) return subscriptionLevelResult.ToInvokeResult();
            var subscriptionLevel = subscriptionLevelResult.Result;

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

            return await _subscriptionManager.EnsureProvisionalSubscriptionAsync(subscription, org, user);
        }

        private static string NormalizeSubscriptionId(string subscriptionId)
        {
            if (String.IsNullOrWhiteSpace(subscriptionId)) return Guid.NewGuid().ToString("D");
            return Guid.TryParse(subscriptionId, out var parsed) ? parsed.ToString("D") : null;
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