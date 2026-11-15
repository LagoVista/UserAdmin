using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LagoVista.UserAdmin.Repos.ApplicationData.Notifications
{
    /// <summary>
    /// Preserves legacy PartitionKey/RowKey as public action identifiers while
    /// using one deterministic Mongo _id per item. No Azure Table entity is stored.
    /// </summary>
    public static class NotificationRecordIdentity
    {
        public static NormalizedId32 For(string partitionKey, string rowKey)
        {
            if (String.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentException("PartitionKey is required.", nameof(partitionKey));
            if (String.IsNullOrWhiteSpace(rowKey))
                throw new ArgumentException("RowKey is required.", nameof(rowKey));
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(
                    "useradmin-notifications:v1:" + partitionKey.Length + ":" + partitionKey + ":" + rowKey));
                return new NormalizedId32(BitConverter.ToString(bytes, 0, 16).Replace("-", String.Empty));
            }
        }

        public static EntityHeader RequirePlatformOrg(IAppConfig config)
        {
            var org = config?.SystemOwnerOrg;
            if (org == null || String.IsNullOrWhiteSpace(org.Id))
                throw new InvalidOperationException("SystemOwnerOrg is required for notification storage.");
            return org;
        }

        public static bool TryPage(ListRequest request, string partitionKey,
            out StoragePageRequest page, out string error)
        {
            page = null;
            error = null;
            if (request == null) { error = "ListRequest is required."; return false; }
            if (request.PageIndex < 1 || request.PageSize < 1 || request.PageSize > 1000)
            {
                error = "PageIndex must be at least 1 and PageSize between 1 and 1000.";
                return false;
            }

            var hasPartition = !String.IsNullOrWhiteSpace(request.NextPartitionKey);
            var hasRow = !String.IsNullOrWhiteSpace(request.NextRowKey);
            if (hasPartition != hasRow ||
                (hasPartition && !String.Equals(request.NextPartitionKey, partitionKey, StringComparison.Ordinal)))
            {
                error = "Notification continuation cursor is invalid for this partition.";
                return false;
            }
            if (request.PageIndex > 1 && !hasRow)
            {
                error = "Pages after the first require the previous response's continuation keys.";
                return false;
            }

            page = new StoragePageRequest(request.PageSize, hasRow ? request.NextRowKey : null);
            return true;
        }

        public static ListResponse<T> PageResponse<T>(IEnumerable<T> items, ListRequest request,
            string partition, string continuation)
        {
            var more = !String.IsNullOrWhiteSpace(continuation);
            return ListResponse<T>.Create(items, request, more,
                more ? partition : String.Empty, continuation ?? String.Empty);
        }
    }

    public sealed class AppUserInboxRecord : IApplicationDataRecord
    {
        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public bool Viewed { get; set; }
        public string ViewedTimeStamp { get; set; }
        public string UserId { get; set; }
        public string OrgId { get; set; }
        public string Type { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public string ItemCreationDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedById { get; set; }
        public string ItemLastUpdatedDate { get; set; }

        public static AppUserInboxRecord FromModel(AppUserInboxItem item, EntityHeader organization)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            if (String.IsNullOrWhiteSpace(item.UserId) || String.IsNullOrWhiteSpace(item.OrgId))
                throw new ArgumentException("Inbox item requires both UserId and OrgId.", nameof(item));
            if (!String.Equals(item.PartitionKey,
                    AppUserInboxItem.ConstructPartitionKey(item.OrgId, item.UserId), StringComparison.Ordinal))
                throw new ArgumentException("Inbox item PartitionKey does not match its organization/user scope.", nameof(item));

            return new AppUserInboxRecord
            {
                Id = NotificationRecordIdentity.For(item.PartitionKey, item.RowKey),
                Organization = organization,
                PartitionKey = item.PartitionKey,
                RowKey = item.RowKey,
                Viewed = item.Viewed,
                ViewedTimeStamp = item.ViewedTimeStamp,
                UserId = item.UserId,
                OrgId = item.OrgId,
                Type = item.Type,
                Icon = item.Icon,
                Title = item.Title,
                Summary = item.Summary,
                Link = item.Link,
                CreatedBy = item.CreatedBy,
                CreatedById = item.CreatedById,
                ItemCreationDate = item.CreationDate,
                LastUpdatedBy = item.LastUpdatedBy,
                LastUpdatedById = item.LastUpdatedById,
                ItemLastUpdatedDate = item.LastUpdatedDate
            };
        }

        public AppUserInboxItem ToModel()
        {
            return new AppUserInboxItem
            {
                PartitionKey = PartitionKey,
                RowKey = RowKey,
                Viewed = Viewed,
                ViewedTimeStamp = ViewedTimeStamp,
                UserId = UserId,
                OrgId = OrgId,
                Type = Type,
                Icon = Icon,
                Title = Title,
                Summary = Summary,
                Link = Link,
                CreatedBy = CreatedBy,
                CreatedById = CreatedById,
                CreationDate = ItemCreationDate,
                LastUpdatedBy = LastUpdatedBy,
                LastUpdatedById = LastUpdatedById,
                LastUpdatedDate = ItemLastUpdatedDate
            };
        }
    }

    public sealed class SystemNotificationRecord : IApplicationDataRecord
    {
        public NormalizedId32 Id { get; set; }
        public EntityHeader Organization { get; set; }
        public UtcTimestamp CreationDate { get; set; }
        public UtcTimestamp LastUpdatedDate { get; set; }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public string OrgId { get; set; }
        public int Index { get; set; }
        public string Icon { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Link { get; set; }
        public string CreatedBy { get; set; }
        public string CreatedById { get; set; }
        public string ItemCreationDate { get; set; }
        public string LastUpdatedBy { get; set; }
        public string LastUpdatedById { get; set; }
        public string ItemLastUpdatedDate { get; set; }

        public static SystemNotificationRecord FromModel(SystemNotification item, EntityHeader organization)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));
            return new SystemNotificationRecord
            {
                Id = NotificationRecordIdentity.For(item.PartitionKey, item.RowKey),
                Organization = organization,
                PartitionKey = item.PartitionKey,
                RowKey = item.RowKey,
                OrgId = item.OrgId,
                Index = item.Index,
                Icon = item.Icon,
                Title = item.Title,
                Summary = item.Summary,
                Link = item.Link,
                CreatedBy = item.CreatedBy,
                CreatedById = item.CreatedById,
                ItemCreationDate = item.CreationDate,
                LastUpdatedBy = item.LastUpdatedBy,
                LastUpdatedById = item.LastUpdatedById,
                ItemLastUpdatedDate = item.LastUpdatedDate
            };
        }

        public SystemNotification ToModel()
        {
            return new SystemNotification
            {
                PartitionKey = PartitionKey,
                RowKey = RowKey,
                OrgId = OrgId,
                Index = Index,
                Icon = Icon,
                Title = Title,
                Summary = Summary,
                Link = Link,
                CreatedBy = CreatedBy,
                CreatedById = CreatedById,
                CreationDate = ItemCreationDate,
                LastUpdatedBy = LastUpdatedBy,
                LastUpdatedById = LastUpdatedById,
                LastUpdatedDate = ItemLastUpdatedDate
            };
        }
    }
}
