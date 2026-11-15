using LagoVista.CloudStorage.Storage;
using LagoVista.UserAdmin.Models.DTOs;
using System;

namespace LagoVista.UserAdmin.Repos.OperationalData.RolePermissions
{
    /// <summary>
    /// Durable role-permission assignment: private rows are partitioned by
    /// organization, while public rows use one shared public partition.
    /// The source organization is retained for lossless migration/DTO identity.
    /// Removed permissions remain tombstoned to prevent Azure resurrection.
    /// </summary>
    public sealed class RoleAccessOperationalRecord : IConditionalOperationalDataRecord
    {
        public const string PublicPartition = "public_role_access";
        public string OrganizationId { get; set; }
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public long Version { get; set; }
        public bool IsDeleted { get; set; }

        public string SourceOrganizationId { get; set; }
        public string SourceOrganizationName { get; set; }
        public string RoleId { get; set; }
        public string RoleKey { get; set; }
        public string RoleName { get; set; }
        public bool IsPublic { get; set; }
        public string CreatedById { get; set; }
        public string CreatedByName { get; set; }
        public string OriginalCreationDate { get; set; }

        public string ModuleId { get; set; }
        public string ModuleKey { get; set; }
        public string ModuleName { get; set; }
        public string AreaId { get; set; }
        public string AreaKey { get; set; }
        public string AreaName { get; set; }
        public string PageId { get; set; }
        public string PageKey { get; set; }
        public string PageName { get; set; }
        public string FeatureId { get; set; }
        public string FeatureKey { get; set; }
        public string FeatureName { get; set; }
        public string FunctionMapId { get; set; }
        public string FunctionMapKey { get; set; }
        public string FunctionMapName { get; set; }
        public string FunctionMapFunctionId { get; set; }
        public string FunctionMapFunctionKey { get; set; }
        public string FunctionMapFunctionName { get; set; }

        public int Create { get; set; }
        public int Read { get; set; }
        public int Update { get; set; }
        public int Delete { get; set; }
        public string Other { get; set; }

        public static RoleAccessOperationalRecord FromDTO(RoleAccessDTO dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            if (String.IsNullOrWhiteSpace(dto.RowKey) ||
                String.IsNullOrWhiteSpace(dto.PartitionKey) ||
                String.IsNullOrWhiteSpace(dto.RoleId))
                throw new ArgumentException("Role access requires an ID, source organization and role.", nameof(dto));
            var sourceOrgId = String.IsNullOrWhiteSpace(dto.OrganizationId)
                ? dto.PartitionKey : dto.OrganizationId;
            if (!String.Equals(dto.PartitionKey, sourceOrgId, StringComparison.Ordinal) &&
                !String.Equals(dto.PartitionKey, PublicPartition, StringComparison.Ordinal))
                throw new ArgumentException("Source organization and original partition must match.", nameof(dto));

            return new RoleAccessOperationalRecord
            {
                OrganizationId = dto.IsPublic ? PublicPartition : dto.PartitionKey,
                SourceOrganizationId = sourceOrgId,
                SourceOrganizationName = dto.OrganizationName,
                Id = dto.RowKey,
                RoleId = dto.RoleId,
                RoleKey = dto.RoleKey,
                RoleName = dto.RoleName,
                IsPublic = dto.IsPublic,
                CreatedById = dto.CreatedById,
                CreatedByName = dto.CreatedByName,
                OriginalCreationDate = dto.CreationDate,
                ModuleId = dto.ModuleId,
                ModuleKey = dto.ModuleKey,
                ModuleName = dto.ModuleName,
                AreaId = dto.AreaId,
                AreaKey = dto.AreaKey,
                AreaName = dto.AreaName,
                PageId = dto.PageId,
                PageKey = dto.PageKey,
                PageName = dto.PageName,
                FeatureId = dto.FeatureId,
                FeatureKey = dto.FeatureKey,
                FeatureName = dto.FeatureName,
                FunctionMapId = dto.FunctionMapId,
                FunctionMapKey = dto.FunctionMapKey,
                FunctionMapName = dto.FunctionMapName,
                FunctionMapFunctionId = dto.FunctionMapFunctionId,
                FunctionMapFunctionKey = dto.FunctionMapFunctionKey,
                FunctionMapFunctionName = dto.FunctionMapFunctionName,
                Create = dto.Create,
                Read = dto.Read,
                Update = dto.Update,
                Delete = dto.Delete,
                Other = dto.Other
            };
        }

        public static RoleAccessOperationalRecord Tombstone(string partition, string id)
        {
            if (String.IsNullOrWhiteSpace(partition))
                throw new ArgumentException("Partition is required.", nameof(partition));
            if (String.IsNullOrWhiteSpace(id))
                throw new ArgumentException("AssignmentId is required.", nameof(id));
            return new RoleAccessOperationalRecord
            {
                OrganizationId = partition,
                SourceOrganizationId = partition,
                Id = id,
                IsPublic = partition == PublicPartition,
                IsDeleted = true
            };
        }

        public RoleAccessDTO ToDTO()
        {
            if (IsDeleted) return null;
            return new RoleAccessDTO
            {
                RowKey = Id,
                PartitionKey = SourceOrganizationId,
                OrganizationId = SourceOrganizationId,
                OrganizationName = SourceOrganizationName,
                RoleId = RoleId,
                RoleKey = RoleKey,
                RoleName = RoleName,
                IsPublic = IsPublic,
                CreatedById = CreatedById,
                CreatedByName = CreatedByName,
                CreationDate = OriginalCreationDate,
                ModuleId = ModuleId,
                ModuleKey = ModuleKey,
                ModuleName = ModuleName,
                AreaId = AreaId,
                AreaKey = AreaKey,
                AreaName = AreaName,
                PageId = PageId,
                PageKey = PageKey,
                PageName = PageName,
                FeatureId = FeatureId,
                FeatureKey = FeatureKey,
                FeatureName = FeatureName,
                FunctionMapId = FunctionMapId,
                FunctionMapKey = FunctionMapKey,
                FunctionMapName = FunctionMapName,
                FunctionMapFunctionId = FunctionMapFunctionId,
                FunctionMapFunctionKey = FunctionMapFunctionKey,
                FunctionMapFunctionName = FunctionMapFunctionName,
                Create = Create,
                Read = Read,
                Update = Update,
                Delete = Delete,
                Other = Other
            };
        }
    }
}
