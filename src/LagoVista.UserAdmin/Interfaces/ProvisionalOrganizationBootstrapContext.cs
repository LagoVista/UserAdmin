using System;
using System.Threading;

namespace LagoVista.UserAdmin.Interfaces
{
    /// <summary>
    /// Carries one-shot knowledge that a provisional organization was successfully created
    /// in the current async execution flow. Consumers use this only to skip impossible
    /// existence probes immediately following that create. The state does not survive
    /// a retry or a new request.
    /// </summary>
    public static class ProvisionalOrganizationBootstrapContext
    {
        private sealed class BootstrapState
        {
            public string OrganizationId { get; set; }
            public string UserId { get; set; }
            public bool RoleProbeAvailable { get; set; }
            public bool MembershipProbeAvailable { get; set; }
        }

        private static readonly AsyncLocal<BootstrapState> _state = new AsyncLocal<BootstrapState>();

        public static void MarkFresh(string organizationId, string userId)
        {
            if (String.IsNullOrWhiteSpace(organizationId) || String.IsNullOrWhiteSpace(userId)) return;

            _state.Value = new BootstrapState
            {
                OrganizationId = organizationId,
                UserId = userId,
                RoleProbeAvailable = true,
                MembershipProbeAvailable = true
            };
        }

        public static bool TryConsumeRoleProbe(string organizationId, string userId)
        {
            var state = _state.Value;
            if (!Matches(state, organizationId, userId) || !state.RoleProbeAvailable) return false;

            state.RoleProbeAvailable = false;
            ClearWhenConsumed(state);
            return true;
        }

        public static bool TryConsumeMembershipProbe(string organizationId, string userId)
        {
            var state = _state.Value;
            if (!Matches(state, organizationId, userId) || !state.MembershipProbeAvailable) return false;

            state.MembershipProbeAvailable = false;
            ClearWhenConsumed(state);
            return true;
        }

        private static bool Matches(BootstrapState state, string organizationId, string userId)
        {
            return state != null &&
                String.Equals(state.OrganizationId, organizationId, StringComparison.Ordinal) &&
                String.Equals(state.UserId, userId, StringComparison.Ordinal);
        }

        private static void ClearWhenConsumed(BootstrapState state)
        {
            if (!state.RoleProbeAvailable && !state.MembershipProbeAvailable) _state.Value = null;
        }
    }
}
