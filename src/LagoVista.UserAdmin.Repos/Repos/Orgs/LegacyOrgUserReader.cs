using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.Orgs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Orgs
{
    public interface ILegacyOrgUserReader
    {
        Task<OrgUser> GetLegacyAsync(string orgId, string userId);
        Task<IEnumerable<OrgUser>> GetLegacyForUserAsync(string userId);
        Task<ListResponse<OrgUser>> GetOrgPageAsync(string orgId, ListRequest request);
    }

    /// <summary>
    /// Read-only Azure membership compatibility. Remove only after full production
    /// membership backfill, key/count reconciliation and cutover validation.
    /// </summary>
    public sealed class LegacyOrgUserReader : TableStorageBase<OrgUser>, ILegacyOrgUserReader
    {
        public LegacyOrgUserReader(IUserAdminSettings settings, IAdminLogger logger)
            : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger) { }

        public Task<OrgUser> GetLegacyAsync(string orgId, string userId)
            => GetAsync(orgId, OrgUser.CreateRowKey(orgId, userId), false);

        public Task<IEnumerable<OrgUser>> GetLegacyForUserAsync(string userId)
            => GetByFilterAsync(FilterOptions.Create(nameof(OrgUser.UserId),
                FilterOptions.Operators.Equals, userId));

        public Task<ListResponse<OrgUser>> GetOrgPageAsync(string orgId, ListRequest request)
            => GetPagedResultsAsync(orgId, request);
    }
}
