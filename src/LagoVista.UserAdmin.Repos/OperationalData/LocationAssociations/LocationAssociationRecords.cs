using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using System;

namespace LagoVista.UserAdmin.Repos.OperationalData.LocationAssociations
{
    /// <summary>
    /// Two physical lookup projections, one partitioned by location and one by
    /// user. The location partition is authoritative. No TTL.
    /// </summary>
    public sealed class LocationUserOperationalRecord : IOperationalDataRecord
    {
        public string OrganizationId { get; set; }
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }

        public string TenantOrganizationId { get; set; }
        public string TenantOrganizationName { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string UserId { get; set; }
        public string UsersName { get; set; }
        public string ProfileImageUrl { get; set; }
        public string Email { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public string OriginalCreationDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedById { get; set; }
        public string OriginalLastUpdatedDate { get; set; }

        public static string LocationPartition(string locationId)
            => "location:" + Require(locationId, nameof(locationId));

        public static string UserPartition(string userId)
            => "user:" + Require(userId, nameof(userId));

        public static string AssociationId(string locationId, string userId)
            => LocationUser.CreateRowId(Require(locationId, nameof(locationId)),
                Require(userId, nameof(userId)));

        public static LocationUserOperationalRecord ForLocation(LocationUser model)
            => FromModel(model, LocationPartition(model?.LocationId));

        public static LocationUserOperationalRecord ForUser(LocationUser model)
            => FromModel(model, UserPartition(model?.UserId));

        private static LocationUserOperationalRecord FromModel(LocationUser model, string partition)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var key = AssociationId(model.LocationId, model.UserId);
            if (String.IsNullOrWhiteSpace(model.OrganizationId))
                throw new ArgumentException("Tenant organization is required.", nameof(model));
            if (!String.IsNullOrEmpty(model.PartitionKey) &&
                !String.Equals(model.PartitionKey, model.LocationId, StringComparison.Ordinal))
                throw new ArgumentException("Location partition does not match LocationId.", nameof(model));
            if (!String.IsNullOrEmpty(model.RowKey) &&
                !String.Equals(model.RowKey, key, StringComparison.Ordinal))
                throw new ArgumentException("Location user RowKey does not match association.", nameof(model));

            return new LocationUserOperationalRecord
            {
                OrganizationId = partition,
                Id = key,
                TenantOrganizationId = model.OrganizationId,
                TenantOrganizationName = model.OrganizationName,
                LocationId = model.LocationId,
                LocationName = model.LocationName,
                UserId = model.UserId,
                UsersName = model.UsersName,
                ProfileImageUrl = model.ProfileImageUrl,
                Email = model.Email,
                CreatedBy = model.CreatedBy,
                CreatedById = model.CreatedById,
                OriginalCreationDate = model.CreationDate,
                LastUpdatedBy = model.LastUpdatedBy,
                LastUpdatedById = model.LastUpdatedById,
                OriginalLastUpdatedDate = model.LastUpdatedDate
            };
        }

        public LocationUser ToModel()
        {
            return new LocationUser(TenantOrganizationId, LocationId, UserId)
            {
                OrganizationName = TenantOrganizationName,
                LocationName = LocationName,
                UsersName = UsersName,
                ProfileImageUrl = ProfileImageUrl,
                Email = Email,
                CreatedBy = CreatedBy,
                CreatedById = CreatedById,
                CreationDate = OriginalCreationDate,
                LastUpdatedBy = LastUpdatedBy,
                LastUpdatedById = LastUpdatedById,
                LastUpdatedDate = OriginalLastUpdatedDate
            };
        }

        public static string Require(string value, string name)
        {
            if (String.IsNullOrWhiteSpace(value))
                throw new ArgumentException(name + " is required.", name);
            return value;
        }
    }

    /// <summary>
    /// Each user-role pair is an independent row within its location partition,
    /// so assigning several roles never overwrites an existing assignment.
    /// No TTL: explicit revocation only.
    /// </summary>
    public sealed class LocationRoleOperationalRecord : IConditionalOperationalDataRecord
    {
        public string OrganizationId { get; set; }
        public string Id { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastUpdatedDate { get; set; }
        public long Version { get; set; }

        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string RoleId { get; set; }
        public string RoleName { get; set; }

        public static string AssociationId(string userId, string roleId)
        {
            userId = LocationUserOperationalRecord.Require(userId, nameof(userId));
            roleId = LocationUserOperationalRecord.Require(roleId, nameof(roleId));
            return userId.Length + ":" + userId + ":" + roleId;
        }

        public static LocationRoleOperationalRecord FromModel(LocationUserRole model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var locationId = LocationUserOperationalRecord.Require(model.LocationId, nameof(model.LocationId));
            var key = AssociationId(model.UserId, model.RoleId);
            if (!String.IsNullOrEmpty(model.PartitionKey) &&
                !String.Equals(model.PartitionKey, locationId, StringComparison.Ordinal))
                throw new ArgumentException("Role assignment partition does not match LocationId.", nameof(model));
            return new LocationRoleOperationalRecord
            {
                OrganizationId = locationId,
                Id = key,
                LocationId = locationId,
                LocationName = model.LocationName,
                UserId = model.UserId,
                UserName = model.UserName,
                RoleId = model.RoleId,
                RoleName = model.RoleName
            };
        }

        public LocationUserRole ToModel()
        {
            var model = new LocationUserRole(EntityHeader.Create(LocationId, LocationName),
                EntityHeader.Create(UserId, UserName))
            {
                RoleId = RoleId,
                RoleName = RoleName
            };
            // A location/user can have several roles. The old RowKey omitted RoleId.
            model.RowKey = model.GetRowKey();
            return model;
        }
    }
}
