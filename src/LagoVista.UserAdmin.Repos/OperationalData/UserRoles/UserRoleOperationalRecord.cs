using LagoVista.CloudStorage.Storage;
using LagoVista.UserAdmin.Models.DTOs;
using System;

namespace LagoVista.UserAdmin.Repos.OperationalData.UserRoles
{
    /// <summary>
    /// Durable organization/user/role assignment. Preserve the original
    /// organization partition and RowKey, with a permanent revocation marker
    /// to prevent legacy Azure import from restoring revoked permissions.
    /// </summary>
    public sealed class UserRoleOperationalRecord : IConditionalOperationalDataRecord
    {
        public string OrganizationId { get; set; }
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public long Version { get; set; }
        public bool IsDeleted { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }
        public string OrganizationName { get; set; }
        public string RoleId { get; set; }
        public string RoleKey { get; set; }
        public string RoleName { get; set; }
        public string OriginalCreationDate { get; set; }
        public string CreatedById { get; set; }
        public string CreatedByName { get; set; }

        public static UserRoleOperationalRecord FromDTO(UserRoleDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (String.IsNullOrWhiteSpace(dto.PartitionKey) ||
                String.IsNullOrWhiteSpace(dto.RowKey) ||
                String.IsNullOrWhiteSpace(dto.UserId) ||
                String.IsNullOrWhiteSpace(dto.RoleId))
                throw new ArgumentException("A role assignment requires organization, Id, user and role.", nameof(dto));
            if (!String.IsNullOrWhiteSpace(dto.OrganizationId) &&
                !String.Equals(dto.OrganizationId, dto.PartitionKey, StringComparison.Ordinal))
                throw new ArgumentException("Role assignment organization must match partition.", nameof(dto));

            return new UserRoleOperationalRecord
            {
                OrganizationId = dto.PartitionKey,
                Id = dto.RowKey,
                UserId = dto.UserId,
                UserName = dto.UserName,
                OrganizationName = dto.OrganizationName,
                RoleId = dto.RoleId,
                RoleKey = dto.RoleKey,
                RoleName = dto.RoleName,
                OriginalCreationDate = dto.CreationDate,
                CreatedById = dto.CreatedById,
                CreatedByName = dto.CreatedByName
            };
        }

        public static UserRoleOperationalRecord Tombstone(string orgId, string id)
        {
            if (String.IsNullOrWhiteSpace(orgId)) throw new ArgumentException("OrganizationId required.", nameof(orgId));
            if (String.IsNullOrWhiteSpace(id)) throw new ArgumentException("Role assignment Id required.", nameof(id));
            return new UserRoleOperationalRecord
            {
                OrganizationId = orgId,
                Id = id,
                IsDeleted = true
            };
        }

        public UserRoleDTO ToDTO()
        {
            if (IsDeleted) return null;
            return new UserRoleDTO
            {
                PartitionKey = OrganizationId,
                RowKey = Id,
                UserId = UserId,
                UserName = UserName,
                OrganizationId = OrganizationId,
                OrganizationName = OrganizationName,
                RoleId = RoleId,
                RoleKey = RoleKey,
                RoleName = RoleName,
                CreationDate = OriginalCreationDate,
                CreatedById = CreatedById,
                CreatedByName = CreatedByName
            };
        }
    }
}
