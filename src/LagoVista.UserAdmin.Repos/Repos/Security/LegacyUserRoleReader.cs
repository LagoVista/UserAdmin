using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public interface ILegacyUserRoleReader
    {
        Task<UserRoleDTO> GetLegacyAsync(string orgId, string roleAssignmentId);
        Task<IEnumerable<UserRoleDTO>> GetRolesForUserLegacyAsync(string userId, string orgId);
        Task<ListResponse<UserRoleDTO>> GetOrgPageAsync(string orgId, ListRequest request);
    }

    /// <summary>
    /// Read-only Azure bridge while production role assignments are backfilled
    /// and validated against Cassandra. Never writes role state to Azure.
    /// </summary>
    public sealed class LegacyUserRoleReader : TableStorageBase<UserRoleDTO>, ILegacyUserRoleReader
    {
        public LegacyUserRoleReader(IUserAdminSettings settings, IAdminLogger logger)
            : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger) { }

        public Task<UserRoleDTO> GetLegacyAsync(string orgId, string roleAssignmentId)
            => GetAsync(orgId, roleAssignmentId, false);

        public Task<IEnumerable<UserRoleDTO>> GetRolesForUserLegacyAsync(string userId, string orgId)
            => GetByFilterAsync(
                FilterOptions.Create(nameof(UserRoleDTO.UserId), FilterOptions.Operators.Equals, userId),
                FilterOptions.Create(nameof(UserRoleDTO.PartitionKey), FilterOptions.Operators.Equals, orgId));

        public Task<ListResponse<UserRoleDTO>> GetOrgPageAsync(string orgId, ListRequest request)
            => GetPagedResultsAsync(orgId, request);
    }
}
