using LagoVista.Core;
using LagoVista.UserAdmin.Interfaces.Repos.Apps;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Managers
{
    /// <summary>
    /// Resolves the canonical AppInstance for a proven human and a trusted application.
    /// The browser identifier is continuity metadata, never authentication.
    /// Only call from a host-verified authentication boundary.
    /// </summary>
    public sealed class BrowserInstallationManager
    {
        public const string BrowserClientType = "browser";
        private readonly IAppInstanceRepo _instances;

        public BrowserInstallationManager(IAppInstanceRepo instances)
            => _instances = instances ?? throw new ArgumentNullException(nameof(instances));

        public static string NewBrowserId() => Guid.NewGuid().ToString("N").ToUpperInvariant();

        private static string CanonicalId(string value)
        {
            if (!Guid.TryParseExact(value, "N", out var id) || id == Guid.Empty)
                throw new ArgumentException("A canonical identifier is required.");
            return id.ToString("N").ToUpperInvariant();
        }

        public static string InstanceId(string appId, string browserId)
        {
            // Deterministic within the existing user partition. Different apps or
            // browser profiles cannot accidentally reuse the same instance row.
            var source = "useradmin-browser-instance:v1:" + CanonicalId(appId) + ":" + CanonicalId(browserId);
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(source));
            return BitConverter.ToString(hash, 0, 16).Replace("-", String.Empty);
        }

        public async Task<AppInstance> BindProvenUserAsync(AppUser user, string trustedAppId, string browserId)
        {
            if (user == null || String.IsNullOrWhiteSpace(user.Id) || user.IsAccountDisabled ||
                user.IsDeleted == true || user.IsAnonymous || user.IsUserDevice ||
                (user.LoginType != LoginTypes.AppUser && user.LoginType != LoginTypes.AppEndUser))
                throw new InvalidOperationException("Browser installation binding requires a proven active human.");

            var appId = CanonicalId(trustedAppId);
            var deviceId = CanonicalId(browserId);
            var userId = user.Id.Value;
            var instanceId = InstanceId(appId, deviceId);
            var current = await _instances.GetAppInstanceAsync(userId, instanceId).ConfigureAwait(false);
            if (current != null)
                return RequireBinding(current, userId, appId, deviceId, instanceId);

            var now = DateTime.UtcNow.ToJSONString();
            var candidate = new AppInstance(instanceId, userId)
            {
                UserId = userId,
                AppId = appId,
                ClientType = BrowserClientType,
                DeviceId = deviceId,
                CreationDate = now,
                LastLogin = now
            };
            try
            {
                await _instances.AddAppInstanceAsync(candidate).ConfigureAwait(false);
                return candidate;
            }
            catch (Exception)
            {
                // The canonical store insert is unique on user/instance. If a
                // concurrent first-use won (or commit acknowledgement was lost),
                // accept only the exact persisted binding. Never overwrite it.
                var winner = await _instances.GetAppInstanceAsync(userId, instanceId).ConfigureAwait(false);
                if (winner == null) throw;
                return RequireBinding(winner, userId, appId, deviceId, instanceId);
            }
        }

        private static AppInstance RequireBinding(AppInstance instance, string userId, string appId,
            string browserId, string instanceId)
        {
            if (instance.PartitionKey != userId || instance.UserId != userId || instance.RowKey != instanceId ||
                instance.AppId != appId || instance.DeviceId != browserId || instance.ClientType != BrowserClientType)
                throw new InvalidOperationException("Browser installation binding rejected.");
            return instance;
        }
    }
}
