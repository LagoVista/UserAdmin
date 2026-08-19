using LagoVista.Core.Models;
using System;

namespace LagoVista.UserAdmin.Authentication.Flows
{
    public enum TotpManagementOperation
    {
        TurnOff = 0,
        RotateRecoveryCodes = 1
    }

    public class TotpManagementFlowRequest
    {
        public TotpManagementFlowRequest(string userId, TotpManagementOperation operation, EntityHeader organization, EntityHeader user)
        {
            if (String.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
            UserId = userId;
            Operation = operation;
            Organization = organization ?? throw new ArgumentNullException(nameof(organization));
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public string UserId { get; }
        public TotpManagementOperation Operation { get; }
        public EntityHeader Organization { get; }
        public EntityHeader User { get; }
    }
}
