using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace LagoVista.UserAdmin.Interfaces
{
    /// <summary>
    /// Carries one-shot knowledge that a provisional organization was successfully created
    /// in the current request. Consumers use this only to skip impossible existence probes
    /// immediately following that create. The state is scoped to the current Activity trace
    /// so it cannot be consumed by a retry or a different request.
    /// </summary>
    public static class ProvisionalOrganizationBootstrapContext
    {
        private sealed class BootstrapState
        {
            public string OrganizationId { get; set; }
            public string UserId { get; set; }
            public bool RoleProbeAvailable { get; set; }
            public bool MembershipProbeAvailable { get; set; }
            public DateTime CreatedUtc { get; set; }
        }

        private static readonly ConcurrentDictionary<string, BootstrapState> _states = new ConcurrentDictionary<string, BootstrapState>();
        private static readonly TimeSpan _stateLifetime = TimeSpan.FromMinutes(5);

        public static void MarkFresh(string organizationId, string userId)
        {
            if (String.IsNullOrWhiteSpace(organizationId) || String.IsNullOrWhiteSpace(userId)) return;

            var key = GetKey(organizationId, userId);
            if (key == null) return;

            CleanupExpired();
            _states[key] = new BootstrapState
            {
                OrganizationId = organizationId,
                UserId = userId,
                RoleProbeAvailable = true,
                MembershipProbeAvailable = true,
                CreatedUtc = DateTime.UtcNow
            };
        }

        public static bool TryConsumeRoleProbe(string organizationId, string userId)
        {
            return TryConsume(organizationId, userId, consumeRoleProbe: true);
        }

        public static bool TryConsumeMembershipProbe(string organizationId, string userId)
        {
            return TryConsume(organizationId, userId, consumeRoleProbe: false);
        }

        private static bool TryConsume(string organizationId, string userId, bool consumeRoleProbe)
        {
            var key = GetKey(organizationId, userId);
            if (key == null || !_states.TryGetValue(key, out var state)) return false;

            lock (state)
            {
                if (!Matches(state, organizationId, userId)) return false;

                if (consumeRoleProbe)
                {
                    if (!state.RoleProbeAvailable) return false;
                    state.RoleProbeAvailable = false;
                }
                else
                {
                    if (!state.MembershipProbeAvailable) return false;
                    state.MembershipProbeAvailable = false;
                }

                if (!state.RoleProbeAvailable && !state.MembershipProbeAvailable)
                    _states.TryRemove(key, out _);

                return true;
            }
        }

        private static string GetKey(string organizationId, string userId)
        {
            var traceId = Activity.Current?.TraceId.ToString();
            if (String.IsNullOrWhiteSpace(traceId)) return null;

            return $"{traceId}:{organizationId}:{userId}";
        }

        private static bool Matches(BootstrapState state, string organizationId, string userId)
        {
            return state != null &&
                String.Equals(state.OrganizationId, organizationId, StringComparison.Ordinal) &&
                String.Equals(state.UserId, userId, StringComparison.Ordinal);
        }

        private static void CleanupExpired()
        {
            var cutoff = DateTime.UtcNow.Subtract(_stateLifetime);
            foreach (var item in _states)
            {
                if (item.Value.CreatedUtc < cutoff)
                    _states.TryRemove(item.Key, out _);
            }
        }
    }
}
