using LagoVista.CloudStorage.Storage;
using LagoVista.Core.Models;
using LagoVista.UserAdmin.Models.Security;
using LagoVista.UserAdmin.Models.Users;
using LagoVista.UserAdmin.Repos.OperationalData.LocationAssociations;
using LagoVista.UserAdmin.Repos.Security;
using LagoVista.UserAdmin.Repos.Users;
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
    public sealed class LocationAssociationCassandraTests
    {
        private const string LocationId = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA";
        private const string UserId = "BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB";
        private const string Role1 = "CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC";
        private const string Role2 = "DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD";

        [Test]
        public void Location_User_Projections_Preserve_Existing_Identifiers_And_Fields()
        {
            var original = NewLocationUser();
            var primary = LocationUserOperationalRecord.ForLocation(original);
            var reverse = LocationUserOperationalRecord.ForUser(original);
            Assert.That(primary.OrganizationId, Is.EqualTo("location:" + LocationId));
            Assert.That(reverse.OrganizationId, Is.EqualTo("user:" + UserId));
            Assert.That(primary.Id, Is.EqualTo(LocationUser.CreateRowId(LocationId, UserId)));
            Assert.That(reverse.Id, Is.EqualTo(primary.Id));
            Assert.That(reverse.TenantOrganizationId, Is.EqualTo("org-1"));
            Assert.That(primary.ToModel().UsersName, Is.EqualTo("Ada"));
            Assert.That(primary.ToModel().Email, Is.EqualTo("ada@example.com"));
            Assert.That(primary.ToModel().PartitionKey, Is.EqualTo(LocationId));
        }

        [Test]
        public async Task Location_User_Insert_Writes_Both_Lookup_Projections()
        {
            var store = new Mock<IOperationalDataStore<LocationUserOperationalRecord>>();
            var records = new List<LocationUserOperationalRecord>();
            store.Setup(s => s.UpsertAsync(It.IsAny<LocationUserOperationalRecord>(),
                It.IsAny<CancellationToken>()))
                .Callback<LocationUserOperationalRecord, CancellationToken>((record, _) => records.Add(record))
                .Returns(Task.CompletedTask);
            await new LocationUserRepo(store.Object).AddUserToLocationAsync(NewLocationUser());
            Assert.That(records.Select(r => r.OrganizationId),
                Is.EqualTo(new[] { "location:" + LocationId, "user:" + UserId }));
            Assert.That(records.Select(r => r.Id).Distinct().Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task Removed_Location_User_Cannot_Reappear_From_Stale_User_Projection()
        {
            var store = new Mock<IOperationalDataStore<LocationUserOperationalRecord>>();
            var reverse = LocationUserOperationalRecord.ForUser(NewLocationUser());
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<LocationUserOperationalRecord>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StoragePageResult<LocationUserOperationalRecord>(new[] { reverse }));
            store.Setup(s => s.GetAsync("location:" + LocationId, reverse.Id,
                It.IsAny<CancellationToken>())).ReturnsAsync((LocationUserOperationalRecord)null);

            var found = (await new LocationUserRepo(store.Object).GetLocationsForUserAsync(UserId)).ToList();
            Assert.That(found, Is.Empty);
            store.Verify(s => s.GetAsync("location:" + LocationId, reverse.Id,
                It.IsAny<CancellationToken>()), Times.Once());
        }

        [Test]
        public async Task Location_User_Reverse_Lookup_Uses_Authoritative_Record()
        {
            var store = new Mock<IOperationalDataStore<LocationUserOperationalRecord>>();
            var reverse = LocationUserOperationalRecord.ForUser(NewLocationUser());
            var canonical = LocationUserOperationalRecord.ForLocation(NewLocationUser());
            canonical.UsersName = "Updated User Name";
            StorageQuery<LocationUserOperationalRecord> captured = null;
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<LocationUserOperationalRecord>>(),
                It.IsAny<CancellationToken>()))
                .Callback<StorageQuery<LocationUserOperationalRecord>, CancellationToken>((query, _) => captured = query)
                .ReturnsAsync(new StoragePageResult<LocationUserOperationalRecord>(new[] { reverse }));
            store.Setup(s => s.GetAsync("location:" + LocationId, reverse.Id,
                It.IsAny<CancellationToken>())).ReturnsAsync(canonical);

            var found = (await new LocationUserRepo(store.Object).GetLocationsForUserAsync(UserId)).ToList();
            Assert.That(found.Single().UsersName, Is.EqualTo("Updated User Name"));
            Assert.That(captured.Filters.Single().Field, Is.EqualTo("OrganizationId"));
            Assert.That(captured.Filters.Single().Value, Is.EqualTo("user:" + UserId));
        }

        [Test]
        public async Task Removal_Deletes_Authoritative_Association_Before_Reverse_Projection()
        {
            var store = new Mock<IOperationalDataStore<LocationUserOperationalRecord>>();
            var removed = new List<string>();
            store.Setup(s => s.DeleteAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
                .Callback<string, string, CancellationToken>((partition, _, __) => removed.Add(partition))
                .Returns(Task.CompletedTask);
            await new LocationUserRepo(store.Object).RemoveUserFromLocationAsync(
                LocationId, UserId, EntityHeader.Create(UserId, "Operator"));
            Assert.That(removed, Is.EqualTo(new[] { "location:" + LocationId, "user:" + UserId }));
        }

        [Test]
        public void Location_Role_Identity_Distinguishes_Roles_Granted_To_Same_User()
        {
            var one = NewLocationRole(Role1);
            var two = NewLocationRole(Role2);
            var a = LocationRoleOperationalRecord.FromModel(one);
            var b = LocationRoleOperationalRecord.FromModel(two);
            Assert.That(a.Id, Is.Not.EqualTo(b.Id));
            Assert.That(a.OrganizationId, Is.EqualTo(LocationId));
            Assert.That(a.ToModel().RowKey, Does.EndWith("." + Role1));
            Assert.That(b.ToModel().RowKey, Does.EndWith("." + Role2));
        }

        [Test]
        public async Task Role_Confirmation_Does_Not_Accept_Different_User_Or_Role()
        {
            var store = new Mock<IConditionalOperationalDataStore<LocationRoleOperationalRecord>>();
            var existing = LocationRoleOperationalRecord.FromModel(NewLocationRole(Role1));
            store.Setup(s => s.GetAsync(LocationId, existing.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            var repo = new LocationRoleRepo(store.Object);
            Assert.That(await repo.ConfirmUserInRoleAsync(LocationId, UserId, Role1), Is.True);
            Assert.That(await repo.ConfirmUserInRoleAsync(LocationId, UserId, Role2), Is.False);
        }

        [Test]
        public async Task Role_Query_Is_Partitioned_By_Location_And_Uses_Indexed_Filter()
        {
            var store = new Mock<IConditionalOperationalDataStore<LocationRoleOperationalRecord>>();
            var item = LocationRoleOperationalRecord.FromModel(NewLocationRole(Role1));
            var queries = new List<StorageQuery<LocationRoleOperationalRecord>>();
            store.Setup(s => s.QueryAsync(It.IsAny<StorageQuery<LocationRoleOperationalRecord>>(),
                It.IsAny<CancellationToken>()))
                .ReturnsAsync((StorageQuery<LocationRoleOperationalRecord> query, CancellationToken _) =>
                {
                    queries.Add(query);
                    return new StoragePageResult<LocationRoleOperationalRecord>(new[] { item });
                });
            var repo = new LocationRoleRepo(store.Object);
            var assigned = (await repo.GetRolesForUserInLocationAsync(LocationId, UserId)).ToList();
            var roleUsers = (await repo.GetUsersInRoleForLocationAsync(Role1, LocationId)).ToList();
            Assert.That(assigned.Count, Is.EqualTo(1));
            Assert.That(roleUsers.Count, Is.EqualTo(1));
            Assert.That(queries.All(q => q.Filters.Any(f =>
                f.Field == "OrganizationId" && (string)f.Value == LocationId)), Is.True);
            Assert.That(queries[0].Filters.Any(f => f.Field == "UserId" &&
                (string)f.Value == UserId), Is.True);
            Assert.That(queries[1].Filters.Any(f => f.Field == "RoleId" &&
                (string)f.Value == Role1), Is.True);
        }

        [Test]
        public async Task Role_Revocation_Uses_Version_Checked_Cassandra_Delete()
        {
            var store = new Mock<IConditionalOperationalDataStore<LocationRoleOperationalRecord>>();
            var record = LocationRoleOperationalRecord.FromModel(NewLocationRole(Role1));
            record.Version = 3;
            store.Setup(s => s.GetAsync(LocationId, record.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(record);
            store.Setup(s => s.TryDeleteAsync(LocationId, record.Id, 3, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ConditionalMutationResult<LocationRoleOperationalRecord>(true, null));
            await new LocationRoleRepo(store.Object).RevokeRoleForUserInLocationAsync(
                LocationId, UserId, Role1);
            store.Verify(s => s.TryDeleteAsync(LocationId, record.Id, 3,
                It.IsAny<CancellationToken>()), Times.Once());
        }

        private static LocationUser NewLocationUser()
        {
            return new LocationUser("org-1", LocationId, UserId)
            {
                UsersName = "Ada",
                LocationName = "Main",
                Email = "ada@example.com"
            };
        }

        private static LocationUserRole NewLocationRole(string roleId)
        {
            return new LocationUserRole(EntityHeader.Create(LocationId, "Main"),
                EntityHeader.Create(UserId, "Ada"))
            {
                RoleId = roleId,
                RoleName = "Operator"
            };
        }
    }
}
