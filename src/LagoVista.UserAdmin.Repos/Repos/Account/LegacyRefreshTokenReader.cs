using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Authentication.Models;
using LagoVista.IoT.Logging.Loggers;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Users
{
    /// <summary>
    /// Read-only compatibility for refresh tokens issued before Cassandra cutover.
    /// Retire after the maximum seven-day legacy token lifetime and rollback window.
    /// </summary>
    public interface ILegacyRefreshTokenReader
    {
        Task<RefreshToken> GetAsync(string userId, string tokenId);
        Task RemoveAllForUserAsync(string userId);
    }

    internal sealed class LegacyRefreshTokenReader : TableStorageBase<RefreshToken>, ILegacyRefreshTokenReader
    {
        public LegacyRefreshTokenReader(IUserAdminSettings settings, IAdminLogger logger)
            : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger) { }

        public Task<RefreshToken> GetAsync(string userId, string tokenId) =>
            base.GetAsync(userId, tokenId, false);

        public Task RemoveAllForUserAsync(string userId) =>
            RemoveByPartitionKeyAsync(userId);
    }
}
