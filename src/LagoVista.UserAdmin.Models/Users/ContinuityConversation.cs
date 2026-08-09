using System;
using System.Collections.Generic;

namespace LagoVista.UserAdmin.Models.Users
{
    public static class ContinuityConversationRoles
    {
        public const string User = "user";
        public const string Assistant = "assistant";
    }

    public class ContinuityConversationMessage
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string Text { get; set; }
        public DateTime CreatedUtc { get; set; }
    }

    public class ContinuityConversationMessageRequest
    {
        public string Message { get; set; }
    }

    public class ContinuityConversationResponse
    {
        public List<ContinuityConversationMessage> Messages { get; set; } = new List<ContinuityConversationMessage>();
        public string Directive { get; set; }
    }
}
