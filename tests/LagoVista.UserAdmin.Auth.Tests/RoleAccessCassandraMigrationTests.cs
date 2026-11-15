using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.UserAdmin.Models.DTOs;
using LagoVista.UserAdmin.Repos.OperationalData.RoleAccess;
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
    public sealed class RoleAccessCassandraMigrationTests
    {
        private const string OrgId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string SystemOrgId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private const string RoleId = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
        private const string ModuleId = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";
        private const string AssignmentId = "EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE";

        [Test]
        public void Public_Role_Assignment_Is_Global_But_Retains_Original_Owner()
        {
            var original = NewDto(true);
            var record = RoleAccessOperationalRecord.FromDTO(original);
            var restored = record.ToDTO();

            Assert.That(record.OrganizationId, Is.EqualTo(RoleAccessOperationalRecord.PublicPartition));
            Assert.That(record.SourceOrganizationId, Is.EqualTo(OrgId));
            Assert.That(restored.PartitionKey, Is.EqualTo(OrgId));
            Assert.That(restored.RowKey, Is.EqualTo(AssignmentId));
            Assert.That(restored.IsPublic, Is.True);
            Assert.That(restored.RoleId, Is.EqualTo(RoleId));
            Assert.That(restored.ModuleId, Is.EqualTo(ModuleId));
            Assert.That(restored.AreaId, Is.EqualTo("area-1"));
            Assert.That(restored.FunctionMapFunctionId, Is.EqualTo("function-1"));
            Assert.That(restored.Create, Is.EqualTo(2));
            Assert.That(restored.Delete, Is.EqualTo(1));
        }

        [Test]
        public void Private_Role_Assignment_Retains_Org_Partition()
        {
            var record = RoleAccessOperationalRecord.FromDTO(NewDto(false));
            Assert.That(record.OrganizationId, Is.EqualTo(OrgId));
            Assert.That(record.IsPublic, Is.False);
            Assert.That(record.ToDTO().RoleKey, Is.EqualTo("role-key"));
        }

        [Test]
        public async Task Removed_Role_Access_Is_Not_ReImported_From_Azure()
        {
            var store = NewStore();
            var old = NewReader();
            var deleted = RoleAccessOperationalRecord.Tombstone(OrgId, AssignmentId);
            store.Setup(s => s.GetAsync(OrgId, AssignmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(deleted);

            // Removed entry is present and cannot be replaced from legacy data.
            await NewRepo(store, old).RemoveRoleAccess(AssignmentId, OrgId);
            old.Verify(r => r.GetLegacyAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            store.Verify(s => s.TryCreateAsync(It.IsAny<RoleAccessOperationalRecord>(),
                It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Role_Access_Revocation_Is_Version_Checked_And_Tombstoned()
        {
            var store = NewStore();
            var old = NewReader();
            var cache = NewCache();
            var active = RoleAccessOperationalRecord.FromDTO(NewDto(true));
            active.Version = 4;
            store.Setup(s => s.GetAsync(OrgId, AssignmentId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((RoleAccessOperationalRecord)null);
            store.Setup(s => s.GetAsync(RoleAccessOperationalRecord.PublicPartition,
                AssignmentId, It.IsAny<CancellationToken>())).ReturnsAsync(active);
            store.Setup(s => s.TryReplaceAsync(It.IsAny<RoleAccessOperationalRecord>(), 4,
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((RoleAccessOperationalRecord candidate, long _, CancellationToken __) =>
                    new ConditionalMutationResult<RoleAccessOperationalRecord>(true, candidate));

            await NewRepo(store, old, cache).RemoveRoleAccess(AssignmentId, OrgId);
            store.Verify(s => s.TryReplaceAsync(It.Is<RoleAccessOperationalRecord>(r =>
                r.IsDeleted && r.IsPublic &&
                r.OrganizationId == RoleAccessOperationalRecord.PublicPartition &&
                r.SourceOrganizationId == OrgId),
                4, It.IsAny<CancellationToken>()), Times.Once);
            cache.Verify(c => c.RemoveAsync("RoleAccess__" + RoleId +
                "__" + RoleAccessOperationalRecord.PublicPartition), Times.Once);
        }

        [Test]
        public async Task Scoped_Permission_Query_Includes_Private_And_Global_Public_Partitions()
        {
            var store = NewStore();
            var old = NewReader();
            var seenPartitions = new List<string>();
            var privateRow = RoleAccessOperationalRecord.FromDTO(NewDto(false));
            var publicDto = NewDto(true);
            publicDto.RowKey = "FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF";
            var publicRow = RoleAccessOperationalRecord.FromDTO(publicDto);
            old.Setup(r => r.GetOrgPageAsync(It.IsAny<string>(), It.IsAny<ListRequest>()))
                .ReturnsAsync((string _, ListRequest request) =>
                    ListResponse<RoleAccessDTO>.Create(new List<RoleAccessDTO>(),
                        request, false, String.Empty, String.Empty));
            old.Setup(r => r.GetPublicLegacyAsync()).ReturnsAsync(new RoleAccessDTO[0]);
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<RoleAccessOperationalRecord>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((StorageQuery<RoleAccessOperationalRecord> query, CancellationToken _) =>
                {
                    var partition = (string)query.Filters.Single(f =>
                        f.Field == "OrganizationId").Value;
                    seenPartitions.Add(partition);
                    return new StoragePageResult<RoleAccessOperationalRecord>(
                        partition == RoleAccessOperationalRecord.PublicPartition ?
                        new[] { publicRow } : new[] { privateRow });
                });

            var roles = await NewRepo(store, old).GetRoleAccessForModuleAsync(ModuleId, OrgId);
            Assert.That(roles.Count, Is.EqualTo(2));
            Assert.That(seenPartitions, Is.EquivalentTo(new[]
            {
                OrgId,
                RoleAccessOperationalRecord.PublicPartition
            }));
        }

        private static RoleAccessDTO NewDto(bool publicAccess)
            => new RoleAccessDTO
            {
                PartitionKey = OrgId,
                RowKey = AssignmentId,
                OrganizationId = OrgId,
                OrganizationName = "Test Org",
                RoleId = RoleId,
                RoleKey = "role-key",
                RoleName = "Editor",
                IsPublic = publicAccess,
                ModuleId = ModuleId,
                ModuleKey = "module",
                ModuleName = "Module",
                AreaId = "area-1",
                AreaKey = "area",
                AreaName = "Area",
                FunctionMapFunctionId = "function-1",
                CreatedById = OrgId,
                CreatedByName = "Operator",
                Create = 2,
                Read = 2,
                Delete = 1
            };

        private static Mock<IConditionalOperationalDataStore<RoleAccessOperationalRecord>> NewStore()
            => new Mock<IConditionalOperationalDataStore<RoleAccessOperationalRecord>>();

        private static Mock<ILegacyRoleAccessReader> NewReader()
            => new Mock<ILegacyRoleAccessReader>();

        private static Mock<ICacheProvider> NewCache()
        {
            var cache = new Mock<ICacheProvider>();
            cache.Setup(c => c.RemoveAsync(It.IsAny<string>())).Returns(Task.CompletedTask);
            return cache;
        }

        private static RoleAccessRepo NewRepo(
            Mock<IConditionalOperationalDataStore<RoleAccessOperationalRecord>> store,
            Mock<ILegacyRoleAccessReader> reader, Mock<ICacheProvider> cache = null)
        {
            var config = new Mock<IAppConfig>();
            config.SetupGet(c => c.SystemOwnerOrg).Returns(
                EntityHeader.Create(SystemOrgId, "Platform"));
            return new RoleAccessRepo(store.Object, reader.Object,
                (cache ?? NewCache()).Object, config.Object);
        }
    }
}
