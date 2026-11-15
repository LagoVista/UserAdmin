using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Repos.OperationalData.UserRoles;
using LagoVista.UserAdmin.Repos.Repos.Security;
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
    public sealed class UserRoleCassandraMigrationTests
    {
        private const string OrganizationId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string UserId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private const string RoleId = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
        private const string AssignmentId = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";

        [Test]
        public void Role_Record_Round_Trip_Preserves_Authorization_Fields()
        {
            var dto = NewRole();
            var record = UserRoleOperationalRecord.FromDTO(dto);
            var copy = record.ToDTO();
            Assert.That(record.OrganizationId, Is.EqualTo(OrganizationId));
            Assert.That(record.Id, Is.EqualTo(AssignmentId));
            Assert.That(copy.RowKey, Is.EqualTo(dto.RowKey));
            Assert.That(copy.UserId, Is.EqualTo(UserId));
            Assert.That(copy.RoleId, Is.EqualTo(RoleId));
            Assert.That(copy.RoleKey, Is.EqualTo("editor"));
            Assert.That(copy.OrganizationName, Is.EqualTo("Test Org"));
            Assert.That(copy.CreatedByName, Is.EqualTo("Operator"));
            Assert.That(record.IsDeleted, Is.False);
            Assert.That(UserRoleOperationalRecord.Tombstone(OrganizationId, AssignmentId).ToDTO(), Is.Null);
        }

        [Test]
        public async Task Deleted_Role_Cannot_Be_Reimported_From_Old_Azure_Row()
        {
            var store = NewStore();
            var old = NewReader();
            var tombstone = UserRoleOperationalRecord.Tombstone(OrganizationId, AssignmentId);
            store.Setup(s => s.GetAsync(OrganizationId, AssignmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(tombstone);

            var found = await NewRepo(store, old).GetRoleAssignmentAsync(AssignmentId, OrganizationId);
            Assert.That(found, Is.Null);
            old.Verify(r => r.GetLegacyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task Legacy_Role_Is_Imported_Using_Create_If_Absent()
        {
            var store = NewStore();
            var old = NewReader();
            var record = UserRoleOperationalRecord.FromDTO(NewRole());
            old.Setup(r => r.GetLegacyAsync(OrganizationId, AssignmentId))
                .ReturnsAsync(NewRole());
            store.Setup(s => s.TryCreateAsync(It.IsAny<UserRoleOperationalRecord>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserRoleOperationalRecord proposed, CancellationToken _) =>
                    new ConditionalMutationResult<UserRoleOperationalRecord>(true, proposed));

            var found = await NewRepo(store, old).GetRoleAssignmentAsync(AssignmentId, OrganizationId);
            Assert.That(found.Role.Id, Is.EqualTo(RoleId));
            store.Verify(s => s.TryCreateAsync(It.Is<UserRoleOperationalRecord>(r =>
                r.OrganizationId == OrganizationId && r.Id == AssignmentId),
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task Revocation_Writes_Permanent_Tombstone_Via_CAS()
        {
            var store = NewStore();
            var old = NewReader();
            var cache = NewCache();
            var record = UserRoleOperationalRecord.FromDTO(NewRole());
            record.Version = 3;
            store.Setup(s => s.GetAsync(OrganizationId, AssignmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);
            store.Setup(s => s.TryReplaceAsync(It.IsAny<UserRoleOperationalRecord>(), 3,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((UserRoleOperationalRecord proposed, long _, CancellationToken __) =>
                    new ConditionalMutationResult<UserRoleOperationalRecord>(true, proposed));

            await NewRepo(store, old, cache).RemoveUserRole(AssignmentId, OrganizationId);
            store.Verify(s => s.TryReplaceAsync(It.Is<UserRoleOperationalRecord>(r =>
                r.IsDeleted && r.UserId == UserId && r.Id == AssignmentId),
                3, It.IsAny<CancellationToken>()), Times.Once());
            cache.Verify(c => c.RemoveAsync("user_role_cache_" + UserId +
                "_" + OrganizationId + "_v2"), Times.Once());
        }

        [Test]
        public async Task Query_Uses_Organization_Partition_And_User_Index()
        {
            var store = NewStore();
            var old = NewReader();
            old.Setup(r => r.GetRolesForUserLegacyAsync(UserId, OrganizationId))
                .ReturnsAsync(new UserRoleDTO[0]);
            StorageQuery<UserRoleOperationalRecord> captured = null;
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<UserRoleOperationalRecord>>(),
                It.IsAny<CancellationToken>()))
                .Callback<StorageQuery<UserRoleOperationalRecord>, CancellationToken>((q, _) => captured = q)
                .ReturnsAsync(new StoragePageResult<UserRoleOperationalRecord>(
                    new[] { UserRoleOperationalRecord.FromDTO(NewRole()) }));

            var roles = await NewRepo(store, old).GetRolesForUserAsync(UserId, OrganizationId);
            Assert.That(roles.Single().Role.Id, Is.EqualTo(RoleId));
            Assert.That(captured.Filters.Any(f => f.Field == "OrganizationId" &&
                (string)f.Value == OrganizationId), Is.True);
            Assert.That(captured.Filters.Any(f => f.Field == "UserId" &&
                (string)f.Value == UserId), Is.True);
        }

        private static UserRoleDTO NewRole() => new UserRoleDTO
        {
            PartitionKey = OrganizationId,
            RowKey = AssignmentId,
            OrganizationId = OrganizationId,
            OrganizationName = "Test Org",
            UserId = UserId,
            UserName = "Ada",
            RoleId = RoleId,
            RoleKey = "editor",
            RoleName = "Editor",
            CreatedById = UserId,
            CreatedByName = "Operator"
        };

        private static Mock<IConditionalOperationalDataStore<UserRoleOperationalRecord>> NewStore()
            => new Mock<IConditionalOperationalDataStore<UserRoleOperationalRecord>>();

        private static Mock<ILegacyUserRoleReader> NewReader()
            => new Mock<ILegacyUserRoleReader>();

        private static Mock<ICacheProvider> NewCache()
        {
            var cache = new Mock<ICacheProvider>();
            cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            return cache;
        }

        private static UserRoleRepo NewRepo(
            Mock<IConditionalOperationalDataStore<UserRoleOperationalRecord>> store,
            Mock<ILegacyUserRoleReader> old, Mock<ICacheProvider> cache = null)
            => new UserRoleRepo(store.Object, old.Object, (cache ?? NewCache()).Object);
    }
}
