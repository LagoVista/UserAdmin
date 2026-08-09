using LagoVista.Core.Validation;
using LagoVista.UserAdmin.Interfaces.Managers;
using LagoVista.UserAdmin.Interfaces.Repos.Users;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    public class ContinuityConversationManager : IContinuityConversationManager
    {
        public const int MaximumMessageLength = 500;
        public const string OfferPromotionDirective = "offerPromotion";

        private const string VisitorIdentityStage = "visitor";

        private static readonly string[] PromotionPhrases =
        {
            "create my workspace",
            "save my work",
            "i'm ready"
        };

        private static readonly string[] Responses =
        {
            "The navigation computer has filed your request under 'probably intentional.'",
            "A nearby moon supports this plan, although it has not read the fine print.",
            "Excellent. The ship has stopped panicking and begun taking notes.",
            "That answer has been forwarded to the Department of Improbable Logistics.",
            "The dashboard agrees, which is unusual and mildly suspicious.",
            "A small committee of robots has declared this direction mostly sensible.",
            "Your message arrived safely, despite a brief disagreement with gravity.",
            "The onboard oracle says yes, then asks whether snacks are included.",
            "That seems workable across at least three known dimensions.",
            "The engines are warming up and pretending they understood everything.",
            "A distant civilization just gave this idea an encouraging thumbs-up.",
            "The probability meter blinked green, apologized, and blinked green again.",
            "Noted. I have hidden the paperwork where bureaucracy cannot find it.",
            "The ship recommends proceeding before the universe changes its mind.",
            "That has the unmistakable shape of a plan with adequate oxygen.",
            "Your request is now orbiting the correct problem.",
            "The autopilot calls this bold; the toaster calls it Tuesday.",
            "We can do that, assuming spacetime remains approximately cooperative.",
            "The cosmic help desk has marked this ticket delightfully actionable.",
            "Consider it recorded in ink that is certified meteor-resistant."
        };

        private readonly IContinuityConversationRepo _conversationRepo;

        public ContinuityConversationManager(IContinuityConversationRepo conversationRepo)
        {
            _conversationRepo = conversationRepo ?? throw new ArgumentNullException(nameof(conversationRepo));
        }

        public async Task<InvokeResult<ContinuityConversationResponse>> GetAsync(string actorId)
        {
            if (String.IsNullOrWhiteSpace(actorId)) return InvokeResult<ContinuityConversationResponse>.FromError("ActorId is required.");

            var messages = await _conversationRepo.GetAsync(actorId);
            return InvokeResult<ContinuityConversationResponse>.Create(new ContinuityConversationResponse { Messages = messages.ToList() });
        }

        public async Task<InvokeResult<ContinuityConversationResponse>> SendAsync(string actorId, string identityStage, ContinuityConversationMessageRequest request)
        {
            if (String.IsNullOrWhiteSpace(actorId)) return InvokeResult<ContinuityConversationResponse>.FromError("ActorId is required.");
            if (request == null || String.IsNullOrWhiteSpace(request.Message)) return InvokeResult<ContinuityConversationResponse>.FromError("Message is required.");

            var text = request.Message.Trim();
            if (text.Length > MaximumMessageLength) return InvokeResult<ContinuityConversationResponse>.FromError($"Message cannot exceed {MaximumMessageLength} characters.");

            var now = DateTime.UtcNow;
            var messages = new[]
            {
                CreateMessage(ContinuityConversationRoles.User, text, now),
                CreateMessage(ContinuityConversationRoles.Assistant, Responses[RandomNumberGenerator.GetInt32(Responses.Length)], now.AddTicks(1))
            };

            await _conversationRepo.AppendAsync(actorId, messages);
            var transcript = await _conversationRepo.GetAsync(actorId);

            return InvokeResult<ContinuityConversationResponse>.Create(new ContinuityConversationResponse
            {
                Messages = transcript.ToList(),
                Directive = ShouldOfferPromotion(identityStage, text) ? OfferPromotionDirective : null
            });
        }

        public async Task<InvokeResult> ClearAsync(string actorId)
        {
            if (String.IsNullOrWhiteSpace(actorId)) return InvokeResult.FromError("ActorId is required.");
            await _conversationRepo.ClearAsync(actorId);
            return InvokeResult.Success;
        }

        private static ContinuityConversationMessage CreateMessage(string role, string text, DateTime createdUtc)
        {
            return new ContinuityConversationMessage { Id = Guid.NewGuid().ToString("N"), Role = role, Text = text, CreatedUtc = createdUtc };
        }

        private static bool ShouldOfferPromotion(string identityStage, string message)
        {
            return String.Equals(identityStage, VisitorIdentityStage, StringComparison.Ordinal) && PromotionPhrases.Any(phrase => message.IndexOf(phrase, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
}
