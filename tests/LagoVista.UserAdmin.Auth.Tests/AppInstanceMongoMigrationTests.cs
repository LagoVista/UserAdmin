using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Apps;
using LagoVista.UserAdmin.Repos.ApplicationData.AppInstances;
using LagoVista.UserAdmin.Repos.Repos.Apps;
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
    public sealed class AppInstanceMongoMigrationTests
    {
        private const string UserId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string InstanceId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private static readonly EntityHeader PlatformOrg = EntityHeader.Create(
            "82A7FA17E3E44920A2C12E08F327D589", "Platform");

        [Test]
        public void AppInstance_Record_Preserves_Legacy_Identity_And_Fields()
        {
            var model = NewInstance();
            var record = AppInstanceRecord.FromModel(model, PlatformOrg);
            var restored = record.ToModel();

            Assert.That(record.Id.Value.Length, Is.EqualTo(32));
            Assert.That(record.Id.Value,
                Is.EqualTo(AppInstanceRecord.CreateId(UserId, InstanceId).Value));
            Assert.That(record.Id.Value,
                Is.Not.EqualTo(AppInstanceRecord.CreateId(UserId, "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC").Value));
            Assert.That(record.Id.Value,
                Is.Not.EqualTo(AppInstanceRecord.CreateId("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD", InstanceId).Value));
            Assert.That(restored.PartitionKey, Is.EqualTo(model.PartitionKey));
            Assert.That(restored.RowKey, Is.EqualTo(model.RowKey));
            Assert.That(restored.AppId, Is.EqualTo(model.AppId));
            Assert.That(restored.DeviceId, Is.EqualTo(model.DeviceId));
            Assert.That(restored.LastAccessTokenRefresh, Is.EqualTo(model.LastAccessTokenRefresh));
            Assert.That(restored.LastLogin, Is.EqualTo(model.LastLogin));
        }

        [Test]
        public async Task Get_By_Id_Prefers_Updated_Mongo_Record_Over_Legacy()
        {
            var store = new Mock<IApplicationDataStore>();
            var reader = new Mock<ILegacyAppInstanceReader>();
            var record = AppInstanceRecord.FromModel(NewInstance(), PlatformOrg);
            record.LastLogin = "new-login";
            store.Setup(s => s.GetAsync<AppInstanceRecord>(It.IsAny<StorageKey>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);

            var actual = await NewRepo(store, reader).GetAppInstanceAsync(UserId, InstanceId);
            Assert.That(actual.LastLogin, Is.EqualTo("new-login"));
            reader.Verify(r => r.GetLegacyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Legacy_Get_Imports_Record_Once_Without_Overwriting_Mongo()
        {
            var model = NewInstance();
            var store = new Mock<IApplicationDataStore>();
            var reader = new Mock<ILegacyAppInstanceReader>();
            reader.Setup(r => r.GetLegacyAsync(UserId, InstanceId)).ReturnsAsync(model);
            store.Setup(s => s.InsertAsync(It.IsAny<AppInstanceRecord>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            var actual = await NewRepo(store, reader).GetAppInstanceAsync(UserId, InstanceId);

            Assert.That(actual.AppId, Is.EqualTo(model.AppId));
            store.Verify(s => s.InsertAsync(It.Is<AppInstanceRecord>(r =>
                r.UserPartition == UserId && r.InstanceKey == InstanceId),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task List_Imports_Legacy_But_Returns_Mongo_Authoritative_Copy()
        {
            var model = NewInstance();
            var store = new Mock<IApplicationDataStore>();
            var reader = new Mock<ILegacyAppInstanceReader>();
            var record = AppInstanceRecord.FromModel(model, PlatformOrg);
            record.LastLogin = "new-login";
            reader.Setup(r => r.GetForUserLegacyAsync(UserId)).ReturnsAsync(new[] { model });
            store.Setup(s => s.GetAsync<AppInstanceRecord>(It.IsAny<StorageKey>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);
            StorageQuery<AppInstanceRecord> captured = null;
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<AppInstanceRecord>>(), It.IsAny<CancellationToken>()))
                .Callback<StorageQuery<AppInstanceRecord>, CancellationToken>((q, _) => captured = q)
                .ReturnsAsync(new StoragePageResult<AppInstanceRecord>(new[] { record }));

            var actual = (await NewRepo(store, reader).GetForUserAsync(UserId)).Single();
            Assert.That(actual.LastLogin, Is.EqualTo("new-login"));
            Assert.That(captured.Filters.Any(f => f.Field == nameof(AppInstanceRecord.UserPartition) &&
                (string)f.Value == UserId), Is.True);
            store.Verify(s => s.InsertAsync(It.IsAny<AppInstanceRecord>(),
                It.IsAny<CancellationToken>()), Times.Never());
        }

        [Test]
        public void Reject_Mismatched_User_And_Partition()
        {
            var model = NewInstance();
            model.UserId = "DIFFERENT";
            Assert.Throws<ArgumentException>(() => AppInstanceRecord.FromModel(model, PlatformOrg));
        }

        private static AppInstance NewInstance()
            => new AppInstance(InstanceId, UserId)
            {
                AppId = "app-1",
                ClientType = "desktop",
                DeviceId = "device-1",
                LastLogin = "old-login",
                LastAccessTokenRefresh = "old-refresh"
            };

        private static AppInstanceRepo NewRepo(
            Mock<IApplicationDataStore> store, Mock<ILegacyAppInstanceReader> reader)
        {
            var config = new Mock<IAppConfig>();
            config.SetupGet(c => c.SystemOwnerOrg).Returns(PlatformOrg);
            return new AppInstanceRepo(store.Object, config.Object, reader.Object);
        }
    }
}
