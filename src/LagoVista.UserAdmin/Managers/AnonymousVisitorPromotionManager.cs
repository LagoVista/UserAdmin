using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class AnonymousVisitorPromotionManager : IAnonymousVisitorPromotionManager
    {
        private const string IdentityStage = "provisional";
        private const string CreationRequestPrefix = "anonymous-visitor:";

        private readonly IAnonymousVisitorRepo _visitorRepo;
        private readonly IProvisionalEnvironmentManager _provisionalEnvironmentManager;
        private readonly IAnonymousVisitorPromotionOptions _options;

        public AnonymousVisitorPromotionManager(IAnonymousVisitorRepo visitorRepo, IProvisionalEnvironmentManager provisionalEnvironmentManager, IAnonymousVisitorPromotionOptions options)
        {
            _visitorRepo = visitorRepo ?? throw new ArgumentNullException(nameof(visitorRepo));
            _provisionalEnvironmentManager = provisionalEnvironmentManager ?? throw new ArgumentNullException(nameof(provisionalEnvironmentManager));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<InvokeResult<AnonymousVisitorPromotionResponse>> PromoteAsync(string actorId, string ipAddress, AnonymousVisitorPromotionRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (String.IsNullOrWhiteSpace(actorId)) return InvokeResult<AnonymousVisitorPromotionResponse>.FromError("ActorId is required.");
            if (!request.TermsAndConditionsAccepted) return InvokeResult<AnonymousVisitorPromotionResponse>.FromError("Terms and conditions must be accepted before promotion.");
            if (String.IsNullOrWhiteSpace(_options.TermsAndConditionsVersion)) return InvokeResult<AnonymousVisitorPromotionResponse>.FromError("AnonymousVisitor:TermsAndConditionsVersion is not configured.");
            if (!String.Equals(request.TermsAndConditionsVersion, _options.TermsAndConditionsVersion, StringComparison.Ordinal)) return InvokeResult<AnonymousVisitorPromotionResponse>.FromError("The current terms and conditions must be accepted before promotion.");

            var visitor = await _visitorRepo.GetByActorIdAsync(actorId);
            if (visitor == null) return InvokeResult<AnonymousVisitorPromotionResponse>.FromError("The anonymous visitor was not found.");
            if (visitor.State == AnonymousVisitorState.Expired) return InvokeResult<AnonymousVisitorPromotionResponse>.FromError("The anonymous visitor has expired.");
            if (visitor.State != AnonymousVisitorState.Active && visitor.State != AnonymousVisitorState.Promoted)
                return InvokeResult<AnonymousVisitorPromotionResponse>.FromError($"Anonymous visitor is {visitor.State.ToString().ToLowerInvariant()}.");

            var now = DateTime.UtcNow;
            if (visitor.State == AnonymousVisitorState.Active && visitor.ExpiresUtc.ToUniversalTime() <= now)
            {
                visitor.State = AnonymousVisitorState.Expired;
                visitor.ExpiredUtc = now;
                visitor.StateChangedUtc = now;
                await _visitorRepo.UpdateAsync(visitor);
                return InvokeResult<AnonymousVisitorPromotionResponse>.FromError("The anonymous visitor has expired.");
            }

            var createResult = await _provisionalEnvironmentManager.CreateAsync(new CreateProvisionalEnvironmentRequest
            {
                CreationRequestId = CreateCreationRequestId(visitor.ActorId),
                OriginActorId = visitor.ActorId,
                InstallationId = request.InstallationId,
                BootstrapContext = visitor.BootstrapContext,
                TermsAndConditionsAccepted = true,
                TermsAndConditionsVersion = _options.TermsAndConditionsVersion,
                TermsAndConditionsAcceptedIPAddress = ipAddress,
                ConversionJourneyId = visitor.ConversionJourneyId,
                AcquisitionSourceKey = visitor.AcquisitionSourceKey,
                CampaignKey = visitor.CampaignKey,
                EntryPointType = visitor.EntryPointType,
                EntryPointKey = visitor.EntryPointKey,
                ExperimentKey = visitor.ExperimentKey,
                ExperimentVariantKey = visitor.ExperimentVariantKey,
                AgentKey = visitor.AgentKey,
                AgentVersion = visitor.AgentVersion,
                PromptVersion = visitor.PromptVersion
            });

            if (!createResult.Successful) return InvokeResult<AnonymousVisitorPromotionResponse>.FromInvokeResult(createResult.ToInvokeResult());

            var environment = createResult.Result;
            if (visitor.State == AnonymousVisitorState.Promoted && !String.Equals(visitor.ProvisionalEnvironmentId, environment.ProvisionalEnvironmentId, StringComparison.Ordinal))
                return InvokeResult<AnonymousVisitorPromotionResponse>.FromError("The anonymous visitor is linked to a different provisional environment.");

            if (visitor.State != AnonymousVisitorState.Promoted)
            {
                visitor.State = AnonymousVisitorState.Promoted;
                visitor.ProvisionalEnvironmentId = environment.ProvisionalEnvironmentId;
                visitor.PromotedUtc = now;
                visitor.StateChangedUtc = now;
                visitor.LastActivityUtc = now;
                visitor.ContinuityTokenHash = null;
                visitor.InstallationIdHash = null;
                try
                {
                    await _visitorRepo.UpdateAsync(visitor);
                }
                catch
                {
                    var persistedVisitor = await _visitorRepo.GetByActorIdAsync(actorId);
                    if (persistedVisitor == null || persistedVisitor.State != AnonymousVisitorState.Promoted || !String.Equals(persistedVisitor.ProvisionalEnvironmentId, environment.ProvisionalEnvironmentId, StringComparison.Ordinal)) throw;
                    visitor = persistedVisitor;
                }
            }

            return InvokeResult<AnonymousVisitorPromotionResponse>.Create(new AnonymousVisitorPromotionResponse
            {
                ActorId = visitor.ActorId,
                IdentityStage = IdentityStage,
                ProvisionalEnvironmentId = environment.ProvisionalEnvironmentId,
                AppUserId = environment.AppUserId,
                OrganizationId = environment.OrganizationId,
                SubscriptionId = environment.SubscriptionId,
                RecoveryToken = environment.RecoveryToken,
                ExpiresUtc = environment.ExpiresUtc,
                BootstrapContext = visitor.BootstrapContext,
                WasResumed = environment.WasResumed
            });
        }

        private static string CreateCreationRequestId(string actorId)
        {
            return $"{CreationRequestPrefix}{actorId}";
        }
    }
}
