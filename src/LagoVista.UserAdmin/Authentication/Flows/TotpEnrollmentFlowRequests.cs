using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public sealed class TotpEnrollmentBeginFlowRequest
    {
        public TotpEnrollmentBeginFlowRequest(string userId, EntityHeader organization, EntityHeader user)
        {
            UserId = String.IsNullOrWhiteSpace(userId) ? throw new ArgumentNullException(nameof(userId)) : userId;
            Organization = organization;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public string UserId { get; }
        public EntityHeader Organization { get; }
        public EntityHeader User { get; }
    }

    public sealed class TotpEnrollmentConfirmFlowRequest
    {
        public TotpEnrollmentConfirmFlowRequest(string userId, string totp, EntityHeader organization, EntityHeader user)
        {
            UserId = String.IsNullOrWhiteSpace(userId) ? throw new ArgumentNullException(nameof(userId)) : userId;
            Totp = totp;
            Organization = organization;
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public string UserId { get; }
        public string Totp { get; }
        public EntityHeader Organization { get; }
        public EntityHeader User { get; }
    }
}
