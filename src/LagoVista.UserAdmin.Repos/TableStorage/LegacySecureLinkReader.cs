using LagoVista.CloudStorage.Storage;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Security;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Security
{
    /// <summary>
    /// Read-only cutover support for already-issued secure links. No new Table writes.
    /// Remove after the longest outstanding pre-migration link has expired.
    /// </summary>
    public sealed class LegacySecureLinkReader : TableStorageBase<SecureLink>
    {
        public LegacySecureLinkReader(IUserAdminSettings settings, IAdminLogger logger)
            : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger) { }

        public Task<SecureLink> GetLegacyAsync(string orgId, string linkId)
            => GetAsync(orgId, linkId, false);
    }
}
