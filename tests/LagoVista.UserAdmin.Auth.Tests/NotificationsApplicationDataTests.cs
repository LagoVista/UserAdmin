using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.ApplicationData.Notifications;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.UserAdmin.Auth.Tests
{
    [TestFixture]
    public sealed class NotificationsApplicationDataTests
    {
        private static readonly EntityHeader PlatformOrg = new EntityHeader
        {
            Id = "82A7FA17E3E44920A2C12E08F327D589",
            Text = "Platform"
        };

        [Test]
        public void Inbox_Record_Retains_Existing_Action_Identifiers_And_Read_State()
        {
            var item = NewInboxItem();
            item.Viewed = true;
            item.ViewedTimeStamp = "2026-11-15T12:00:00Z";
            var record = AppUserInboxRecord.FromModel(item, PlatformOrg);
            var restored = record.ToModel();

            Assert.That(record.Id.Value.Length, Is.EqualTo(32));
            Assert.That(record.Id.Value, Is.EqualTo(
                NotificationRecordIdentity.For(item.PartitionKey, item.RowKey).Value));
            Assert.That(restored.PartitionKey, Is.EqualTo(item.PartitionKey));
            Assert.That(restored.RowKey, Is.EqualTo(item.RowKey));
            Assert.That(restored.Viewed, Is.True);
            Assert.That(restored.ViewedTimeStamp, Is.EqualTo(item.ViewedTimeStamp));
            Assert.That(restored.Summary, Is.EqualTo(item.Summary));
        }

        [Test]
        public void Different_Inbox_Partitions_Cannot_Share_The_Same_Mongo_Identity()
        {
            var first = NotificationRecordIdentity.For("org-a-user-a", "row-1");
            var second = NotificationRecordIdentity.For("org-b-user-a", "row-1");
            Assert.That(first.Value, Is.Not.EqualTo(second.Value));
        }

        [Test]
        public void System_Notification_Round_Trip_Preserves_Public_Partition_And_Index()
        {
            var notification = new SystemNotification
            {
                PartitionKey = "public",
                RowKey = "00001",
                Index = 42,
                Title = "Announcement",
                Summary = "Public notice"
            };
            var result = SystemNotificationRecord.FromModel(notification, PlatformOrg).ToModel();
            Assert.That(result.PartitionKey, Is.EqualTo("public"));
            Assert.That(result.RowKey, Is.EqualTo("00001"));
            Assert.That(result.Index, Is.EqualTo(42));
            Assert.That(result.ToInboxItem().Scope, Is.EqualTo("system"));
        }

        [Test]
        public async Task Unread_Inbox_Query_Uses_Viewed_False_And_User_Partition()
        {
            var store = NewStore();
            StorageQuery<AppUserInboxRecord> captured = null;
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<AppUserInboxRecord>>(),
                It.IsAny<CancellationToken>()))
                .Callback<StorageQuery<AppUserInboxRecord>, CancellationToken>((query, _) => captured = query)
                .ReturnsAsync(new StoragePageResult<AppUserInboxRecord>(
                    new[] { AppUserInboxRecord.FromModel(NewInboxItem(), PlatformOrg) }));
            var repo = new AppUserIndboxRepo(store.Object, NewConfig().Object);
            var request = new ListRequest { PageIndex = 1, PageSize = 20 };
            var response = await repo.GetInboxItemsAsync("org-1", "user-1", request, unreadOnly: true);

            Assert.That(response.Model.Count(), Is.EqualTo(1));
            Assert.That(captured.Filters.Any(f =>
                f.Field == nameof(AppUserInboxRecord.PartitionKey) &&
                (string)f.Value == "org-1-user-1"), Is.True);
            Assert.That(captured.Filters.Any(f =>
                f.Field == nameof(AppUserInboxRecord.Viewed) &&
                f.Value is bool viewed && viewed == false), Is.True);
            Assert.That(captured.Sorts.Any(s => s.Field == nameof(AppUserInboxRecord.RowKey) &&
                s.Direction == StorageSortDirection.Ascending), Is.True);
        }

        [Test]
        public async Task System_Notices_Combine_Public_And_Organization_Only()
        {
            var store = NewStore();
            var partitions = new List<string>();
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<SystemNotificationRecord>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((StorageQuery<SystemNotificationRecord> query, CancellationToken _) =>
                {
                    var partition = (string)query.Filters.Single(f =>
                        f.Field == nameof(SystemNotificationRecord.PartitionKey)).Value;
                    partitions.Add(partition);
                    var record = SystemNotificationRecord.FromModel(new SystemNotification
                    {
                        PartitionKey = partition,
                        RowKey = partition == "public" ? "p" : "o",
                        Index = partition == "public" ? 1 : 2,
                        Title = partition
                    }, PlatformOrg);
                    return new StoragePageResult<SystemNotificationRecord>(new[] { record });
                });
            var repo = new SystemNotificationsRepo(store.Object, NewConfig().Object);
            var notices = (await repo.GetSystemAndPublicNotifications("org-1")).ToList();

            Assert.That(partitions, Is.EquivalentTo(new[] { "public", "org-1" }));
            Assert.That(notices.Select(n => n.Title), Is.EqualTo(new[] { "org-1", "public" }));
        }

        private static AppUserInboxItem NewInboxItem()
        {
            return new AppUserInboxItem
            {
                PartitionKey = AppUserInboxItem.ConstructPartitionKey("org-1", "user-1"),
                RowKey = "000123",
                OrgId = "org-1",
                UserId = "user-1",
                Title = "Work item",
                Summary = "Details",
                Viewed = false
            };
        }

        private static Mock<IApplicationDataStore> NewStore() => new Mock<IApplicationDataStore>();

        private static Mock<IAppConfig> NewConfig()
        {
            var config = new Mock<IAppConfig>();
            config.SetupGet(x => x.SystemOwnerOrg).Returns(PlatformOrg);
            return config;
        }
    }
}
