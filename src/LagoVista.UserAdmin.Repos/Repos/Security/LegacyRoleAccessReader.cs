using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.UserAdmin.Models.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Repos.Repos.Security
{
    public interface ILegacyRoleAccessReader
    {
        Task<RoleAccessDTO> GetLegacyAsync(string orgId, string assignmentId);
        Task<ListResponse<RoleAccessDTO>> GetOrgPageAsync(string orgId, ListRequest request);
        Task<IEnumerable<RoleAccessDTO>> GetPublicLegacyAsync();
    }

    /// <summary>
    /// Read-only Azure permission bridge, including legacy public rows that
    /// may live under multiple source organization partitions.
    /// </summary>
    public sealed class LegacyRoleAccessReader : TableStorageBase<RoleAccessDTO>, ILegacyRoleAccessReader
    {
        public LegacyRoleAccessReader(IUserAdminSettings settings, IAdminLogger logger)
            : base(settings.UserTableStorage.AccountId, settings.UserTableStorage.AccessKey, logger) { }

        public Task<RoleAccessDTO> GetLegacyAsync(string orgId, string assignmentId)
            => GetAsync(orgId, assignmentId, false);

        public Task<ListResponse<RoleAccessDTO>> GetOrgPageAsync(string orgId, ListRequest request)
            => GetPagedResultsAsync(orgId, request);

        public Task<IEnumerable<RoleAccessDTO>> GetPublicLegacyAsync()
            => GetByFilterAsync(FilterOptions.Create(nameof(RoleAccessDTO.IsPublic),
                FilterOptions.Operators.Equals, true));
    }
}
