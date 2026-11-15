using LagoVista.CloudStorage.Storage;
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.Orgs;
using LagoVista.UserAdmin.Repos.ApplicationData.OrgMemberships;
using LagoVista.UserAdmin.Repos.Orgs;
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
    public sealed class OrgMembershipMongoMigrationTests
    {
        private const string OrgId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string UserId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private static readonly EntityHeader PlatformOrg = EntityHeader.Create(
            "82A7FA17E3E44920A2C12E08F327D589", "Platform");

        [Test]
        public void Membership_Round_Trip_Preserves_Privileges_And_Legacy_Identity()
        {
            var model = NewMember();
            var record = OrgMembershipRecord.FromModel(model, PlatformOrg);
            var copy = record.ToModel();

            Assert.That(record.Id.Value.Length, Is.EqualTo(32));
            Assert.That(copy.OrgId.Value, Is.EqualTo(OrgId));
            Assert.That(copy.UserId.Value, Is.EqualTo(UserId));
            Assert.That(copy.RowKey, Is.EqualTo(OrgUser.CreateRowKey(OrgId, UserId)));
            Assert.That(copy.IsOrgAdmin, Is.True);
            Assert.That(copy.IsAppBuilder, Is.True);
            Assert.That(copy.DefaultRoleId, Is.EqualTo("role-1"));
            Assert.That(copy.Email, Is.EqualTo("Ada@Example.com"));
            Assert.That(record.EmailKey, Is.EqualTo("ADA@EXAMPLE.COM"));
            Assert.That(record.Id.Value,
                Is.EqualTo(OrgMembershipRecord.CreateId(OrgId, UserId).Value));
        }

        [Test]
        public async Task Tombstone_Fails_Closed_Without_Reading_Old_Azure_Record()
        {
            var store = NewStore();
            var reader = NewReader();
            var deleted = OrgMembershipRecord.Deleted(OrgId, UserId, PlatformOrg, null);
            store.Setup(s => s.GetAsync<OrgMembershipRecord>(
                It.IsAny<StorageKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(deleted);

            var repo = NewRepo(store, reader);
            Assert.That(await repo.GetOrgUserAsync(OrgId, UserId), Is.Null);
            Assert.That(await repo.QueryOrgHasUserAsync(OrgId, UserId), Is.False);
            reader.Verify(r => r.GetLegacyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Legacy_Get_Imports_Membership_Once()
        {
            var model = NewMember();
            var store = NewStore();
            var reader = NewReader();
            reader.Setup(r => r.GetLegacyAsync(OrgId, UserId)).ReturnsAsync(model);
            store.Setup(s => s.InsertAsync(It.IsAny<OrgMembershipRecord>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var repo = NewRepo(store, reader);
            var member = await repo.GetOrgUserAsync(OrgId, UserId);
            Assert.That(member.IsOrgAdmin, Is.True);
            store.Verify(s => s.InsertAsync(It.Is<OrgMembershipRecord>(r =>
                r.OrgId == OrgId && r.UserId == UserId && !r.IsDeleted),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Removal_Always_Creates_Tombstone_And_Invalidates_Caches()
        {
            var store = NewStore();
            var reader = NewReader();
            var cache = NewCache();
            store.Setup(s => s.InsertAsync(It.IsAny<OrgMembershipRecord>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await NewRepo(store, reader, cache).RemoveUserFromOrgAsync(
                OrgId, UserId, EntityHeader.Create(UserId, "Operator"));

            store.Verify(s => s.InsertAsync(It.Is<OrgMembershipRecord>(r =>
                r.OrgId == OrgId && r.UserId == UserId && r.IsDeleted),
                It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(c => c.RemoveAsync("ORG_USERS_" + OrgId), Times.Once);
            cache.Verify(c => c.RemoveAsync("org_user_" + OrgId + "_" + UserId), Times.Once);
        }

        [Test]
        public async Task Explicit_Rejoin_Can_Reactivate_Tombstoned_Membership()
        {
            var store = NewStore();
            var reader = NewReader();
            var existing = OrgMembershipRecord.Deleted(OrgId, UserId, PlatformOrg, null);
            store.Setup(s => s.GetAsync<OrgMembershipRecord>(
                It.IsAny<StorageKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(existing);
            store.Setup(s => s.UpdateAsync(It.IsAny<OrgMembershipRecord>(),
                It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            await NewRepo(store, reader).AddOrgUserAsync(NewMember());
            store.Verify(s => s.UpdateAsync(It.Is<OrgMembershipRecord>(r =>
                !r.IsDeleted && r.IsOrgAdmin), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Mongo_Is_Authoritative_Over_Legacy_Membership()
        {
            var store = NewStore();
            var reader = NewReader();
            var persisted = OrgMembershipRecord.FromModel(NewMember(), PlatformOrg);
            persisted.IsOrgAdmin = false;
            store.Setup(s => s.GetAsync<OrgMembershipRecord>(
                It.IsAny<StorageKey>(), It.IsAny<CancellationToken>())).ReturnsAsync(persisted);

            var member = await NewRepo(store, reader).GetOrgUserAsync(OrgId, UserId);
            Assert.That(member.IsOrgAdmin, Is.False);
            reader.Verify(r => r.GetLegacyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public void Mismatched_Membership_Partition_Is_Rejected()
        {
            var model = NewMember();
            model.PartitionKey = "not-our-org";
            Assert.Throws<ArgumentException>(() => OrgMembershipRecord.FromModel(model, PlatformOrg));
        }

        private static OrgUser NewMember()
        {
            return new OrgUser(new NormalizedId32(OrgId), new NormalizedId32(UserId))
            {
                UserName = "Ada",
                Email = "Ada@Example.com",
                IsOrgAdmin = true,
                IsAppBuilder = true,
                DefaultRoleId = "role-1"
            };
        }

        private static Mock<IApplicationDataStore> NewStore() => new Mock<IApplicationDataStore>();
        private static Mock<ILegacyOrgUserReader> NewReader() => new Mock<ILegacyOrgUserReader>();

        private static Mock<ICacheProvider> NewCache()
        {
            var cache = new Mock<ICacheProvider>();
            cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            return cache;
        }

        private static OrgUserRepo NewRepo(Mock<IApplicationDataStore> store,
            Mock<ILegacyOrgUserReader> reader, Mock<ICacheProvider> cache = null)
        {
            var config = new Mock<IAppConfig>();
            config.SetupGet(c => c.SystemOwnerOrg).Returns(PlatformOrg);
            return new OrgUserRepo(store.Object, config.Object, reader.Object,
                (cache ?? NewCache()).Object);
        }
    }
}
